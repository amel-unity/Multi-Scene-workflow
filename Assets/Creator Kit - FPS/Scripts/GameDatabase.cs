using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Application = UnityEngine.Application;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditorInternal;
#endif


/// <summary>
/// This store multiple data related to the game like the type of Ammo, the list of levels etc...
/// When access in the editor, if it don't exit it create an instance in the Resources folder, otherwise it load it
/// for modification.
/// </summary>
public class GameDatabase : ScriptableObject
{
#if UNITY_EDITOR
    static GameDatabase()
    {
        BuildPlayerWindow.RegisterGetBuildPlayerOptionsHandler(HandleBuild);

        EditorApplication.playModeStateChanged += change =>
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                var db = GameDatabase.Instance;
                foreach (var ep in db.episodes)
                {
                    ep.UpdateScenePath();
                }
            }
        };
    }

    static BuildPlayerOptions HandleBuild(BuildPlayerOptions opts)
    {
        opts = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(opts);
        
        List<string> buildSettingScenes = new List<string>(opts.scenes);

        var db = GameDatabase.Instance;
        foreach (var ep in db.episodes)
        {
            ep.UpdateScenePath();
            foreach (var s in ep.scenes)
            {
                if (!string.IsNullOrEmpty(s) && !buildSettingScenes.Contains(s))
                {
                    Debug.Log($"Added scene {s} to build");
                    buildSettingScenes.Add(s);
                }
            }
        }
        
       

       opts.scenes = buildSettingScenes.ToArray();

       return opts;
    }
    
#endif
    
    public static GameDatabase Instance
    {
        get
        {
            if (s_Instance == null)
            {
                var db = Resources.Load<GameDatabase>("GameDatabase");

                if (db == null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        db = CreateInstance<GameDatabase>();

                        if (!System.IO.Directory.Exists(Application.dataPath + "/Resources"))
                            AssetDatabase.CreateFolder("Assets", "Resources");
                        
                        AssetDatabase.CreateAsset(db, "Assets/Resources/GameDatabase.asset");
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.LogError("Game Database couldn't be found.");
                        return null;
                    }
#else
                    Debug.LogError("Game Database couldn't be found.");
                    return null;
#endif
                }

                s_Instance = db;
            }

            return s_Instance;
        }
    }

    static GameDatabase s_Instance;

    public AmmoDatabase ammoDatabase;
    public Episode[] episodes;
    
}

[System.Serializable]
public class AmmoDatabase
{
    public int maxId = 0;
    public Queue<int> freeID = new Queue<int>();

    [System.Serializable]
    public class Entry
    {
        public string name;
        public int id;
    }

    public Entry[] entries;

#if  UNITY_EDITOR
    public Entry AddEntry(string name)
    {
        Entry e = new Entry();
        
        if(freeID.Count > 0)
            e.id = freeID.Dequeue();
        else
        {
            e.id = maxId;
            maxId++;
        }

        e.name = name;

        ArrayUtility.Add(ref entries, e);

        return e;
    }
    
    public void RemoveEntry(Entry e)
    {
        freeID.Enqueue(e.id);
        ArrayUtility.Remove(ref entries, e);
    }
#endif

    public Entry GetEntry(string name)
    {
        return entries.First(entry => entry.name == name);
    }

    public Entry GetEntry(int id)
    {
        return entries.First(entry => entry.id == id);
    }
}

[System.Serializable]
public class Episode
{
    public string title = "Episode";
    
#if UNITY_EDITOR
    public SceneAsset[] sceneAssets = new SceneAsset[0];

    public void UpdateScenePath()
    {
        bool needUpdating = false;
        string[] scenePaths = new string[sceneAssets.Length];
        
        for (int i = 0; i < sceneAssets.Length; ++i)
        {
            scenePaths[i] = AssetDatabase.GetAssetPath(sceneAssets[i]);

            if (scenes.Length <= i || scenes[i] != scenePaths[i])
                needUpdating = true;
        }

        //doing this allow to avoid dirtying the assets when the scenes haven't changed, better for source control.
        if (needUpdating)
            scenes = scenePaths;
    }
#endif
    
    public string[] scenes = new string[0];
}

#if UNITY_EDITOR

public class DBEditor : EditorWindow
{
    GameDatabase db;

    int m_EditedCategory = 0;
    string[] m_Categories = new[] { "Episodes", "Ammo Type" };

    ReorderableList[] m_ReorderableLists = new ReorderableList[0];
    SerializedObject m_SerializedObject;
    
    [MenuItem("FPSKIT/Game Database")]
    static void Open()
    {
        GetWindow<DBEditor>();
    }

