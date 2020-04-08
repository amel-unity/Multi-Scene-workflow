using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LevelLayout : MonoBehaviour
{
    public LevelRoom[] rooms = new LevelRoom[0];
     
    //Each piece when they are destroyed try to update the pieces array, problem is if the system is destroyed, that
    //spawned an assert. So we use that bool to know if the system is getting destroyed.
    public bool Destroyed { get; private set; }
    
    void OnDestroy()
    {
        Destroyed = true;
    }

//This is a small hack to go around the fact that hideFlag change on prefab instance (like the room) does not get saved
//in the scene. So we make sure (only in editor) to hide all the room manually every frame
#if UNITY_EDITOR
    void Update()
    {
        if(Application.isPlaying)
            return;
        
        foreach (var room in rooms)
        {
            room.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelLayout))]
public class LevelLayoutEditor : Editor
{
    LevelLayout m_LevelLayout;
    
    bool m_EditingLayout = false;
    Material m_HighlightMaterial;
    int m_EditingMode = 0;
    
    List<LevelRoomGroup> m_AvailablesPalettes = new List<LevelRoomGroup>();

    LevelRoomGroup m_CurrentGroup = null;

    Vector2 m_PaletteSelectionScroll;
    Vector2 m_ObjectSelectScroll;

    LevelRoom m_SelectedRoom = null;   
    LevelRoom m_CurrentInstance = null;
    GameObject m_SelectedPrefab;
    int m_CurrentUsedExit = 0;
    
    Vector3 m_CurrentScale = Vector3.one;
    
    SerializedProperty m_PieceProperty;
    
