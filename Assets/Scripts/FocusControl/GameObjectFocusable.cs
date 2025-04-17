using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectFocusable : MonoBehaviour, IFocusableTarget
{
    [SerializeField] private UnityEvent focusEvent;
    [SerializeField] private UnityEvent unfocusEvent;
    [SerializeField] private List<GFGInputActionEvent> eventList;

    void OnEnable() {
        FocusManager.Instance.RegisterFocusableTarget(this);
    }

    void OnDisable() {
        FocusManager.Instance.UnregisterFocusableTarget(this);
    }


    public void Focus() {
        focusEvent.Invoke();
    }

    public void Unfocus() {
        unfocusEvent.Invoke();
    }

    public bool ProcessInput(GFGInputAction action) {
        foreach (GFGInputActionEvent actionEvent in eventList) {
            if (actionEvent.action == action) {
                actionEvent.unityEvent.Invoke();
                return true;
            }
        }
        return false;
    }

    [System.Serializable]
    public class GFGInputActionEvent {
        public GFGInputAction action;
        public UnityEvent unityEvent;
    }
}
