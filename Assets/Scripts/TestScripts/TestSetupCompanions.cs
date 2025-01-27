using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TestSetupCompanions : MonoBehaviour
{
    public GameStateVariableSO gameStateVariableSO;
    public ShopViewController shopViewController;

    [ContextMenu("Go")]
    public void Go() {
        shopViewController.SetupUnitManagement(gameStateVariableSO.companions);
    }
}
