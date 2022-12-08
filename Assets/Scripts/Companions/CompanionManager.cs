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
    private List<Companion> companions = new List<Companion>();
    private List<string> companionIds = new List<string>();

    public void companionInstantiatedEventHandler(CompanionInstantiatedEventInfo info){
        companions.Add(info.companion);
    }

    public string getRandomCompanionId(){
        string ret = companions[Random.Range(0,companions.Count)].id;
        return ret;
    }

    public List<string> getCompanionIds(){
        companionIds.Clear();
        foreach(Companion companion in companions){
            companionIds.Add(companion.id);
        }
        return companionIds;
    }

    
}
