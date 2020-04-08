using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This script need to be attached to any Objects with a renderer that need to be rendered in the minimap
/// </summary>
public class MinimapElement : MonoBehaviour
{
    public static List<Renderer> Renderers => s_Renderers;
    
    static List<Renderer> s_Renderers = new List<Renderer>();

    Renderer m_Renderer;
    
    void OnEnable()
    {
        m_Renderer = GetComponent<Renderer>();
        
        if(m_Renderer != null)
            s_Renderers.Add(m_Renderer);
    }

    void OnDisable()
    {
        if (m_Renderer)
            s_Renderers.Remove(m_Renderer);
    }
}
