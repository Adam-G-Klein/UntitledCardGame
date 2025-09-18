using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class ScreenShakeManager : GenericSingleton<ScreenShakeManager>
{

    private CinemachineImpulseSource src;

    [SerializeField]
    private Vector3 randVelocityLow;
    [SerializeField]
    private Vector3 randVelocityHigh;
    [Header("Uncomment the Update->check keycode block in this script to debug")]
    [SerializeField]
    private float debugForce;

    void Start()
    {
        src = GetComponent<CinemachineImpulseSource>();
    }

    


    // TODO: create custom editor to allow for screenshake debugging
    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShakeWithForce(debugForce);
            
        }
    }
    */

    public void ShakeWithForce(float force)
    {
        src.m_DefaultVelocity = new Vector3(
            Random.Range(randVelocityLow.x, randVelocityHigh.x),
            Random.Range(randVelocityLow.y, randVelocityHigh.y),
            Random.Range(randVelocityLow.z, randVelocityHigh.z)
            );
        src.GenerateImpulse(force);
    }



}