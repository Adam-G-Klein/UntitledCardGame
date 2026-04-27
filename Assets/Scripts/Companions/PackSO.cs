using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Pack",
    menuName = "Pack/Pack SO")]
public class PackSO : ScriptableObject
{
    [Header("Pack Info")]
    public string packName;
    public string packDescription;
    public CompanionPoolSO companionPoolSO;
    public CardPoolSO packCardPoolSO;
    public Color packColor;

    [Header("Art Assets")]
    public Sprite packIcon; // TODO: this should be entirely deprecated
    public Sprite cardBack;

    [Header("Rarity Icons")]
    public Sprite commonIcon;
    public Sprite uncommonIcon;
    public Sprite rareIcon;
}
