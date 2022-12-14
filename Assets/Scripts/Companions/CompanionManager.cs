using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CompanionInstantiatedEventListener))]
public class CompanionManager : MonoBehaviour
{
    /* There should never be much code in here if we 
        want to keep with the pattern of having very 
        modular scenes. All I know right now is that we'll need 
        something in the scene that listens to companions/enemy
        instantiations and knows about all of them
    */
    private List<CombatEntityInstance> companions = new List<CombatEntityInstance>();
    private List<string> companionIds = new List<string>();

    public void companionInstantiatedEventHandler(CompanionInstantiatedEventInfo info){
        companions.Add(info.companionInstance);
    }

    public string getRandomCompanionId(){
        string ret = companions[Random.Range(0,companions.Count)].baseStats.getId();
        return ret;
    }

    public CombatEntityInstance getCompanionInstanceById(string id){
        foreach(CombatEntityInstance instance in companions) {
            if(instance.baseStats.getId().Equals(id)){
                return instance;
            }
        }
        Debug.LogWarning("No companion found by id: " + id);
        return null;
    }

    public List<string> getCompanionIds(){
        companionIds.Clear();
        foreach(CombatEntityInstance companion in companions){
            companionIds.Add(companion.baseStats.getId());
        }
        return companionIds;
    }

    
}
