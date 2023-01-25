using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CombatEntityInstantiatedEventListener))]
[RequireComponent(typeof(CombatEntityDeathEventListener))]
public class CompanionManager : MonoBehaviour
{
    [SerializeField]
    // put this here when I wanted effect procedures to have access to it
    // it's a hack, but I don't want to overthink this right now
    public EncounterConstants encounterConstants;
    private List<CompanionInstance> companions = new List<CompanionInstance>();
    private List<MinionInstance> minions = new List<MinionInstance>();
    private List<string> companionIds = new List<string>();

    [SerializeField]
    private EndEncounterEvent endEncounterEvent;


    public void combatEntityInstantiatedHandler(CombatEntityInstantiatedEventInfo info) {
        if(info.instance is CompanionInstance){
            companions.Add((CompanionInstance) info.instance);
        } else if(info.instance is MinionInstance){
            minions.Add((MinionInstance) info.instance);
        }
    }
    public void combatEntityDeathHandler(CombatEntityDeathEventInfo info) {
        if(info.instance is CompanionInstance){
            companions.Remove((CompanionInstance) info.instance);
            if(companions.Count == 0) {
                StartCoroutine(endEncounterEvent.RaiseAtEndOfFrameCoroutine(new EndEncounterEventInfo(EncounterOutcome.Defeat)));
            }
        }
        if(info.instance is MinionInstance){
            Debug.Log("Minion died, removing in companion manager");
            minions.Remove((MinionInstance) info.instance);
        }
    }

    public string getRandomCompanionId(){
        string ret = companions[Random.Range(0,companions.Count)].id;
        return ret;
    }

    public CompanionInstance getCompanionInstanceById(string id){
        foreach(CompanionInstance instance in companions) {
            if(instance.id.Equals(id)){
                return instance;
            }
        }
        Debug.LogWarning("No companion found by id: " + id);
        return null;
    }

    public List<string> getCompanionIds(){
        companionIds.Clear();
        foreach(CombatEntityInstance companion in companions){
            companionIds.Add(companion.id);
        }
        return companionIds;
    }

    public List<CompanionInstance> getCompanions(){
        return companions;
    }
    
    public List<MinionInstance> getMinions(){
        return minions;
    }

    public List<TargettableEntity> getEnemyTargets(){
        List<TargettableEntity> retList = new List<TargettableEntity>(minions);
        retList.AddRange(companions);
        return retList;
    }
}
