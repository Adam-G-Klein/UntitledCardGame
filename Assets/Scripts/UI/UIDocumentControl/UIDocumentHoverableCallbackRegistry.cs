using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIDocumentHoverableCallbackRegistry : GenericSingleton<UIDocumentHoverableCallbackRegistry>{

    private Dictionary<string, Action> callbacksByElementName = new Dictionary<string, Action>();
    public string testInvokeCallback;

    [ContextMenu("Invoke Test Callback")]
    void InvokeTestCallback(){
        InvokeCallback(testInvokeCallback);
    }

    public void RegisterCallback(string elementName, Action callback){
        if(callbacksByElementName.ContainsKey(elementName)){
            Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " already has a callback registered");
            return;
        }
        callbacksByElementName[elementName] = callback;
    }

    public void UnregisterCallback(string elementName){
        if(!callbacksByElementName.ContainsKey(elementName)){
            Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " does not have a callback registered");
            return;
        }
        callbacksByElementName.Remove(elementName);
    }

    public void InvokeCallback(string elementName){
        if(!callbacksByElementName.ContainsKey(elementName)){
            Debug.LogError("UIDocumentHoverableCallbackRegistry: Element name " + elementName + " does not have a callback registered");
            return;
        }
        callbacksByElementName[elementName].Invoke();
    }

}