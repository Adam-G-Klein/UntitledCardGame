using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(
    fileName ="Colors",
    menuName = "Variables/New Colors Variable")]
public class UIColors : ScriptableObject
{
    public Color friendlyEffectColor;
    public Color enemyEffectColor;
    public Color neutralEffectColor;
    
}
