using System;
using System.Collections;
using System.Collections.Generic;
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

    void Update()
    {
        if(currentStartingSwirlEffectInstance != null)
        {
            currentStartingSwirlEffectInstance.transform.position = meothraController.enemyInstance.enemyView.focusable.GetWorldspacePosition();
        }
    }

    public void Explode()
    {

        Debug.Log("MeothraVFXController: Explode called!");
        Destroy(currentStartingSwirlEffectInstance);
        currentExplosionEffectInstance = Instantiate(introExplosion, meothraController.GetFrameLocation(), Quaternion.identity);

    }

    public void StopBillowing()
    {
        // TODO: Ayo to implement here
        Debug.Log("MeothraVFXController: StopBillowing called!");
        // TODO wait for the end of the current billowing animation and destroy it
        currentIdleBackgroundEffectInstance = Instantiate(idleBackgroundEffect, idleBackgroundEffectSpawnPoint.position, idleBackgroundEffectSpawnPoint.rotation);
        List<List<Vector4>> allCustomParticleData = new List<List<Vector4>>();
        LeanTween.value(2, -1, 5).setOnUpdate((float val) =>
        {
            /* one would hope this would work. 
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

    public void Retract()
    {
        // TODO: Ayo to implement here
        Debug.Log("MeothraVFXController: Retract called!");
        // TODO, make it work
        LeanTween.alpha(currentIdleBackgroundEffectInstance, 0f, 2f)
            .setOnComplete(() => Destroy(currentIdleBackgroundEffectInstance));
        currentCombatOverSuccInstance = Instantiate(combatOverSucc, meothraController.GetFrameLocation(), Quaternion.identity);
        currentEndingSwirlEffectInstance = Instantiate(endingSwirlEffect, meothraController.GetFrameLocation(), Quaternion.identity);
    }

    public void RetractionComplete()
    {
        // TODO: Ayo to implement here
        Debug.Log("MeothraVFXController: RetractionComplete called!");
    }

    public void FullyDestroy()
    {
        // TODO: Ayo to implement here
        Debug.Log("MeothraVFXController: FullyDestroy called!");
        Destroy(currentEndingSwirlEffectInstance);
        Destroy(currentCombatOverSuccInstance);

    }
}
