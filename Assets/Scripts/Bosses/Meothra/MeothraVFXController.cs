using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using CommonTools.Extensions;
#endif
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MeothraVFXController : MonoBehaviour
{

    [SerializeField] private GameObject introExplosion;
    [SerializeField] private GameObject idleBackgroundEffect;
    [SerializeField] private Transform idleBackgroundEffectSpawnPoint;
    [SerializeField] private GameObject endingSwirlEffect;
    [SerializeField] private Transform endingSwirlEffectSpawnPoint;
    private GameObject currentEndingSwirlEffectInstance;
    [SerializeField] private GameObject startingSwirlEffect;
    [SerializeField] private Transform startingSwirlEffectSpawnPoint;
    [SerializeField] private GameObject currentStartingSwirlEffectInstance;

    [SerializeField] private GameObject combatOverSucc;
    [SerializeField] private Transform combatOverSuccSpawnPoint;
    private GameObject currentCombatOverSuccInstance;
    private GameObject currentExplosionEffectInstance;
    [SerializeField] private GameObject currentIdleBackgroundEffectInstance;
    [SerializeField] private MeothraController meothraController;
    [SerializeField] private List<Material> idleBackgroundMaterials = new List<Material>();
    [SerializeField] private float idleMaterialTweenInDuration = 5f;
    [SerializeField] private float idleMaterialTweenOutDuration = 5f;

/*
    void OnGUI()
    {
        foreach(ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.PlayInEditor(true, null);
        }
    }
    */

    void Update()
    {
        if(currentStartingSwirlEffectInstance != null && currentStartingSwirlEffectInstance.activeInHierarchy)
        {
            currentStartingSwirlEffectInstance.transform.position = meothraController.enemyInstance.enemyView.focusable.GetWorldspacePosition();
        }
    }

    // DEPRECATED, timeline now handles this
    public void Explode()
    {

        Debug.Log("MeothraVFXController: Explode called!");

    }

    public void TweenInIdleMaterials()
    {
        // TODO: Ayo to implement here
        Debug.Log("MeothraVFXController: StopBillowing called!");
        // TODO wait for the end of the current billowing animation and destroy it
        List<List<Vector4>> allCustomParticleData = new List<List<Vector4>>();
        LeanTween.value(2, -1, idleMaterialTweenInDuration).setOnUpdate((float val) =>
        {
            /* 
            // one would hope this would work. 
            // alas it does not.
            // keeping it here so people know there is theoretically a better way
            ParticleSystem[] particleSystems = currentIdleBackgroundEffectInstance.GetComponentsInChildren<ParticleSystem>();
            List<Vector4> customParticleData = new List<Vector4>();
            ps.GetCustomParticleData(customParticleData, ParticleSystemCustomData.Custom1);
            // get the first particle, set its x value to val, our tweened value
            for(int i = 0; i < customParticleData.Count; i++)
            {
                customParticleData[i] = new Vector4(val, 0, 0, 0);
            }
            ps.SetCustomParticleData(customParticleData, ParticleSystemCustomData.Custom1);
            */
            foreach(Material mat in idleBackgroundMaterials)
            {
                mat.SetFloat("_Cutoff_Value", val);
            }

        });
        
    }

    public void TweenOutIdleMaterials()
    {
        LeanTween.value(-1, 2, idleMaterialTweenOutDuration).setOnUpdate((float val) =>
        {
            foreach (Material mat in idleBackgroundMaterials)
            {
                mat.SetFloat("_Cutoff_Value", val);
            }

        }).setOnComplete(() =>
        {
            Destroy(currentIdleBackgroundEffectInstance);
        });}

    // DEPRECATED, timeline now handles this
    public void Retract()
    {
        // TODO: Ayo to implement here
        Debug.Log("MeothraVFXController: Retract called!");
        // TODO, make it work
    }

    // DEPRECATED, timeline now handles this
    public void RetractionComplete()
    {
        // TODO: Ayo to implement here
        Debug.Log("MeothraVFXController: RetractionComplete called!");
    }

    // DEPRECATED, timeline now handles this
    public void FullyDestroy()
    {
        // TODO: Ayo to implement here
        Debug.Log("MeothraVFXController: FullyDestroy called!");
        Destroy(currentEndingSwirlEffectInstance);
        Destroy(currentCombatOverSuccInstance);

    }
}
