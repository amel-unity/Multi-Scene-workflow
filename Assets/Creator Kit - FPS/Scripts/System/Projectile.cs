using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{    
    static Collider[] s_SphereCastPool = new Collider[32];
    
    public bool DestroyedOnHit = true;
    public float TimeToDestroyed = 4.0f;
    public float ReachRadius = 5.0f;
    public float damage = 10.0f;
    public AudioClip DestroyedSound;
    
    //TODO : maybe pool that somewhere to not have to create one for each projectile.
    public GameObject PrefabOnDestruction;

    Weapon m_Owner;
    Rigidbody m_Rigidbody;
    float m_TimeSinceLaunch;
    
    void Awake()
    {
        PoolSystem.Instance.InitPool(PrefabOnDestruction, 4);
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void Launch(Weapon launcher, Vector3 direction, float force)
    {
        m_Owner = launcher;

        transform.position = launcher.GetCorrectedMuzzlePlace();
        transform.forward = launcher.EndPoint.forward;
        
        gameObject.SetActive(true);
        m_TimeSinceLaunch = 0.0f;
        m_Rigidbody.AddForce(direction * force);
    }
    
    void OnCollisionEnter(Collision other)
    {
        if (DestroyedOnHit)
        {
            Destroy();
        }
    }

    void Destroy()
    {
        Vector3 position = transform.position;
        
        var effect = PoolSystem.Instance.GetInstance<GameObject>(PrefabOnDestruction);
        effect.transform.position = position;
        effect.SetActive(true);

        int count = Physics.OverlapSphereNonAlloc(position, ReachRadius, s_SphereCastPool, 1<<10);

        for (int i = 0; i < count; ++i)
        {
            Target t = s_SphereCastPool[i].GetComponent<Target>();
            
            t.Got(damage);
        }
        
        gameObject.SetActive(false);
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        m_Owner.ReturnProjecticle(this);

        var source = WorldAudioPool.GetWorldSFXSource();

        source.transform.position = position;
        source.pitch = Random.Range(0.8f, 1.1f);
        source.PlayOneShot(DestroyedSound);
    }

    void Update()
    {
        m_TimeSinceLaunch += Time.deltaTime;

        if (m_TimeSinceLaunch >= TimeToDestroyed)
        {
            Destroy();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, ReachRadius);
    }
}
