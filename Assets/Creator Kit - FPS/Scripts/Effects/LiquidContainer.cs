// This is based on the great article by Minions Art (https://www.patreon.com/posts/quick-game-art-18245226)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidContainer : MonoBehaviour
{
    public float MaxWobble = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;
    
    Renderer m_Renderer;
    Vector3 m_PreviousPosition;
    Vector3 m_Velocity;
    Vector3 m_LastRotation;  
    Vector3 m_AngularVelocity;
    float m_WobbleAmountX;
    float m_WobbleAmountZ;
    float m_WobbleAmountToAddX;
    float m_WobbleAmountToAddZ;
    float m_Pulse;
    float m_Time = 0.5f;

    Material m_Material;

    int m_LiquidRotationId;
    int m_FillAmountId;
    
    // Use this for initialization
    void Awake()
    {
        m_Renderer = GetComponent<Renderer>();
        m_Material = m_Renderer.material;

        m_LiquidRotationId = Shader.PropertyToID("_LiquidRotation");
        m_FillAmountId = Shader.PropertyToID("_FillAmount");
    }

    public void ChangeLiquidAmount(float liquidAmount)
    {
        m_Material.SetFloat(m_FillAmountId, liquidAmount);
    }
    
    private void Update()
    {
        m_Time += Time.deltaTime;
        // decrease wobble over time
        m_WobbleAmountToAddX = Mathf.Lerp(m_WobbleAmountToAddX, 0, Time.deltaTime * (Recovery));
        m_WobbleAmountToAddZ = Mathf.Lerp(m_WobbleAmountToAddZ, 0, Time.deltaTime * (Recovery));

        // make a sine wave of the decreasing wobble
        m_Pulse = 2 * Mathf.PI * WobbleSpeed;
        m_WobbleAmountX = m_WobbleAmountToAddX * Mathf.Sin(m_Pulse * m_Time);
        m_WobbleAmountZ = m_WobbleAmountToAddZ * Mathf.Sin(m_Pulse * m_Time);
        
        Matrix4x4 rotation = Matrix4x4.Rotate( Quaternion.AngleAxis(m_WobbleAmountZ, Vector3.right) * Quaternion.AngleAxis(m_WobbleAmountX, Vector3.forward));

        // send it to the shader
        m_Material.SetMatrix(m_LiquidRotationId, rotation);
        
        // velocity
        m_Velocity = (m_PreviousPosition - transform.position) / Time.deltaTime;
        m_AngularVelocity = transform.rotation.eulerAngles - m_LastRotation;


        // add clamped velocity to wobble
        m_WobbleAmountToAddX += Mathf.Clamp((m_Velocity.x + (m_AngularVelocity.z * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
        m_WobbleAmountToAddZ += Mathf.Clamp((m_Velocity.z + (m_AngularVelocity.x * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);

        // keep last position
        m_PreviousPosition = transform.position;
        m_LastRotation = transform.rotation.eulerAngles;
    }
}
