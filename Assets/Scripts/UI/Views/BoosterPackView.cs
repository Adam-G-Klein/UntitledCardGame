using UnityEngine;
using UnityEngine.UIElements;

public class BoosterPackView
{
    public VisualElement boosterPackContainer;
    private BoosterPack boosterPackSO;

    public BoosterPackView(BoosterPack boosterPack)
    {
        this.boosterPackSO = boosterPack;
        boosterPackContainer = CreateBoosterPackView(boosterPack);
    }

    private VisualElement CreateBoosterPackView(BoosterPack boosterPack)
    {
        VisualElement container = new VisualElement();
        container.AddToClassList("booster-pack-container");

        // Set background color from pack's primary color
        if (boosterPack.GetPackSO() != null)
        {
            Color packColor = boosterPack.GetPackSO().packColor;
            container.style.backgroundColor = packColor;
        }

        // Add pack name label
        Label packNameLabel = new Label();
        packNameLabel.text = boosterPack.BoosterPackName();
        packNameLabel.AddToClassList("booster-pack-label");

        container.Add(packNameLabel);

        // Add an icon
        VisualElement iconElement = new VisualElement();
        iconElement.AddToClassList("booster-pack-icon");
        if (boosterPack is RatBoosterPackSO ratBoosterPack)
        {
            iconElement.style.backgroundImage = new StyleBackground(ratBoosterPack.ratSO.icon);
        }
        else if (boosterPack.GetPackSO() != null)
        {
            iconElement.style.backgroundImage = new StyleBackground(boosterPack.GetPackSO().packColorIcon);
        }
        container.Add(iconElement);

        return container;
    }

    public VisualElement GetContainer()
    {
        return boosterPackContainer;
    }
}