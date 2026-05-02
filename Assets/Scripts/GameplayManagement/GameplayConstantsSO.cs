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
