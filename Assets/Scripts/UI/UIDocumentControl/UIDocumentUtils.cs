using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;

public class UIDocumentUtils : MonoBehaviour
{
    // Hey Patrick, please help me figure out where commonly used view construction methods should go :P
    public static VisualElement makeWorldspaceCardView(CardType card) {
        var container = new VisualElement();
        container.AddToClassList("worldspace-card-container");

        var manaCost = new Label();
        manaCost.AddToClassList("mana-card-label");
        manaCost.text = card.Cost.ToString();
        container.Add(manaCost);

        var image = new VisualElement();
        image.AddToClassList("card-image");
        image.style.backgroundImage = new StyleBackground(card.Artwork);
        container.Add(image);

        var name = new Label();
        name.AddToClassList("card-title-label");
        name.text = card.Name;
        container.Add(name);

        var desc = new Label();
        desc.AddToClassList("card-desc-label");
        desc.text = card.Description;
        container.Add(desc);

        return container;
    }

    public static void SetAllPickingMode(VisualElement ve, PickingMode mode){
        ve.pickingMode = mode;
        foreach (VisualElement child in ve.Children()){
            SetAllPickingMode(child, mode);
        }
    }

    public static void SetAllMouseMotionCallbacks(VisualElement ve, EventCallback<MouseEnterEvent> mouseEnter, EventCallback<MouseLeaveEvent> mouseLeave){
        ve.RegisterCallback<MouseEnterEvent>(mouseEnter);
        ve.RegisterCallback<MouseLeaveEvent>(mouseLeave);
        foreach (VisualElement child in ve.Children()){
            SetAllMouseMotionCallbacks(child, mouseEnter, mouseLeave);
        }
    }
}
