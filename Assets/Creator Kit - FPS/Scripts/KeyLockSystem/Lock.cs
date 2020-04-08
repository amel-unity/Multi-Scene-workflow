using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Lock : GameTrigger
{
    public string keyType;
    public Text KeyNameText;


    Canvas m_Canvas;

    void Start()
    {
        KeyNameText.text = keyType;

        m_Canvas = KeyNameText.GetComponentInParent<Canvas>();
        m_Canvas.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        m_Canvas.gameObject.SetActive(true);
        
        var keychain = other.GetComponent<Keychain>();

        if (keychain != null && keychain.HaveKey(keyType))
        {
            keychain.UseKey(keyType);
            Opened();
            //just destroy the script, if it's on the door we don't want to destroy the door.
            Destroy(this);
            Destroy(m_Canvas.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        m_Canvas.gameObject.SetActive(false);
    }

    public virtual void Opened()
    {
        Trigger();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Lock))]
public class LockEditor : Editor
{
    SerializedProperty m_ActionListProperty;
    SerializedProperty m_KeyNameTextProperty;
    Lock m_Lock;

    int m_KeyTypeIndex = -1;
    string[] m_AllKeyType = new string[0];

    void OnEnable()
    {
        m_Lock = target as Lock;
        m_ActionListProperty = serializedObject.FindProperty("actions");
        m_KeyNameTextProperty = serializedObject.FindProperty("KeyNameText");

        var allKeys = Resources.FindObjectsOfTypeAll<Key>();
        foreach (var key in allKeys)
        {
            ArrayUtility.Add(ref m_AllKeyType, key.keyType);

            if (m_Lock.keyType == key.keyType)
            {
                m_KeyTypeIndex = m_AllKeyType.Length - 1;
            }
        }
    }
    
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_KeyNameTextProperty);
        EditorGUILayout.PropertyField(m_ActionListProperty, true);

        if (m_AllKeyType.Length > 0)
        {
            int index = EditorGUILayout.Popup("Key Type", m_KeyTypeIndex, m_AllKeyType);
            if (index != m_KeyTypeIndex)
            {
                Undo.RecordObject(m_Lock, "Changed Key Type");

                m_Lock.keyType = m_AllKeyType[index];
                m_KeyTypeIndex = index;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Add at least a key in the scene to be able to select the type here", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif