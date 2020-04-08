using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Weapon : MonoBehaviour
{
    static RaycastHit[] s_HitInfoBuffer = new RaycastHit[8];
    
    public enum TriggerType
    {
        Auto,
        Manual
    }

    public enum WeaponType
    {
        Raycast,
        Projectile
    }

    public enum WeaponState
    {
        Idle,
        Firing,
        Reloading
    }

    [System.Serializable]
    public class AdvancedSettings
    {
        public float spreadAngle = 0.0f;
        public int projectilePerShot = 1;
        public float screenShakeMultiplier = 1.0f;
    }

    public TriggerType triggerType = TriggerType.Manual;
    public WeaponType weaponType = WeaponType.Raycast;
    public float fireRate = 0.5f;
    public float reloadTime = 2.0f;
    public int clipSize = 4;
    public float damage = 1.0f;

    [AmmoType]
    public int ammoType = -1;

    public Projectile projectilePrefab;
    public float projectileLaunchForce = 200.0f;

    public Transform EndPoint; 

    public AdvancedSettings advancedSettings;
    
    [Header("Animation Clips")]
    public AnimationClip FireAnimationClip;
    public AnimationClip ReloadAnimationClip;

    [Header("Audio Clips")]
    public AudioClip FireAudioClip;
    public AudioClip ReloadAudioClip;
    
    [Header("Visual Settings")]
    public LineRenderer PrefabRayTrail;
    
    [Header("Visual Display")]
    public AmmoDisplay AmmoDisplay;

    public bool triggerDown
    {
        get { return m_TriggerDown; }
        set 
        { 
            m_TriggerDown = value;
            if (!m_TriggerDown) m_ShotDone = false;
        }
    }

    public WeaponState CurrentState => m_CurrentState;
    public int ClipContent => m_ClipContent;
    public Controller Owner => m_Owner;

    Controller m_Owner;
    
    Animator m_Animator;
    WeaponState m_CurrentState;
    bool m_ShotDone;
    float m_ShotTimer = -1.0f;
    bool m_TriggerDown;
    int m_ClipContent;

    AudioSource m_Source;

    Vector3 m_ConvertedMuzzlePos;

    class ActiveTrail
    {
        public LineRenderer renderer;
        public Vector3 direction;
        public float remainingTime;
    }
    
    List<ActiveTrail> m_ActiveTrails = new List<ActiveTrail>();
    
    Queue<Projectile> m_ProjectilePool = new Queue<Projectile>();
    
    int fireNameHash = Animator.StringToHash("fire");
    int reloadNameHash = Animator.StringToHash("reload");     

    void Awake()
    {
        m_Animator = GetComponentInChildren<Animator>();
        m_Source = GetComponentInChildren<AudioSource>();
        m_ClipContent = clipSize;

        if (PrefabRayTrail != null)
        {
            const int trailPoolSize = 16;
            PoolSystem.Instance.InitPool(PrefabRayTrail, trailPoolSize);
        }

        if (projectilePrefab != null)
        {
            //a minimum of 4 is useful for weapon that have a clip size of 1 and where you can throw a second
            //or more before the previous one was recycled/exploded.
            int size = Mathf.Max(4, clipSize) * advancedSettings.projectilePerShot;
            for (int i = 0; i < size; ++i)
            {
                Projectile p = Instantiate(projectilePrefab);
                p.gameObject.SetActive(false);
                m_ProjectilePool.Enqueue(p);
            }
        }
    }

    public void PickedUp(Controller c)
    {
        m_Owner = c;
    }

    public void PutAway()
    {
        m_Animator.WriteDefaultValues();
        
        for (int i = 0; i < m_ActiveTrails.Count; ++i)
        {
            var activeTrail = m_ActiveTrails[i];
            m_ActiveTrails[i].renderer.gameObject.SetActive(false);
        }
        
        m_ActiveTrails.Clear();
    }

    public void Selected()
    {
        var ammoRemaining = m_Owner.GetAmmo(ammoType);
        
        //gun get disabled when ammo is == 0 and there is no more ammo in the clip, so this allow to re-enable it if we
        //grabbed ammo since last time we switched
        gameObject.SetActive(ammoRemaining != 0 || m_ClipContent != 0);
        
        if(FireAnimationClip != null)
            m_Animator.SetFloat("fireSpeed",  FireAnimationClip.length / fireRate);
        
        if(ReloadAnimationClip != null)
            m_Animator.SetFloat("reloadSpeed", ReloadAnimationClip.length / reloadTime);
        
        m_CurrentState = WeaponState.Idle;

        triggerDown = false;
        m_ShotDone = false;
        
        WeaponInfoUI.Instance.UpdateWeaponName(this);
        WeaponInfoUI.Instance.UpdateClipInfo(this);
        WeaponInfoUI.Instance.UpdateAmmoAmount(m_Owner.GetAmmo(ammoType));
        
        if(AmmoDisplay)
            AmmoDisplay.UpdateAmount(m_ClipContent, clipSize);

        if (m_ClipContent == 0 && ammoRemaining != 0)
        { 
            //this can only happen if the weapon ammo reserve was empty and we picked some since then. So directly
            //reload the clip when wepaon is selected          
            int chargeInClip = Mathf.Min(ammoRemaining, clipSize);
            m_ClipContent += chargeInClip;        
            if(AmmoDisplay)
                AmmoDisplay.UpdateAmount(m_ClipContent, clipSize);        
            m_Owner.ChangeAmmo(ammoType, -chargeInClip);       
            WeaponInfoUI.Instance.UpdateClipInfo(this);
        }
        
        m_Animator.SetTrigger("selected");
    }

    public void Fire()
    {
        if (m_CurrentState != WeaponState.Idle || m_ShotTimer > 0 || m_ClipContent == 0)
            return;
        
        m_ClipContent -= 1;
        
        m_ShotTimer = fireRate;

        if(AmmoDisplay)
            AmmoDisplay.UpdateAmount(m_ClipContent, clipSize);
        
        WeaponInfoUI.Instance.UpdateClipInfo(this);

        //the state will only change next frame, so we set it right now.
        m_CurrentState = WeaponState.Firing;
        
        m_Animator.SetTrigger("fire");

        m_Source.pitch = Random.Range(0.7f, 1.0f);
        m_Source.PlayOneShot(FireAudioClip);
        
        CameraShaker.Instance.Shake(0.2f, 0.05f * advancedSettings.screenShakeMultiplier);

        if (weaponType == WeaponType.Raycast)
        {
            for (int i = 0; i < advancedSettings.projectilePerShot; ++i)
            {
                RaycastShot();
            }
        }
        else
        {
            ProjectileShot();
        }
    }


    void RaycastShot()
    {

        //compute the ratio of our spread angle over the fov to know in viewport space what is the possible offset from center
        float spreadRatio = advancedSettings.spreadAngle / Controller.Instance.MainCamera.fieldOfView;

        Vector2 spread = spreadRatio * Random.insideUnitCircle;
        
        RaycastHit hit;
        Ray r = Controller.Instance.MainCamera.ViewportPointToRay(Vector3.one * 0.5f + (Vector3)spread);
        Vector3 hitPosition = r.origin + r.direction * 200.0f;
        
        if (Physics.Raycast(r, out hit, 1000.0f, ~(1 << 9), QueryTriggerInteraction.Ignore))
        {
            Renderer renderer = hit.collider.GetComponentInChildren<Renderer>();
            ImpactManager.Instance.PlayImpact(hit.point, hit.normal, renderer == null ? null : renderer.sharedMaterial);

            //if too close, the trail effect would look weird if it arced to hit the wall, so only correct it if far
            if (hit.distance > 5.0f)
                hitPosition = hit.point;
            
            //this is a target
            if (hit.collider.gameObject.layer == 10)
            {
                Target target = hit.collider.gameObject.GetComponent<Target>();
                target.Got(damage);
            }
        }


        if (PrefabRayTrail != null)
        {
            var pos = new Vector3[] { GetCorrectedMuzzlePlace(), hitPosition };
            var trail = PoolSystem.Instance.GetInstance<LineRenderer>(PrefabRayTrail);
            trail.gameObject.SetActive(true);
            trail.SetPositions(pos);
            m_ActiveTrails.Add(new ActiveTrail()
            {
                remainingTime = 0.3f,
                direction = (pos[1] - pos[0]).normalized,
                renderer = trail
            });
        }
    }

    void ProjectileShot()
    {
        for (int i = 0; i < advancedSettings.projectilePerShot; ++i)
        {
            float angle = Random.Range(0.0f, advancedSettings.spreadAngle * 0.5f);
            Vector2 angleDir = Random.insideUnitCircle * Mathf.Tan(angle * Mathf.Deg2Rad);

            Vector3 dir = EndPoint.transform.forward + (Vector3)angleDir;
            dir.Normalize();

            var p = m_ProjectilePool.Dequeue();
            
            p.gameObject.SetActive(true);
            p.Launch(this, dir, projectileLaunchForce);
        }
    }

    //For optimization, when a projectile is "destroyed" it is instead disabled and return to the weapon for reuse.
    public void ReturnProjecticle(Projectile p)
    {
        m_ProjectilePool.Enqueue(p);
    }

    public void Reload()
    {
        if (m_CurrentState != WeaponState.Idle || m_ClipContent == clipSize)
            return;

        int remainingBullet = m_Owner.GetAmmo(ammoType);

        if (remainingBullet == 0)
        {
            //No more bullet, so we disable the gun so it's not displayed anymore and change weapon
            gameObject.SetActive(false);
            return;
        }


        if (ReloadAudioClip != null)
        {
            m_Source.pitch = Random.Range(0.7f, 1.0f);
            m_Source.PlayOneShot(ReloadAudioClip);
        }

        int chargeInClip = Mathf.Min(remainingBullet, clipSize - m_ClipContent);
     
        //the state will only change next frame, so we set it right now.
        m_CurrentState = WeaponState.Reloading;
        
        m_ClipContent += chargeInClip;
        
        if(AmmoDisplay)
            AmmoDisplay.UpdateAmount(m_ClipContent, clipSize);
        
        m_Animator.SetTrigger("reload");
        
        m_Owner.ChangeAmmo(ammoType, -chargeInClip);
        
        WeaponInfoUI.Instance.UpdateClipInfo(this);
    }

    void Update()
    {
        UpdateControllerState();        
        
        if (m_ShotTimer > 0)
            m_ShotTimer -= Time.deltaTime;

        Vector3[] pos = new Vector3[2];
        for (int i = 0; i < m_ActiveTrails.Count; ++i)
        {
            var activeTrail = m_ActiveTrails[i];
            
            activeTrail.renderer.GetPositions(pos);
            activeTrail.remainingTime -= Time.deltaTime;

            pos[0] += activeTrail.direction * 50.0f * Time.deltaTime;
            pos[1] += activeTrail.direction * 50.0f * Time.deltaTime;
            
            m_ActiveTrails[i].renderer.SetPositions(pos);
            
            if (m_ActiveTrails[i].remainingTime <= 0.0f)
            {
                m_ActiveTrails[i].renderer.gameObject.SetActive(false);
                m_ActiveTrails.RemoveAt(i);
                i--;
            }
        }
    }

    void UpdateControllerState()
    {
        m_Animator.SetFloat("speed", m_Owner.Speed);
        m_Animator.SetBool("grounded", m_Owner.Grounded);
        
        var info = m_Animator.GetCurrentAnimatorStateInfo(0);

        WeaponState newState;
        if (info.shortNameHash == fireNameHash)
            newState = WeaponState.Firing;
        else if (info.shortNameHash == reloadNameHash)
            newState = WeaponState.Reloading;
        else
            newState = WeaponState.Idle;

        if (newState != m_CurrentState)
        {
            var oldState = m_CurrentState;
            m_CurrentState = newState;
            
            if (oldState == WeaponState.Firing)
            {//we just finished firing, so check if we need to auto reload
                if(m_ClipContent == 0)
                    Reload();
            }
        }

        if (triggerDown)
        {
            if (triggerType == TriggerType.Manual)
            {
                if (!m_ShotDone)
                {
                    m_ShotDone = true;
                    Fire();
                }
            }
            else
                Fire();
        }
    }
    
    /// <summary>
    /// This will compute the corrected position of the muzzle flash in world space. Since the weapon camera use a
    /// different FOV than the main camera, using the muzzle spot to spawn thing rendered by the main camera will appear
    /// disconnected from the muzzle flash. So this convert the muzzle post from
    /// world -> view weapon -> clip weapon -> inverse clip main cam -> inverse view cam -> corrected world pos
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCorrectedMuzzlePlace()
    {
        Vector3 position = EndPoint.position;

        position = Controller.Instance.WeaponCamera.WorldToScreenPoint(position);
        position = Controller.Instance.MainCamera.ScreenToWorldPoint(position);

        return position;
    }
}

