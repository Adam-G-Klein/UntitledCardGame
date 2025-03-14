using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public enum InputActionType {
    Hover,
    Unhover,
    Select
}
public class UIDocumentHoverableCallbackRegistry : GenericSingleton<UIDocumentHoverableCallbackRegistry>{

    private Dictionary<VisualElement, Action> selectCallbacksByElement = new Dictionary<VisualElement, Action>();
    private Dictionary<VisualElement, Action> hoverCallbacksByElement = new Dictionary<VisualElement, Action>();
    private Dictionary<VisualElement, Action> unhoverCallbacksByElement = new Dictionary<VisualElement, Action>();

    public void RegisterCallback(VisualElement element, InputActionType actionType, Action callback){
        switch(actionType){
            case InputActionType.Select:
                if(selectCallbacksByElement.ContainsKey(element)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + element + " already has a select callback registered");
                    return;
                }
                selectCallbacksByElement.Add(element, callback);
                break;
            case InputActionType.Hover:
                if(hoverCallbacksByElement.ContainsKey(element)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + element + " already has a hover callback registered");
                    return;
                }
                hoverCallbacksByElement.Add(element, callback);
                break;
            case InputActionType.Unhover:
                if(unhoverCallbacksByElement.ContainsKey(element)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + element + " already has an unhover callback registered");
                    return;
                }
                unhoverCallbacksByElement.Add(element, callback);
                break;
            default:
                Debug.LogError("UIDocumentHoverableCallbackRegistry: Invalid action type " + actionType);
                break;
        }
    }

    public void UnregisterAllCallbacks(VisualElement element){
        if(selectCallbacksByElement.ContainsKey(element)){
            selectCallbacksByElement.Remove(element);
        }
        if(hoverCallbacksByElement.ContainsKey(element)){
            hoverCallbacksByElement.Remove(element);
        }
        if(unhoverCallbacksByElement.ContainsKey(element)){
            unhoverCallbacksByElement.Remove(element);
        }
    }

    public void UnregisterCallback(VisualElement element, InputActionType actionType){
        switch(actionType){
            case InputActionType.Select:
                if(!selectCallbacksByElement.ContainsKey(element)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + element + " does not have a select callback registered");
                    return;
                }
                selectCallbacksByElement.Remove(element);
                break;
            case InputActionType.Hover:
                if(!hoverCallbacksByElement.ContainsKey(element)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + element + " does not have a hover callback registered");
                    return;
                }
                hoverCallbacksByElement.Remove(element);
                break;
            case InputActionType.Unhover:
                if(!unhoverCallbacksByElement.ContainsKey(element)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + element + " does not have an unhover callback registered");
                    return;
                }
                unhoverCallbacksByElement.Remove(element);
                break;
            default:
                Debug.LogError("UIDocumentHoverableCallbackRegistry: Invalid action type " + actionType);
                break;
        }
    }

    public void InvokeCallback(VisualElement element, InputActionType actionType){
        switch(actionType){
            case InputActionType.Select:
                if(!selectCallbacksByElement.ContainsKey(element)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + element + " does not have a select callback registered");
                    return;
                }
                if(selectCallbacksByElement[element] == null){
                    // a valid case, the element may not have a select callback
                    return;
                }
                selectCallbacksByElement[element].Invoke();
                break;
            case InputActionType.Hover:
                if(!hoverCallbacksByElement.ContainsKey(element)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + element + " does not have a hover callback registered");
                    return;
                }
                if(hoverCallbacksByElement[element] == null){
                    // a valid case, the element may not have a hover callback
                    return;
                }
                hoverCallbacksByElement[element].Invoke();
                break;
            case InputActionType.Unhover:
                if(!unhoverCallbacksByElement.ContainsKey(element)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + element + " does not have an unhover callback registered");
                    return;
                }
                if(unhoverCallbacksByElement[element] == null){
                    // a valid case, the element may not have an unhover callback
                    return;
                }
                unhoverCallbacksByElement[element].Invoke();
                break;
            default:
                Debug.LogError("UIDocumentHoverableCallbackRegistry: Invalid action type " + actionType);
                break;
        }
    }

    public bool HasCallback(VisualElement element, InputActionType actionType){
        switch(actionType){
            case InputActionType.Select:
                return selectCallbacksByElement.ContainsKey(element);
            case InputActionType.Hover:
                return hoverCallbacksByElement.ContainsKey(element);
            case InputActionType.Unhover:
                return unhoverCallbacksByElement.ContainsKey(element);
            default:
                Debug.LogError("UIDocumentHoverableCallbackRegistry: Invalid action type " + actionType);
                return false;
        }
    }

}