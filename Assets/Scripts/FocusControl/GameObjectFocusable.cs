using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectFocusable : MonoBehaviour, IFocusableTarget
{
    [SerializeField] private UnityEvent focusEvent;
    [SerializeField] private UnityEvent unfocusEvent;
    [SerializeField] private List<GFGInputActionEvent> eventList;
    [SerializeField] private bool canFocusOffscreen;

    void OnEnable() {
        FocusManager.Instance.RegisterFocusableTarget(this);
    }

    void OnDisable() {
        FocusManager focusManager = FocusManager.CheckInstance;
        if (focusManager) focusManager.UnregisterFocusableTarget(this);
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

    public bool IsOnScreen() {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(gameObject.transform.position);

        // Check if it's in front of the camera
        if (viewportPoint.z < 0)
            return false;

        // Check if it's within the screen bounds
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
            viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }

    public Vector2 GetWorldspacePosition() {
        return transform.position;
    }

    public Vector2 GetUIPosition()
    {
        throw new System.NotImplementedException();
    }

    public bool CanFocusOffscreen() {
        return canFocusOffscreen;
    }

    public object GetCommonalityObject()
    {
        return null;
    }

    [System.Serializable]
    public class GFGInputActionEvent {
        public GFGInputAction action;
        public UnityEvent unityEvent;
    }
}
