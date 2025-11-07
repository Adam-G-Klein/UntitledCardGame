using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/*

This class is intended to be instantiated and maintained by the individual
scenes / views that need to display tooltips. The code in here was duplicated
enough that it made sense to put it somewhere central.

*/
public class TooltipController {
    private List<(GameObject, VisualElement)> tooltips;
    private GameObject prefab;

    public TooltipController(GameObject prefab) {
        this.tooltips = new List<(GameObject, VisualElement)>();
        this.prefab = prefab;
    }

    public void DisplayTooltip(VisualElement element, TooltipViewModel tooltipViewModel, TooltipContext context) {
        Vector3 position = CalculatePosition(element, context);
        GameObject tooltipGO = GameObject.Instantiate(prefab, position, Quaternion.identity);
        TooltipView tooltipView = tooltipGO.GetComponent<TooltipView>();
        tooltipView.tooltip = tooltipViewModel;
        if (!element.HasUserData<List<TooltipView>>()) {
            element.SetUserData(new List<TooltipView>());
        }
        element.GetUserData<List<TooltipView>>().Add(tooltipView);
        tooltips.Add((tooltipGO, element));
    }

    public void DestroyTooltip(VisualElement element) {
        if (!element.HasUserData<List<TooltipView>>()) return;

        List<TooltipView> userDataTooltips = element.GetUserData<List<TooltipView>>();
        List<TooltipView> tooltipsToDestroy = new List<TooltipView>(userDataTooltips);
        foreach (TooltipView tooltipView in tooltipsToDestroy) {
            userDataTooltips.Remove(tooltipView);
            tooltips.Remove((tooltipView.gameObject, element));
            GameObject.Destroy(tooltipView.gameObject);
        }
    }

    public void DestroyAllTooltips() {
        List<(GameObject, VisualElement)> tooltipCopy = new List<(GameObject, VisualElement)>(tooltips);
        foreach ((GameObject, VisualElement) tuple in tooltipCopy) {
            DestroyTooltip(tuple.Item2);
        }
    }

    private Vector3 CalculatePosition(VisualElement element, TooltipContext context) {
        Vector3 position = Vector3.zero;
        float xTooltipPos = 0f;
        float yTooltipPos = 0f;
        switch (context) {
            case TooltipContext.CompendiumCard:
                xTooltipPos = element.worldBound.center.x + (element.resolvedStyle.width * 0.7f * 1.5f);
                yTooltipPos = element.worldBound.center.y + (element.resolvedStyle.height * .1f);
                position = UIDocumentGameObjectPlacer.GetWorldPositionFromUIDocumentPosition(new Vector3(xTooltipPos, yTooltipPos, 0));
                position.z  = -1;
            break;

            case TooltipContext.CompendiumCompanion:
                xTooltipPos = element.worldBound.center.x + (element.resolvedStyle.width * 0.7f);
                yTooltipPos = element.worldBound.center.y + (element.resolvedStyle.height * .1f);
                position = UIDocumentGameObjectPlacer.GetWorldPositionFromUIDocumentPosition(new Vector3(xTooltipPos, yTooltipPos, 0));
                position.z = -1;
            break;

            case TooltipContext.StartingTeam:
                xTooltipPos = element.worldBound.center.x - (element.resolvedStyle.width * 0.93f);
                yTooltipPos = element.worldBound.center.y + (element.resolvedStyle.height * .2f);
                position = UIDocumentGameObjectPlacer.GetWorldPositionFromUIDocumentPosition(new Vector3(xTooltipPos, yTooltipPos, 0));
                position.z = -2;
            break;

            case TooltipContext.Shop:
                xTooltipPos = element.worldBound.center.x - (element.resolvedStyle.width * 1f);
                yTooltipPos = element.worldBound.center.y + (element.resolvedStyle.height * .1f);
                position = UIDocumentGameObjectPlacer.GetWorldPositionFromUIDocumentPosition(new Vector3(xTooltipPos, yTooltipPos, 0));
            break;

            case TooltipContext.CompanionManagementView:
                xTooltipPos = element.worldBound.center.x - (element.resolvedStyle.width * 1f);
                yTooltipPos = element.worldBound.center.y + (element.resolvedStyle.height * .4f);
                position = UIDocumentGameObjectPlacer.GetWorldPositionFromUIDocumentPosition(new Vector3(xTooltipPos, yTooltipPos, 0));
            break;
        }

        return position;
    }
}

public enum TooltipContext {
    CompendiumCompanion,
    CompendiumCard,
    StartingTeam,
    Shop,
    CompanionManagementView
}