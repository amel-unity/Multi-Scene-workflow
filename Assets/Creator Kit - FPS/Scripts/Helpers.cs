using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// This path system allow to define a path with different node. It is a generic system than can be added to any class
/// to define, store & use a path. The class to use it need a custom editor to call the custom Inspector & SceneGUI
/// See the Spawner for an example of use.
/// </summary>
[System.Serializable]
public class PathSystem
{
    [System.Serializable]
    public class PathData
    {
        public Vector3 position;
        public int currentNode;
        public int nextNode;
        public int direction;
    }
    
    public enum PathType
    {
        BackForth,
        Loop,
        Once
    }

    public enum PathEvent
    {
        Nothing,
        ChangedDirection,
        Finished
    }

    public PathType pathType;
    
    public Vector3[] worldNode {  get { return m_WorldNode; } }
    
    [HideInInspector]
    public Vector3[] localNodes = new Vector3[1];
    Vector3[] m_WorldNode;
    
    public void Init(Transform parentTransform)
    {
        // we make point in the path being defined in local space so game designer can move the spawner &
        // path together
        // So we convert the local nodes
        // (only used at edit time) to world position (only use at runtime)
        m_WorldNode = new Vector3[localNodes.Length];
        for (int i = 0; i < m_WorldNode.Length; ++i)
            m_WorldNode[i] = parentTransform.TransformPoint(localNodes[i]);
    }

    public void InitData(PathData data)
    {
        data.currentNode = 0;
        data.nextNode = 1;
        data.position = m_WorldNode[0];
        data.direction = 1;
    }

    /// <summary>
    /// Will change the pathData to move along the path by the given distance
    /// </summary>
    /// <param name="pathData">A PathData that store a current state on the path</param>
    /// <param name="distanceToGo">The amount to move along the path</param>
    /// <returns>The event that happened during the move : nothing, direction changed (for ping pong) or finished (for once path)</returns>
    public PathEvent Move(PathData pathData, float distanceToGo)
    {        
        PathEvent evt = PathEvent.Nothing;

        while (distanceToGo > 0)
        {
            Vector3 direction = m_WorldNode[pathData.nextNode] - pathData.position;

            float dist = distanceToGo;
            if (direction.sqrMagnitude < dist * dist)
            {
                dist = direction.magnitude;

                pathData.currentNode = pathData.nextNode;

                if (pathData.direction > 0)
                {
                    pathData.nextNode += 1;
                    if (pathData.nextNode >= m_WorldNode.Length)
                    {
                        //we reach the end
                        switch (pathType)
                        {
                            case PathType.BackForth:
                                pathData.nextNode = m_WorldNode.Length - 2;
                                pathData.direction = -1;
                                evt = PathEvent.ChangedDirection;
                                break;
                            case PathType.Loop:
                                pathData.nextNode = 0;
                                break;
                            case PathType.Once:
                                pathData.nextNode -= 1;
                                evt = PathEvent.Finished;
                                distanceToGo = -1;
                                break;
                        }
                    }
                }
                else
                {
                    pathData.nextNode -= 1;
                    if (pathData.nextNode < 0)
                    {
                        //reached the beginning again
                        switch (pathType)
                        {
                            case PathType.BackForth:
                                pathData.nextNode = 1;
                                pathData.direction = 1;
                                evt = PathEvent.ChangedDirection;
                                break;
                            case PathType.Loop:
                                pathData.nextNode = m_WorldNode.Length - 1;
                                break;
                            case PathType.Once:
                                pathData.nextNode += 1;
                                distanceToGo = -1;
                                evt = PathEvent.Finished;
                                break;
                        }
                    }
                }
            }

            pathData.position = pathData.position + direction.normalized * dist;
            
            //We remove the distance we moved. That way if we didn't had enough distance to the next goal, we will do a new loop to finish
            //the remaining distance we have to cover this frame toward the new goal
            distanceToGo -= dist;
        }
        
        return evt;
    }

    
    //This is in a define block for editor, as this will only be used by editor class, i.e. the Custom Editor of the 
    //class using it
    #if UNITY_EDITOR

    public void InspectorGUI(UnityEngine.Object target)
    {
        EditorGUI.BeginChangeCheck();
        PathType platformType = (PathType)EditorGUILayout.EnumPopup("Looping", pathType);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Moving Platform type");
            pathType = platformType;
            
            SceneView.RepaintAll();
        }
        
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        if (GUILayout.Button("Add Node"))
        {
            Undo.RecordObject(target, "added node");
            Vector3 position = localNodes[localNodes.Length - 1] + Vector3.right;
            ArrayUtility.Add(ref localNodes, position);
                    
            SceneView.RepaintAll();
        }

        EditorGUIUtility.labelWidth = 64;
        int delete = -1;
        for (int i = 0; i < localNodes.Length; ++i)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();

            int size = 64;
            EditorGUILayout.BeginVertical(GUILayout.Width(size));
            EditorGUILayout.LabelField("Node " + i, GUILayout.Width(size));
            if (i != 0 && GUILayout.Button("Delete", GUILayout.Width(size)))
            {
                delete = i;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            Vector3 newPosition;
            if (i == 0)
                newPosition = localNodes[i];
            else
                newPosition = EditorGUILayout.Vector3Field("Position", localNodes[i]);
            EditorGUILayout.EndVertical();


            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "changed position");
                localNodes[i] = newPosition;
                        
                SceneView.RepaintAll();
            }
        }
        EditorGUIUtility.labelWidth = 0;

        if (delete != -1)
        {
            Undo.RecordObject(target, "Removed point in path");

            ArrayUtility.RemoveAt(ref localNodes, delete);
                    
            SceneView.RepaintAll();
        }
    }
    
    public void SceneGUI(Transform parentTransform)
    {
        for (int i = 0; i < localNodes.Length; ++i)
        {
            Vector3 worldPos;
            if (Application.isPlaying)
            {
                worldPos = worldNode[i];
            }
            else
            {
                worldPos = parentTransform.TransformPoint(localNodes[i]);
            }


            Vector3 newWorld = worldPos;
            Vector3 previousWorld;

            if (i == 0)
            {
                previousWorld = parentTransform.TransformPoint(localNodes[localNodes.Length - 1]);
            }
            else
            {
                previousWorld = parentTransform.TransformPoint(localNodes[i - 1]);
            }

            Vector3 direction = worldPos - previousWorld;
            float fullDist = direction.magnitude;
            direction.Normalize();

            if (i != 0)
            {
                newWorld = Handles.PositionHandle(worldPos, Quaternion.identity);
            }

            Handles.color = Color.red;

            if (i == 0)
            {
                if (pathType != PathType.Loop)
                    continue;


                Handles.DrawDottedLine(worldPos, previousWorld, 10);
            }
            else
            {
                Handles.DrawDottedLine(worldPos, previousWorld, 10);

                if (worldPos != newWorld)
                {
                    Undo.RecordObject(parentTransform.gameObject, "moved point");
                    localNodes[i] = parentTransform.InverseTransformPoint(newWorld);
                }
            }

            float dist = 0.6f;
            while (dist < fullDist)
            {
                Handles.ConeHandleCap(-1, previousWorld + direction * dist, Quaternion.LookRotation(direction), 0.15f, EventType.Repaint);
                dist += 0.6f;
            }
        }
    }
    #endif
}

/// <summary>
/// Small static Helpers functions
/// </summary>
public class Helpers
{
    public static void RecursiveLayerChange(Transform parent, int layer)
    {
        parent.gameObject.layer = layer;
        foreach(Transform t in parent)
            RecursiveLayerChange(t, layer);
    }
}