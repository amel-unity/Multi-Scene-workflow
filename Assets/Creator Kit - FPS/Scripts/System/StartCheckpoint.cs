using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCheckpoint : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        GameSystem.Instance.ResetTimer();
        GameSystem.Instance.StartTimer();
        Destroy(gameObject);
    }
}
