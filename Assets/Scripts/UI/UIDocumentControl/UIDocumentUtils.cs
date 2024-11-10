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
}
