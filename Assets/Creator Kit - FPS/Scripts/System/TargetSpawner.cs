using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TargetSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEvent
    {
        public GameObject targetToSpawn;
        public int count;
        public float timeBetweenSpawn;
    }

    public SpawnEvent[] spawnEvents;
    
    public float speed = 1.0f;
    
    public PathSystem path = new PathSystem();
    
    class SpawnQueueElement
    {
        public GameObject obj;
        public Target target;
        public Rigidbody rb;
        public float remainingTime;
        public PathSystem.PathData pathData = new PathSystem.PathData();
    }

    Queue<SpawnQueueElement> m_SpawnQueue;
    List<SpawnQueueElement> m_ActiveElements;
    
    // Start is called before the first frame update
    void Awake()
    {
        path.Init(transform);
        
        m_SpawnQueue = new Queue<SpawnQueueElement>();
        
        foreach (var e in spawnEvents)
        {
            for (int i = 0; i < e.count; ++i)
            {
                SpawnQueueElement element = new SpawnQueueElement()
                {
                    obj = Instantiate(e.targetToSpawn),
                    remainingTime = e.timeBetweenSpawn
                };
                element.rb = element.obj.GetComponent<Rigidbody>();
                element.target = element.obj.GetComponentInChildren<Target>();
                element.obj.SetActive(false);
                element.obj.transform.position = transform.position;
                element.obj.transform.rotation = transform.rotation;
                
                path.InitData(element.pathData);
                
                m_SpawnQueue.Enqueue(element);
            }
        }
        
        if(m_SpawnQueue.Count == 0)
            Destroy(gameObject);
        else
        {
            m_ActiveElements = new List<SpawnQueueElement>();      
           Dequeue();
        }
    }

    void Dequeue()
    {
        var e = m_SpawnQueue.Dequeue();
        e.obj.SetActive(true);
        
        m_ActiveElements.Add(e);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_SpawnQueue.Count > 0)
        {
            var elem = m_SpawnQueue.Peek();
            elem.remainingTime -= Time.deltaTime;

            if (elem.remainingTime <= 0)
            {
                Dequeue();
            }
        }

        float distanceToGo = speed * Time.deltaTime;
        for (int i = 0; i < m_ActiveElements.Count; ++i)
        {
            var currentElem = m_ActiveElements[i];
            
            //the target was already destroyed no need to update its position.
            if(currentElem.target.Destroyed)
                continue;

            var evt = path.Move(currentElem.pathData, distanceToGo);

            switch (evt)
            {
                case PathSystem.PathEvent.Finished :
                    m_ActiveElements.RemoveAt(i);
                    i--;
                    break;
                default:
                    currentElem.rb.MovePosition(currentElem.pathData.position);
                    break;
            }
        }
    }
}

//====================

#if UNITY_EDITOR
[CustomEditor(typeof(TargetSpawner))]
public class MovingPlatformEditor : Editor
{
    TargetSpawner m_TargetSpawner;
    
    void OnEnable()
    {
        m_TargetSpawner = target as TargetSpawner;
    }

    public override void OnInspectorGUI()
    { 
        base.OnInspectorGUI();
        
        EditorGUI.BeginChangeCheck();
        float newSpeed = EditorGUILayout.FloatField("Speed", m_TargetSpawner.speed);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Speed");
            m_TargetSpawner.speed = newSpeed;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        m_TargetSpawner.path.InspectorGUI(m_TargetSpawner.transform);
    }

    private void OnSceneGUI()
    {
        m_TargetSpawner.path.SceneGUI(m_TargetSpawner.transform);
    }
}
#endif