    void OnEnable()
    {
        m_LevelLayout = target as LevelLayout;
        
        var assets = AssetDatabase.FindAssets("t:LevelRoomGroup");
        foreach (var a in assets)
        {
            string path = AssetDatabase.GUIDToAssetPath(a);
            LevelRoomGroup palette = AssetDatabase.LoadAssetAtPath<LevelRoomGroup>(path);
            
            m_AvailablesPalettes.Add(palette);
        }

        m_PieceProperty = serializedObject.FindProperty("rooms");

        m_HighlightMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

        m_HighlightMaterial.color = new Color32(255,238,0, 255);
        m_HighlightMaterial.SetInt("ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        
        EditorApplication.playModeStateChanged += PlayModeChange;
    }

    void OnDisable()
    {
        Clean();       
        EditorApplication.playModeStateChanged -= PlayModeChange;
    }
    
    void Clean()
    {
        if (m_CurrentInstance != null)
        {
            DestroyImmediate(m_CurrentInstance.gameObject);
            m_CurrentInstance = null;
        }
    }

    public override void OnInspectorGUI()
    {
        bool editing = GUILayout.Toggle(m_EditingLayout, "Editing Layout", "Button");

        if (editing != m_EditingLayout)
        {
            if (!editing)
            {//disabled editing, cleanup
                if(m_CurrentInstance != null)
                    DestroyImmediate(m_CurrentInstance.gameObject);
                m_CurrentGroup = null;
                m_CurrentInstance = null;
                m_SelectedRoom = null;
            }
            else
            {
                if (!SceneView.lastActiveSceneView.drawGizmos)
                {
                    if (EditorUtility.DisplayDialog("Warning", "Gizmos are globally disabled, which prevents the layout editing tools from working. Do you want to re-enable Gizmos?", "Yes", "No"))
                    {
                        SceneView.lastActiveSceneView.drawGizmos = true;
                    }
                    else
                    {
                        editing = false;
                    }
                }
            }

            m_EditingLayout = editing;
        }

        if (m_EditingLayout)
        {
            EditorGUILayout.HelpBox("Press R to change which door the room use to connect to other room", MessageType.Info);
            
            EditorGUILayout.BeginHorizontal();

            int editingMode = GUILayout.Toolbar(m_EditingMode, new[] { "Add", "Remove" }, GUILayout.Width(120));
            if (editingMode != m_EditingMode)
            {
                if (editingMode == 1)
                {
                    if(m_CurrentInstance != null)
                        DestroyImmediate(m_CurrentInstance.gameObject);

                    m_SelectedRoom = null;
                }

                m_EditingMode = editingMode;
            }

            if (m_CurrentInstance != null)
            {
                EditorGUILayout.LabelField("Flip : ", GUILayout.Width(32));
                EditorGUI.BeginChangeCheck();

                bool flipX = GUILayout.Toggle(m_CurrentScale.x < 0, "X", "button", GUILayout.Width(32));
                bool flipY = GUILayout.Toggle(m_CurrentScale.y < 0, "Y", "button", GUILayout.Width(32));
                bool flipZ = GUILayout.Toggle(m_CurrentScale.z < 0, "Z", "button", GUILayout.Width(32));

                GUILayout.FlexibleSpace();

                if (EditorGUI.EndChangeCheck())
                {
                    m_CurrentScale = new Vector3(flipX ? -1 : 1, flipY ? -1 : 1, flipZ ? -1 : 1);
                    m_CurrentInstance.transform.localScale = m_CurrentScale;
                }   
            }
            
            EditorGUILayout.EndHorizontal();
            
            //we repaint all scene view to be sure they get a notification so they can "steal" focus in edit mode
            SceneView.RepaintAll();
        }
        
        GUILayout.BeginHorizontal();
        
        GUILayout.BeginVertical();
        GUILayout.Label("Group");
        GUILayout.BeginScrollView(m_PaletteSelectionScroll);
        
        foreach (var p in m_AvailablesPalettes)
        {
            GUI.enabled = m_CurrentGroup != p;
            if (GUILayout.Button(p.name))
            {
                Debug.Log($"clicked on {p.name}");
                if (!m_EditingLayout)
                {
                    if (!SceneView.lastActiveSceneView.drawGizmos)
                    {
                        if (EditorUtility.DisplayDialog("Warning", "Gizmos are globally disabled, which prevent the layout editing tool to work. Do you want to re-enable Gizmos?", "Yes", "No"))
                        {
                            SceneView.lastActiveSceneView.drawGizmos = true;
                            m_EditingLayout = true;
                        }
                    }
                    else
                        m_EditingLayout = true;
                }
                
                if(m_EditingLayout)
                    m_CurrentGroup = p;
            }
        }

        GUI.enabled = true;

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        
        m_PaletteSelectionScroll = GUILayout.BeginScrollView(m_PaletteSelectionScroll, GUILayout.Width(72*3));
        GUILayout.BeginVertical();

        if (m_CurrentGroup != null)
        {
            bool horizontalOpen = false;

            for (int i = 0; i < m_CurrentGroup.levelPart.Length; ++i)
            {
                LevelRoom part = m_CurrentGroup.levelPart[i];
                
                if (i % 3 == 0 && i != 0)
                {
                    GUILayout.EndHorizontal();
                    horizontalOpen = false;
                }
                
                if (!horizontalOpen)
                {
                    GUILayout.BeginHorizontal();
                    horizontalOpen = true;
                }
                
                Texture2D preview = AssetPreview.GetAssetPreview(part.gameObject);

                GUI.enabled = part != m_SelectedRoom;
                if (GUILayout.Button(preview, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    m_SelectedRoom = part;
                    
                    if(m_CurrentInstance != null)
                        DestroyImmediate(m_CurrentInstance.gameObject);

                    m_CurrentInstance = Instantiate(m_SelectedRoom, m_LevelLayout.transform);                                       
                    m_CurrentInstance.gameObject.isStatic = false;
                    m_CurrentInstance.gameObject.tag = "EditorOnly";
                    m_CurrentInstance.name = "TempInstance";
                    m_CurrentUsedExit = 0;
                    
                    m_CurrentInstance.transform.localScale = m_CurrentScale;
                    
                    m_EditingMode = 0;
                }
            }
            
            if(horizontalOpen)
                GUILayout.EndHorizontal();
        }

        GUI.enabled = true;
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        
        GUILayout.EndHorizontal();
    }

       void OnSceneGUI()
    {
        if (m_EditingLayout)
        {
            if (m_EditingMode == 0)
            {
                AddPiece();
            }
            else
            {
                RemovePiece();
            }
        }
    }

     void RemovePiece()
    {
        int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
        
        if(GUIUtility.hotControl == 0)
            HandleUtility.AddDefaultControl(controlID);
        
        if (m_CurrentInstance != null)
        {
            m_CurrentInstance.gameObject.SetActive(false);
        }
        
        var mousePos = Event.current.mousePosition;
        LevelRoom closestPiece = null;
        Bounds closestBound = new Bounds();
               
        float closestSqrDist = float.MaxValue;
        for (int i = 0; i < m_LevelLayout.rooms.Length; ++i)
        {
            LevelRoom r = m_LevelLayout.rooms[i];           
                    
            if(r == null)
                continue;
            
            //This bit is inneficient, but should be enough for our purpose here in the kit. In very big scene
            //it could slow down the editing process. Bound should probably be stored in local space or better should
            //find a way to use the built-in picking but that require more complexity than necessary for those small kit
            Bounds b = new Bounds();
            bool init = false;
            
            MeshRenderer[] renderers = r.GetComponentsInChildren<MeshRenderer>();

            if (renderers.Length > 0)
            {
                for (int k = 0; k < renderers.Length; ++k)
                {          
                    if (!init)
                    {
                        b = renderers[k].bounds;
                        init = true;
                    }
                    else
                    {
                        b.Encapsulate(renderers[k].bounds);
                    }
                }
            }
            else
            {
                //if the piece got no collider, it may be an "empty" piece used to introduce gap, so instead look for
                //a collider to find it's size.
                Collider[] colliders = r.GetComponentsInChildren<Collider>();
                for (int k = 0; k < colliders.Length; ++k)
                {
                    if (k == 0)
                    {
                        b = colliders[k].bounds;
                    }
                    else
                    {
                        b.Encapsulate(colliders[k].bounds);
                    }
                }
            }

            var guiPts = HandleUtility.WorldToGUIPoint(b.center);
            
            Handles.DrawWireDisc(b.center, Vector3.up, 0.5f);

            float dist = (guiPts - mousePos).sqrMagnitude;

            if (dist < closestSqrDist)
            {
                closestSqrDist = dist;
                closestPiece = r;
                closestBound = b;
            }
        }

        if (closestPiece != null)
        {
            if (Event.current.type == EventType.Repaint)
            {
                MeshFilter[] filter = closestPiece.GetComponentsInChildren<MeshFilter>();

                if (filter != null)
                {
                    m_HighlightMaterial.SetPass(0);
                    foreach (var f in filter)
                    {
                        if (f.sharedMesh == null)
                            continue;
                        
                        Graphics.DrawMeshNow(f.sharedMesh, f.transform.localToWorldMatrix);
                    }
                }
               
                Handles.DrawWireCube(closestBound.center, closestBound.size);
            }
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && GUIUtility.hotControl == 0)
            {
                closestPiece.Removed();
                Undo.DestroyObjectImmediate(closestPiece.gameObject);
            }
            else
            {
                SceneView.currentDrawingSceneView.Repaint();
            }
        }
    }

    void AddPiece()
    {
        if(m_CurrentInstance == null)
            return;
        
        m_CurrentInstance.gameObject.SetActive(true);
        
        //Since the scene view is not having focus after we choose a new room, pressing R won't rotate it until
        //we click on the scene view. So we force focus on windows. But we only do it if the cursor is above the
        //scene view otherwise we mess focus on OSX with the scene view always stealing focus from other app
        if(SceneView.currentDrawingSceneView.position.Contains(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)))
            SceneView.currentDrawingSceneView.Focus();
            
        int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            
        if(GUIUtility.hotControl == 0)
            HandleUtility.AddDefaultControl(controlID);
        
        if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.R)
            {
                m_CurrentUsedExit = m_CurrentUsedExit + 1 >= m_CurrentInstance.Exits.Length ? 0 : m_CurrentUsedExit + 1;
            }
        }
        
        
        LevelRoom currentClosestPiece = null;
        int currentClosestExit = -1;
        
        if (m_LevelLayout.rooms.Length == 0)
        {//if we have no piece, we force the instance in 0,0,0, as it's the seed piece
            m_CurrentInstance.transform.position = m_LevelLayout.transform.TransformPoint(Vector3.zero);
        }
        else
        {
            var mousePos = Event.current.mousePosition;
           
            float closestSqrDist = float.MaxValue;
            for (int i = 0; i < m_LevelLayout.rooms.Length; ++i)
            {
                LevelRoom r = m_LevelLayout.rooms[i];
                
                if(r == null)
                    continue;
                
                for (int k = 0; k < r.Exits.Length; ++k)
                {
                    if (r.ExitDestination[k] != null)
                        continue;
                    
                    var guiPts = HandleUtility.WorldToGUIPoint(r.Exits[k].transform.position);
        
                    float dist = (guiPts - mousePos).sqrMagnitude;
        
                    if (dist < closestSqrDist)
                    {
                        closestSqrDist = dist;
                        currentClosestPiece = r;
                        currentClosestExit = k;
                    }
                }
            }
        
            if (currentClosestPiece != null)
            {
                m_CurrentInstance.transform.rotation = Quaternion.identity;
                
                Transform closest = currentClosestPiece.Exits[currentClosestExit];
                Transform usedExit = m_CurrentInstance.Exits[m_CurrentUsedExit];
        
                Quaternion targetRotation = Quaternion.LookRotation(-closest.forward, closest.up);
                Quaternion difference = targetRotation * Quaternion.Inverse(usedExit.rotation);
                
                Quaternion rotation = m_CurrentInstance.transform.rotation * difference;
                m_CurrentInstance.transform.rotation = rotation;
        
                m_CurrentInstance.transform.position = closest.position + m_CurrentInstance.transform.TransformVector(-usedExit.transform.localPosition);
            }
        }
        
        
        //if hot control is not 0, that mean we clicked a gizmo and we don't want that.
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && GUIUtility.hotControl == 0)
        {           
            var c = PrefabUtility.InstantiatePrefab(m_SelectedRoom) as LevelRoom;
               
            int i = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Added new piece");
            
            Undo.RegisterCreatedObjectUndo(c.gameObject, "Added new piece");
        
            c.gameObject.hideFlags = HideFlags.HideInHierarchy;
            c.transform.SetParent(m_LevelLayout.transform, false);
            
            c.transform.position = m_CurrentInstance.transform.position;
            c.transform.rotation = m_CurrentInstance.transform.rotation;
            c.transform.localScale = m_CurrentInstance.transform.localScale;
            
            c.name = m_SelectedRoom.gameObject.name;
            c.gameObject.isStatic = true;
            
            c.Placed(m_LevelLayout);
            
            m_PieceProperty.serializedObject.Update();
            
            m_PieceProperty.InsertArrayElementAtIndex(m_PieceProperty.arraySize);
            m_PieceProperty.GetArrayElementAtIndex(m_PieceProperty.arraySize - 1).objectReferenceValue = c;
        
            if (currentClosestPiece != null)
            {
                Snap(currentClosestPiece, c, currentClosestExit, m_CurrentUsedExit);
                
                //go through all remaining exit and will find if it is close to another to link it
                for (int k = 0; k < c.Exits.Length; ++k)
                {
                    if (k == m_CurrentUsedExit)
                        continue;
        
                    bool exitLinked = false;
                    Transform testedExit = c.Exits[k];
        
                    for (int r = 0; r < m_LevelLayout.rooms.Length && !exitLinked; ++r)
                    {
                        for (int re = 0; re < m_LevelLayout.rooms[r].Exits.Length; ++re)
                        {
                            //this is an already used exit no need to test here
                            if(m_LevelLayout.rooms[r].ExitDestination[re] != null)
                                continue;
        
                            //if we are close enough, let's consider those 2 exit Linked.
                            if (Vector3.SqrMagnitude(m_LevelLayout.rooms[r].Exits[re].position - testedExit.position) < 0.2f * 0.2f)
                            {
                                Snap(m_LevelLayout.rooms[r], c, re, k);
                                exitLinked = true;
                                break;
                            }
                        }
                    }
                }
            }
        
            Undo.CollapseUndoOperations(i);
        
            m_PieceProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    static void Snap(LevelRoom A, LevelRoom B, int exitA, int exitB)
    {
        SerializedObject newObj = new SerializedObject(A);
        SerializedObject currentObj = new SerializedObject(B);
                    
        var propNew = newObj.FindProperty("ExitDestination");
        var propCurrent = currentObj.FindProperty("ExitDestination");
                    
        newObj.Update();
        currentObj.Update();

        propCurrent.GetArrayElementAtIndex(exitB).objectReferenceValue = A;
        propNew.GetArrayElementAtIndex(exitA).objectReferenceValue = B;

        var exitAObj = new SerializedObject(A.Exits[exitA].gameObject);
        var exitAActiveProp = exitAObj.FindProperty("m_IsActive");

        exitAActiveProp.boolValue = false;
        
        var exitBObj = new SerializedObject(B.Exits[exitB].gameObject);
        var exitBActiveProp = exitBObj.FindProperty("m_IsActive");

        exitBActiveProp.boolValue = false;
        
        exitAObj.ApplyModifiedProperties();
        exitBObj.ApplyModifiedProperties();
        newObj.ApplyModifiedProperties();
        currentObj.ApplyModifiedProperties();
    }

    void PlayModeChange(PlayModeStateChange mode)
    {
        Clean();
    }
}
#endif