public class AmmoTypeAttribute : PropertyAttribute
{
    
}

public abstract class AmmoDisplay : MonoBehaviour
{
    public abstract void UpdateAmount(int current, int max);
}

#if UNITY_EDITOR


[CustomPropertyDrawer(typeof(AmmoTypeAttribute))]
public class AmmoTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        AmmoDatabase ammoDB = GameDatabase.Instance.ammoDatabase;

        if (ammoDB.entries == null || ammoDB.entries.Length == 0)
        {
            EditorGUI.HelpBox(position, "Please define at least 1 ammo type in the Game Database", MessageType.Error);
        }
        else
        {
            int currentID = property.intValue;
            int currentIdx = -1;

            //this is pretty ineffective, maybe find a way to cache that if prove to take too much time
            string[] names = new string[ammoDB.entries.Length];
            for (int i = 0; i < ammoDB.entries.Length; ++i)
            {
                names[i] = ammoDB.entries[i].name;
                if (ammoDB.entries[i].id == currentID)
                    currentIdx = i;
            }

            EditorGUI.BeginChangeCheck();
            int idx = EditorGUI.Popup(position, "Ammo Type", currentIdx, names);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = ammoDB.entries[idx].id;
            }
        }
    }
}

[CustomEditor(typeof(Weapon))]
public class WeaponEditor : Editor
{ 
   SerializedProperty m_TriggerTypeProp;
   SerializedProperty m_WeaponTypeProp;
   SerializedProperty m_FireRateProp;
   SerializedProperty m_ReloadTimeProp;
   SerializedProperty m_ClipSizeProp;
   SerializedProperty m_DamageProp;
   SerializedProperty m_AmmoTypeProp;
   SerializedProperty m_ProjectilePrefabProp;
   SerializedProperty m_ProjectileLaunchForceProp; 
   SerializedProperty m_EndPointProp; 
   SerializedProperty m_AdvancedSettingsProp;
   SerializedProperty m_FireAnimationClipProp;
   SerializedProperty m_ReloadAnimationClipProp;
   SerializedProperty m_FireAudioClipProp;
   SerializedProperty m_ReloadAudioClipProp;
   SerializedProperty m_PrefabRayTrailProp;
   SerializedProperty m_AmmoDisplayProp;

