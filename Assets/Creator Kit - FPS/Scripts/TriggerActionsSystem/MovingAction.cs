using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MovingAction : GameAction
{
    public PathSystem path;
    public float speed = 2.0f; 
    
    bool m_isMoving;
    PathSystem.PathData pathData = new PathSystem.PathData();
    
    public override void Activated()
    {
        m_isMoving = true;
    }

    void Start()
    {
        path.Init(transform);
        path.InitData(pathData);
    }

    void Update()
    {
        if (m_isMoving)
        {
            float distanceToGo = speed * Time.deltaTime;
            var evt = path.Move(pathData, distanceToGo);

            transform.position = pathData.position;

            if (evt == PathSystem.PathEvent.Finished)
                Destroy(this);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MovingAction))]
public class MovingActionEditor : Editor
{
    MovingAction m_Target;
    SerializedProperty m_SpeedProperty;

    void OnEnable()
    {
        m_Target = target as MovingAction;
        m_SpeedProperty = serializedObject.FindProperty("speed");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_SpeedProperty);
        m_Target.path.InspectorGUI(m_Target.transform);

        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        m_Target.path.SceneGUI(m_Target.transform);
    }
}
#endif

