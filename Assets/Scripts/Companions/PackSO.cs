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
    public Sprite cardBackCommon;
    public Sprite cardBackUncommon;
    public Sprite cardBackRare;
    public Sprite cardTab;
    public Sprite levelOneFrame;
    public Sprite levelTwoFrame;
    public Sprite levelThreeFrame;
    public Sprite frameBackground;

    [Header("Rarity Icons")]
    public Sprite commonIcon;
    public Sprite uncommonIcon;
    public Sprite rareIcon;
}
