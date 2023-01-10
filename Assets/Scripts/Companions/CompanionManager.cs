using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CombatEntityInstantiatedEventListener))]
[RequireComponent(typeof(CombatEntityDeathEventListener))]
public class CompanionManager : MonoBehaviour
{
    /* There should never be much code in here if we 
        want to keep with the pattern of having very 
        modular scenes. All I know right now is that we'll need 
        something in the scene that listens to companions/enemy
        instantiations and knows about all of them
    */
    private List<CompanionInstance> companions = new List<CompanionInstance>();
    private List<string> companionIds = new List<string>();

    [SerializeField]
    private EndEncounterEvent endEncounterEvent;


    public void combatEntityInstantiatedHandler(CombatEntityInstantiatedEventInfo info) {
        if(info.instance is CompanionInstance){
            companions.Add((CompanionInstance) info.instance);
        }
    }
    public void combatEntityDeathHandler(CombatEntityDeathEventInfo info) {
        if(info.instance is CompanionInstance){
            companions.Remove((CompanionInstance) info.instance);
            if(companions.Count == 0) {
                StartCoroutine(endEncounterEvent.RaiseAtEndOfFrameCoroutine(new EndEncounterEventInfo(EncounterOutcome.Defeat)));
            }
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
    
}
