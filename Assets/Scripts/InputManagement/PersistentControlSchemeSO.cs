using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PersistentControlInfoSO", 
    menuName = "PersistentControlInfoSO")]
public class PersistentControlInfoSO : ScriptableObject {
    public ControlsManager.ControlMethod currentControlMethod;
    public ControlsManager.ControlScheme currentControlScheme;
    public bool valuesFromLastScene = false;
}