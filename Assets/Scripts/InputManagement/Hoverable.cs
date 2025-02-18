using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Hoverable : MonoBehaviour {

    private List<IPointerEnterHandler> enterHandlers;
    private List<IPointerExitHandler> exitHandlers;
    private List<IPointerClickHandler> clickHandlers;

    void Start() {
        NonMouseInputManager.Instance.RegisterHoverable(this);
        enterHandlers = new List<IPointerEnterHandler>(GetComponents<IPointerEnterHandler>());
        exitHandlers = new List<IPointerExitHandler>(GetComponents<IPointerExitHandler>());
        clickHandlers = new List<IPointerClickHandler>(GetComponents<IPointerClickHandler>());
        foreach(IPointerEnterHandler handler in clickHandlers) {
            Debug.Log("[NonMouseInputControls] registered click handler " + handler.GetType().Name + " on " + gameObject.name);
        }
    }

    void OnDestroy() {
        // if the instance is destroyed, the game is shutting down.
        // if we don't check this, generic singleton will spawn the instance again
        // which causes an error in editor, and potentially a memory leak in the build
        if(!NonMouseInputManager.isDestroyed)
            NonMouseInputManager.Instance.UnregisterHoverable(this);
    }

    public void onHover() {
        Debug.Log("[NonMouseInputControls] hovering over " + gameObject.name);
        foreach(IPointerEnterHandler handler in enterHandlers) {
            handler.OnPointerEnter(null);
        }
    }

    public void onUnhover() {
        Debug.Log("[NonMouseInputControls] unhovering " + gameObject.name);
        foreach(IPointerExitHandler handler in exitHandlers) {
            handler.OnPointerExit(null);
        }
    }

    public void onSelect() {
        foreach(IPointerClickHandler handler in clickHandlers) {
            handler.OnPointerClick(null);
        }
    }

    public Vector2 getScreenPosition() {
        return Camera.main.WorldToScreenPoint(transform.position);
    }

}