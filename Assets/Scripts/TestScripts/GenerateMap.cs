using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GenerateMap : MonoBehaviour
{
    public MapGeneratorSO mapGenerator;
    public MapVariableSO activeMapVariable;
    public CompanionListVariableSO activeCompanionList;

    public void go() {
        activeMapVariable.SetValue(mapGenerator.generateMap());
        SceneManager.LoadScene("Map");
    }
}