   void OnEnable()
   {
       m_TriggerTypeProp = serializedObject.FindProperty("triggerType");
       m_WeaponTypeProp = serializedObject.FindProperty("weaponType");
       m_FireRateProp = serializedObject.FindProperty("fireRate");
       m_ReloadTimeProp = serializedObject.FindProperty("reloadTime");
       m_ClipSizeProp = serializedObject.FindProperty("clipSize");
       m_DamageProp = serializedObject.FindProperty("damage");
       m_AmmoTypeProp = serializedObject.FindProperty("ammoType");
       m_ProjectilePrefabProp = serializedObject.FindProperty("projectilePrefab");
       m_ProjectileLaunchForceProp = serializedObject.FindProperty("projectileLaunchForce");
       m_EndPointProp = serializedObject.FindProperty("EndPoint");
       m_AdvancedSettingsProp = serializedObject.FindProperty("advancedSettings");
       m_FireAnimationClipProp = serializedObject.FindProperty("FireAnimationClip");
       m_ReloadAnimationClipProp = serializedObject.FindProperty("ReloadAnimationClip");
       m_FireAudioClipProp = serializedObject.FindProperty("FireAudioClip");
       m_ReloadAudioClipProp = serializedObject.FindProperty("ReloadAudioClip");
       m_PrefabRayTrailProp = serializedObject.FindProperty("PrefabRayTrail");
       m_AmmoDisplayProp = serializedObject.FindProperty("AmmoDisplay");
   }

