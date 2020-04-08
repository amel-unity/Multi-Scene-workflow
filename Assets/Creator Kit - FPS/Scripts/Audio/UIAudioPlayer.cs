using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAudioPlayer : MonoBehaviour
{
    public static UIAudioPlayer Instance { get; private set; }

    public AudioClip PositiveSound;
    public AudioClip NegativeSound;
    
    AudioSource m_Source;

    void Awake()
    {
        m_Source = GetComponent<AudioSource>();
        Instance = this;
    }

    public static void PlayPositive()
    {
        Instance.m_Source.PlayOneShot(Instance.PositiveSound);
    }

    public static void PlayNegative()
    {
        Instance.m_Source.PlayOneShot(Instance.NegativeSound);
    }
}
