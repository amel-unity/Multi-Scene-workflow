using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenMap : MonoBehaviour
{
    public static FullscreenMap Instance { get; private set; }

    public RawImage MapImageTarget;
    public RectTransform Arrow;

    public int Resolution;

    public MinimapSystem.MinimapSystemSetting MinimapSystemSettings;
    
    public RenderTexture RenderTexture => m_RT;
    
    RenderTexture m_RT;
    float m_Ratio;
    
    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
        m_Ratio = MapImageTarget.rectTransform.rect.height / MapImageTarget.rectTransform.rect.width;
        m_RT = new RenderTexture(Resolution, Mathf.FloorToInt(Resolution * m_Ratio), 16, RenderTextureFormat.ARGB32);
        MapImageTarget.texture = m_RT;
    }

    public void UpdateForPlayerTransform(Transform playerTransform)
    {
        MinimapSystem.Render(m_RT, playerTransform.position, playerTransform.forward, MinimapSystemSettings);

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
