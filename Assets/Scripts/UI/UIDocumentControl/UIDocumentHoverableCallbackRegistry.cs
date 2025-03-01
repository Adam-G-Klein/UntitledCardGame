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

    private Dictionary<string, Action> selectCallbacksByElementName = new Dictionary<string, Action>();
    private Dictionary<string, Action> hoverCallbacksByElementName = new Dictionary<string, Action>();
    private Dictionary<string, Action> unhoverCallbacksByElementName = new Dictionary<string, Action>();
    public string testInvokeCallback;
    public InputActionType testInvokeActionType;

    [ContextMenu("Invoke Test Callback")]
    void InvokeTestCallback(){
        InvokeCallback(testInvokeCallback, testInvokeActionType);
    }

    public void RegisterCallback(string elementName, InputActionType actionType, Action callback){
        switch(actionType){
            case InputActionType.Select:
                if(selectCallbacksByElementName.ContainsKey(elementName)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " already has a select callback registered");
                    return;
                }
                selectCallbacksByElementName.Add(elementName, callback);
                break;
            case InputActionType.Hover:
                if(hoverCallbacksByElementName.ContainsKey(elementName)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " already has a hover callback registered");
                    return;
                }
                hoverCallbacksByElementName.Add(elementName, callback);
                break;
            case InputActionType.Unhover:
                if(unhoverCallbacksByElementName.ContainsKey(elementName)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " already has an unhover callback registered");
                    return;
                }
                unhoverCallbacksByElementName.Add(elementName, callback);
                break;
            default:
                Debug.LogError("UIDocumentHoverableCallbackRegistry: Invalid action type " + actionType);
                break;
        }
    }

    public void UnregisterCallback(string elementName, InputActionType actionType){
        switch(actionType){
            case InputActionType.Select:
                if(!selectCallbacksByElementName.ContainsKey(elementName)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " does not have a select callback registered");
                    return;
                }
                selectCallbacksByElementName.Remove(elementName);
                break;
            case InputActionType.Hover:
                if(!hoverCallbacksByElementName.ContainsKey(elementName)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " does not have a hover callback registered");
                    return;
                }
                hoverCallbacksByElementName.Remove(elementName);
                break;
            case InputActionType.Unhover:
                if(!unhoverCallbacksByElementName.ContainsKey(elementName)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " does not have an unhover callback registered");
                    return;
                }
                unhoverCallbacksByElementName.Remove(elementName);
                break;
            default:
                Debug.LogError("UIDocumentHoverableCallbackRegistry: Invalid action type " + actionType);
                break;
        }
    }

    public void InvokeCallback(string elementName, InputActionType actionType){
        switch(actionType){
            case InputActionType.Select:
                if(!selectCallbacksByElementName.ContainsKey(elementName)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " does not have a select callback registered");
                    return;
                }
                if(selectCallbacksByElementName[elementName] == null){
                    // a valid case, the element may not have a select callback
                    return;
                }
                selectCallbacksByElementName[elementName].Invoke();
                break;
            case InputActionType.Hover:
                if(!hoverCallbacksByElementName.ContainsKey(elementName)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " does not have a hover callback registered");
                    return;
                }
                if(hoverCallbacksByElementName[elementName] == null){
                    // a valid case, the element may not have a hover callback
                    return;
                }
                hoverCallbacksByElementName[elementName].Invoke();
                break;
            case InputActionType.Unhover:
                if(!unhoverCallbacksByElementName.ContainsKey(elementName)){
                    Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " does not have an unhover callback registered");
                    return;
                }
                if(unhoverCallbacksByElementName[elementName] == null){
                    // a valid case, the element may not have an unhover callback
                    return;
                }
                unhoverCallbacksByElementName[elementName].Invoke();
                break;
            default:
                Debug.LogError("UIDocumentHoverableCallbackRegistry: Invalid action type " + actionType);
                break;
        }
    }

}