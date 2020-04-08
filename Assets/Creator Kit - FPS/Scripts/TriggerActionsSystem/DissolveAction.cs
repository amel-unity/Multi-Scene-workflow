using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DissolveAction : GameAction
{
    public float DissolveEffectTime = 2;
    public AnimationCurve FadeIn;

    public GameAction[] FinishedAction;
    
    ParticleSystem m_ParticleSystem;
    float m_Timer = 0;

    Renderer[] m_Renderers;
    MaterialPropertyBlock m_PropertyBlock;

    int m_CutoffProperty;

    void Start ()
    {
        m_CutoffProperty = Shader.PropertyToID("_Cutoff");
        m_Renderers = GetComponentsInChildren<Renderer>();
        
        m_PropertyBlock = new MaterialPropertyBlock();

        m_ParticleSystem = GetComponentInChildren <ParticleSystem>();

        var main = m_ParticleSystem.main;
        main.duration = DissolveEffectTime;
        
        //Disable that to avoid the Update function being called. Being called by the GameTrigger will reactivate it
        enabled = false;    
    }
	
    void Update ()
    {
        m_Timer += Time.deltaTime;

        float value = FadeIn.Evaluate(Mathf.InverseLerp(0, DissolveEffectTime, m_Timer));

        m_PropertyBlock.SetFloat(m_CutoffProperty,value);
        foreach (var r in m_Renderers)
        {
            r.SetPropertyBlock(m_PropertyBlock);
        }
        
        if (m_Timer > DissolveEffectTime)
        {
            foreach (var gameAction in FinishedAction)
            {
                gameAction.Activated();
            }
            
            Destroy(gameObject);
        }
    }
    
    public override void Activated()
    {
        enabled = true;
        m_ParticleSystem.Play();
    }
}
