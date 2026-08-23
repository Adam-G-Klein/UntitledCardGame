using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(
    fileName = "New GameplayConstants",
    menuName = "Constants/Gameplay Constants")]
public class GameplayConstantsSO : ScriptableObject
{
    public VisualTreeAsset cardTemplate;
    public VisualTreeAsset mapTemplate;
    public VisualTreeAsset enemyTemplate;
    public Sprite neutralCommonIcon;
    public Sprite neutralUncommonIcon;
    public Sprite neutralRareIcon;
    public Sprite neutralPackIcon;

    [SerializeField]
    private List<DescriptionIconEntry> descriptionIconList = new();

    [SerializeField]
    private List<BoosterPackSpriteEntry> boosterPackSpriteList = new();

    private Dictionary<DescriptionToken.DescriptionIconType, DescriptionIconInfo> _descriptionIconSprites;
    public Dictionary<DescriptionToken.DescriptionIconType, DescriptionIconInfo> descriptionIconSprites
    {
        get
        {
            if (_descriptionIconSprites == null)
            {
                _descriptionIconSprites = new Dictionary<DescriptionToken.DescriptionIconType, DescriptionIconInfo>();
                foreach (var entry in descriptionIconList)
                {
                    _descriptionIconSprites[entry.iconType] = new DescriptionIconInfo {sprite = entry.sprite, iconName = entry.iconName};
                }
            }
            return _descriptionIconSprites;
        }
    }

    private Dictionary<(BoosterPack.BoosterPackType, PackSO), Sprite> _boosterPackSprites;
    public Sprite GetBoosterPackSprite(BoosterPack.BoosterPackType boosterPackType, PackSO packSO)
    {
        if (_boosterPackSprites == null)
        {
            _boosterPackSprites = new Dictionary<(BoosterPack.BoosterPackType, PackSO), Sprite>();
            foreach (var entry in boosterPackSpriteList)
            {
                _boosterPackSprites[(entry.boosterPackType, entry.packSO)] = entry.sprite;
            }
        }
        _boosterPackSprites.TryGetValue((boosterPackType, packSO), out Sprite sprite);
        return sprite;
    }
}

[Serializable]
public class DescriptionIconEntry
{
    public DescriptionToken.DescriptionIconType iconType;
    public Sprite sprite;
    public string iconName;
}

[Serializable]
public class DescriptionIconInfo
{
    public Sprite sprite;
    public string iconName;
}

[Serializable]
public class BoosterPackSpriteEntry
{
    public BoosterPack.BoosterPackType boosterPackType;
    public PackSO packSO;
    public Sprite sprite;
}
