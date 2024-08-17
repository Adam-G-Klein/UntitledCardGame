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

    public static void SetAllPickingMode(VisualElement ve, PickingMode pickingMode){
        ve.pickingMode = pickingMode;
        foreach (VisualElement child in ve.Children()){
            SetAllPickingMode(child, pickingMode);
        }
    }

    // TODO, this method could be flexible enough to register different methods for all of these pointer actions
    // leavin it easy for now because I'm just using it for setStateDirty
    public static void SetAllPointerEventsToCallback(VisualElement ve, Action callback){
        ve.pickingMode = PickingMode.Position;

        // so we get the nice default hover animation
        ve.RegisterCallback<PointerEnterEvent>((evt) => {
            callback.Invoke();
        });

        ve.RegisterCallback<PointerLeaveEvent>((evt) => {
            callback.Invoke();
        });

        ve.RegisterCallback<ClickEvent>((evt) => {
            callback.Invoke();
        });
    }

    /// <summary>
    /// Recursively sets all pointer events on a visual element and its children to a callback
    /// DO NOT USE UNLESS YOU'RE READY FOR THIS CALLBACK TO BE CALLED A LOT
    /// This is just a slight optimization over calling the callback in Update()
    /// </summary>
    /// <param name="ve"></param>
    /// <param name="callback"></param>
    public static void RecursivelySetAllPointerEventsToCallback(VisualElement ve, Action callback) {
        SetAllPointerEventsToCallback(ve, callback);
        foreach (VisualElement child in ve.Children()){
            RecursivelySetAllPointerEventsToCallback(child, callback);
        }
    }


}
