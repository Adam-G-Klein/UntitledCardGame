using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class ScreenShakeDebug : MonoBehaviour
{

    private CinemachineImpulseSource src;

    void Start()
    {
        src = GetComponent<CinemachineImpulseSource>();
    }

    [ContextMenu("DebugShake")]
    public void Shake()
    {
        //src.m_ImpulseDefinition = cinemachineImpulseDefinition;
        src.GenerateImpulse();
    }



}