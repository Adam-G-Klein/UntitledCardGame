using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GenerateMap : MonoBehaviour
{
    public MapGeneratorSO mapGenerator;
    public MapVariableSO activeMapVariable;
    public CompanionListVariableSO activeCompanionList;
    public PlayerDataVariableSO playerData;
    public List<CompanionTypeSO> startingCompanions;

    public void go() {
        activeCompanionList.companionBench = new List<Companion>();
        activeCompanionList.companionList = new List<Companion>();
        activeCompanionList.currentCompanionSlots = 3;

        playerData.GetValue().gold = 3;
        
        foreach (CompanionTypeSO companionType in startingCompanions) {
            activeCompanionList.companionList.Add(new Companion(companionType));
        }
        activeMapVariable.SetValue(mapGenerator.generateMap());
        SceneManager.LoadScene("Map");
    }
}