   public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(m_TriggerTypeProp);
        EditorGUILayout.PropertyField(m_WeaponTypeProp);
        EditorGUILayout.PropertyField(m_FireRateProp);
        EditorGUILayout.PropertyField(m_ReloadTimeProp);
        EditorGUILayout.PropertyField(m_ClipSizeProp);
        EditorGUILayout.PropertyField(m_DamageProp);
        EditorGUILayout.PropertyField(m_AmmoTypeProp);

        if (m_WeaponTypeProp.intValue == (int)Weapon.WeaponType.Projectile)
        {
            EditorGUILayout.PropertyField(m_ProjectilePrefabProp);
            EditorGUILayout.PropertyField(m_ProjectileLaunchForceProp);
        }
        
        EditorGUILayout.PropertyField(m_EndPointProp); 
        EditorGUILayout.PropertyField(m_AdvancedSettingsProp, new GUIContent("Advance Settings"), true);
        EditorGUILayout.PropertyField(m_FireAnimationClipProp);
        EditorGUILayout.PropertyField(m_ReloadAnimationClipProp);
        EditorGUILayout.PropertyField(m_FireAudioClipProp);
        EditorGUILayout.PropertyField(m_ReloadAudioClipProp);

        if (m_WeaponTypeProp.intValue == (int)Weapon.WeaponType.Raycast)
        {
            EditorGUILayout.PropertyField(m_PrefabRayTrailProp);
        }

        EditorGUILayout.PropertyField(m_AmmoDisplayProp);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif