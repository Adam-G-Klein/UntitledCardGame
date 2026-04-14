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

    private Dictionary<DescriptionToken.DescriptionIconType, Sprite> _descriptionIconSprites;
    public Dictionary<DescriptionToken.DescriptionIconType, Sprite> descriptionIconSprites
    {
        get
        {
            if (_descriptionIconSprites == null)
            {
                _descriptionIconSprites = new Dictionary<DescriptionToken.DescriptionIconType, Sprite>();
                foreach (var entry in descriptionIconList)
                {
                    _descriptionIconSprites[entry.iconType] = entry.sprite;
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
}
