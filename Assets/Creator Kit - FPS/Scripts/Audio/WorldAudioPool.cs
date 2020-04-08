using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAudioPool : MonoBehaviour
{
    static WorldAudioPool s_Instance;

    public AudioSource WorldSFXSourcePrefab;
    
    void Awake()
    {
        s_Instance = this;
    }

    public static void Init()
    {
        PoolSystem.Instance.InitPool(s_Instance.WorldSFXSourcePrefab, 32);
    }
    
    public static AudioSource GetWorldSFXSource()
    {
        return PoolSystem.Instance.GetInstance<AudioSource>(s_Instance.WorldSFXSourcePrefab);
    }
}
