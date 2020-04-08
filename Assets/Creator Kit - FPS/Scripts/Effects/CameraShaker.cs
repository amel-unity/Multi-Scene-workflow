using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; protected set; }

    float m_RemainingShakeTime;
    float m_ShakeStrength;
    Vector3 m_OriginalPosition;
    
    void Awake()
    {
        Instance = this;
        m_OriginalPosition = transform.localPosition;
    }

    void Update()
    {
        if (m_RemainingShakeTime > 0)
        {
            m_RemainingShakeTime -= Time.deltaTime;

            if (m_RemainingShakeTime <= 0)
            {
                transform.localPosition = m_OriginalPosition;
            }
            else
            {
                Vector3 randomDir = Random.insideUnitSphere;
                transform.localPosition = m_OriginalPosition + randomDir * m_ShakeStrength;
            }
        }
    }

    public void Shake(float time, float strength)
    {
        m_ShakeStrength = strength;
        m_RemainingShakeTime = time;
    }
}
