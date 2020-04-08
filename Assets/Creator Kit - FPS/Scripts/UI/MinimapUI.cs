using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    public static MinimapUI Instance { get; private set; }

    public RawImage MapImageTarget;
    public RectTransform Arrow;

    public int Resolution;

    public MinimapSystem.MinimapSystemSetting MinimapSystemSettings;
    
    public RenderTexture RenderTexture => m_RT;
    
    RenderTexture m_RT;
    float m_Ratio;
    bool m_HeightWasInited;
    float m_InitialHeight;
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        m_HeightWasInited = false;
        m_Ratio = MapImageTarget.rectTransform.rect.height / MapImageTarget.rectTransform.rect.width;
        m_RT = new RenderTexture(Resolution, Mathf.FloorToInt(Resolution * m_Ratio), 16, RenderTextureFormat.ARGB32);
        MapImageTarget.texture = m_RT;
    }

    public void UpdateForPlayerTransform(Transform playerTransform)
    {
        if (!m_HeightWasInited)
        {
            m_HeightWasInited = true;
            m_InitialHeight = playerTransform.position.y;
        }

        Vector3 usedPosition = playerTransform.position;
        float heightDifference = m_InitialHeight - usedPosition.y;
        float heightSign = Mathf.Sign(heightDifference);
        heightDifference = Mathf.FloorToInt(Mathf.Abs(heightDifference/MinimapSystemSettings.heightStep)) * heightSign * heightDifference;

        usedPosition.y = m_InitialHeight + heightDifference;
        
        MinimapSystem.Render(m_RT, usedPosition, playerTransform.forward, MinimapSystemSettings);

        if (MinimapSystemSettings.isFixed)
        {
            Arrow.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(playerTransform.forward, Vector3.forward, Vector3.up));
        }
        else
        {
            Arrow.rotation = Quaternion.identity;
        }
    }
}
