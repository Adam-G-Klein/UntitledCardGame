using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
        ClampTooltipToScreen(tooltipGO, Camera.main);
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

    public static void ClampTooltipToScreen(GameObject tooltipGO, Camera cam) {
        RectTransform rectTransform;
        UIDocument uiDoc;
        RawImage rawImage;
        RenderTexture renderTexture;

        if (tooltipGO.TryGetComponent<RectTransform>(out var rectTnsfrm)) {
            rectTransform = rectTnsfrm;
        } else {
            return;
        }

        if (tooltipGO.TryGetComponent<UIDocument>(out var uiDocument)) {
            uiDoc = uiDocument;
        } else {
            return;
        }

        if (tooltipGO.TryGetComponent<RawImage>(out var rawImg)) {
            rawImage = rawImg;
        } else {
            return;
        }

        if (rawImage.texture is RenderTexture) {
            renderTexture = rawImage.texture as RenderTexture;
        } else {
            return;
        }

        VisualElement tooltipContainer = uiDoc.rootVisualElement.Q<VisualElement>("tooltip-background");

        tooltipContainer.RegisterCallback<GeometryChangedEvent>((evt) => {
            // Get world corners
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);

            // Need to get the height percent in pixels, then scale
            // the Canvas RectTransform Y values of the bottom corners
            float tooltipSpaceHeight = tooltipContainer.worldBound.height;
            float tooltipToRenderTextureHeightRatio = tooltipSpaceHeight / renderTexture.height;

            // In worldspace, negative Y is down and positive Y is up
            float worldCornerMinY = worldCorners[0].y;
            float worldCornerMaxY = worldCorners[0].y;
            for (int i = 0; i < 4; i++) {
                worldCornerMinY = Mathf.Min(worldCornerMinY, worldCorners[i].y);
                worldCornerMaxY = Mathf.Max(worldCornerMaxY, worldCorners[i].y);
            }

            // Take the worldspace corners that we found for the entire canvas,
            // and adjust the bottom corners to be the bounds of the actual tooltip
            // area and not the entire canvas/render texture
            float adjustedMinY = worldCornerMaxY - ((worldCornerMaxY - worldCornerMinY) * tooltipToRenderTextureHeightRatio);
            for (int i = 0; i < 4; i++) {
                if (worldCorners[i].y == worldCornerMinY) {
                    worldCorners[i].y = adjustedMinY;
                }
            }

            // Convert to viewport positions
            Vector3[] vp = new Vector3[4];
            for (int i = 0; i < 4; i++)
                vp[i] = cam.WorldToViewportPoint(worldCorners[i]);

            // Track the minimum adjustment needed
            float minX = 0f;
            float minY = 0f;

            // If any corner is left of screen
            foreach (var c in vp)
            {
                if (c.x < 0) minX = Mathf.Max(minX, -c.x);
                if (c.x > 1) minX = Mathf.Min(minX, 1 - c.x);

                if (c.y < 0) minY = Mathf.Max(minY, -c.y);
                if (c.y > 1) minY = Mathf.Min(minY, 1 - c.y);
            }

            // If no adjustment needed, bail early
            if (minX == 0 && minY == 0)
                return;

            // Convert viewport adjustment to world-space delta
            Vector3 deltaWorld =
                cam.ViewportToWorldPoint(new Vector3(minX, minY, vp[0].z)) -
                cam.ViewportToWorldPoint(new Vector3(0, 0, vp[0].z));

            // Apply correction
            rectTransform.position += deltaWorld;
        });
    }
}

public enum TooltipContext {
    CompendiumCompanion,
    CompendiumCard,
    StartingTeam,
    Shop,
    CompanionManagementView
}