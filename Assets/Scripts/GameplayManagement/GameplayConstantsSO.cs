using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(
    fileName = "New GameplayConstants",
    menuName = "Constants/Gameplay Constants")]
public class GameplayConstantsSO : ScriptableObject
{
    public bool DEVELOPMENT_MODE = true;
    public int START_TURN_MANA = 0;
    public int COMPANIONS_FOR_LEVELTWO_COMBINATION = 3;
    public int COMPANIONS_FOR_LEVELTHREE_COMBINATION = 2;

    // not being enforced right now, just for spawn locations
    public int MAX_MINIONS_PER_COMPANION = 4;

    public int MAX_HAND_SIZE = 10;
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