    void OnEnable()
    {
        db = GameDatabase.Instance;
        m_SerializedObject = new SerializedObject(db);

        var serializedProperty = m_SerializedObject.FindProperty("episodes");
        int idx = 0;
        
        foreach (var e in db.episodes)
        {
            var itm = serializedProperty.GetArrayElementAtIndex(idx);
            var listProp = itm.FindPropertyRelative("sceneAssets");
            
            ReorderableList list = new ReorderableList(m_SerializedObject, listProp);
            ArrayUtility.Add(ref m_ReorderableLists, list);

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var elem = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, elem, new GUIContent("Scene"));
            };

            idx++;
        }
    }

    void OnGUI()
    {
        m_EditedCategory = GUILayout.Toolbar(m_EditedCategory, m_Categories);

        switch (m_EditedCategory)
        {
            case 0:
                EpisodeEditor();
                break;
            case 1 :
                AmmoDatabaseEditor();
                break;
            default:
                break;
        }
    }

    void EpisodeEditor()
    {
        int toDelete = -1;
        for (int i = 0; i < db.episodes.Length; ++i)
        {
            EditorGUILayout.BeginHorizontal();

            db.episodes[i].title = EditorGUILayout.TextField("Title", db.episodes[i].title);
            
            if(i != 0)
            {
                if(GUILayout.Button("↑", GUILayout.Width(32)))
                {
                    Undo.RecordObject(db, "Changed episode order");
                    var e = db.episodes[i-1];
                    db.episodes[i - 1] = db.episodes[i];
                    db.episodes[i] = e;

                    var l = m_ReorderableLists[i - 1];
                    m_ReorderableLists[i - 1] = m_ReorderableLists[i];
                    m_ReorderableLists[i] = l;
                    EditorUtility.SetDirty(db);
                }
            }
            else
            {
                GUILayout.Label("", GUILayout.Width(32));
            }
            
            if(i < db.episodes.Length-1)
            {
                if(GUILayout.Button("↓", GUILayout.Width(32)))
                {
                    Undo.RecordObject(db, "Changed episode order");
                    var e = db.episodes[i + 1];
                    db.episodes[i + 1] = db.episodes[i];
                    db.episodes[i] = e;
                    
                    var l = m_ReorderableLists[i + 1];
                    m_ReorderableLists[i + 1] = m_ReorderableLists[i];
                    m_ReorderableLists[i] = l;
                    
                    EditorUtility.SetDirty(db);
                }
            }
            else
            {
                GUILayout.Label("", GUILayout.Width(32));
            }

            if (GUILayout.Button("-", GUILayout.Width(32)))
            {
                toDelete = i;
            }

            EditorGUILayout.EndHorizontal();

            m_ReorderableLists[i].DoLayoutList();
        }

        if (toDelete != -1)
        {
            Undo.RecordObject(db, "Removed episode");
            ArrayUtility.RemoveAt(ref db.episodes, toDelete);
            ArrayUtility.RemoveAt(ref m_ReorderableLists, toDelete);
            EditorUtility.SetDirty(db);
        }

        if (GUILayout.Button("New Episode"))
        {
            var episode = new Episode();
            
            Undo.RecordObject(db, "Added episode");
            ArrayUtility.Add(ref db.episodes, episode);
            
            m_SerializedObject.Update();
            
            var serializedProperty = m_SerializedObject.FindProperty("episodes");
            var itm = serializedProperty.GetArrayElementAtIndex(db.episodes.Length - 1);
            var listProp = itm.FindPropertyRelative("sceneAssets");
            
            var list = new ReorderableList(m_SerializedObject, listProp);
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var elem = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, elem, new GUIContent("Scene"));
            };
            
            ArrayUtility.Add(ref m_ReorderableLists, list);
        }

        m_SerializedObject.ApplyModifiedProperties();
    }

    void AmmoDatabaseEditor()
    {
        AmmoDatabase.Entry todelete = null;
        for (int i = 0; i < db.ammoDatabase.entries.Length; ++i)
        {
            GUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            db.ammoDatabase.entries[i].name = GUILayout.TextField(db.ammoDatabase.entries[i].name);
            if(EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(db);
            
            if (GUILayout.Button("-", GUILayout.Width(64)))
            {
                todelete = db.ammoDatabase.entries[i];
            }
            GUILayout.EndHorizontal();
        }

        if (todelete != null)
        {
            ArrayUtility.Remove(ref db.ammoDatabase.entries, todelete);
        }

        if (GUILayout.Button("Add Ammo Type"))
        {
            db.ammoDatabase.AddEntry("Ammo");
            EditorUtility.SetDirty(db);
        }
    }
}
#endif
