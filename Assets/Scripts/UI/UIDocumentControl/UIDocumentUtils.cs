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

    public static VisualElement GetRootElement(VisualElement ve){
        if (ve.parent == null){
            return ve;
        }
        return GetRootElement(ve.parent);
    }

    public static bool ElementIsReady(VisualElement ve){
        if (ve == null){
            return false;
        }
        // Debug.Log("[HoverableInstantiation] ve.worldBound.width: " + ve.worldBound.width);
        if (float.IsNaN(ve.worldBound.width) || ve.worldBound.width == 0){
            return false;
        }
        return true;
    }

    public static int UpdateTextSize(string desc, int maxChar, int fontSize, int scaleFactor = 4){
        if (desc.Length > maxChar){
            float textSizeRatio = (float) maxChar / (float) desc.Length;
            double scalingRatio = Math.Pow(textSizeRatio, (float)1/ (float)scaleFactor);
            return (int)Math.Floor(fontSize * scalingRatio);
        }
        return fontSize;
    }
}
