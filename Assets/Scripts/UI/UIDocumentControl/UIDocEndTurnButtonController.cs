using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

public class UIDocEndTurnButtonController : MonoBehaviour {

    private UIDocumentScreenspace screenspaceDoc;

    [SerializeField]
    private TurnPhaseEvent turnPhaseEvent;

    private bool endTurnButtonEnabled = true;

    private VisualElement endTurnElement;

    void Start() {
        StartCoroutine(LateStart());
        
    }

    private IEnumerator LateStart() {
        yield return new WaitUntil(() => UIDocumentGameObjectPlacer.Instance.IsReady());
        screenspaceDoc = GetComponent<UIDocumentScreenspace>();
        if(screenspaceDoc == null) {
            Debug.LogError("UIDocEndTurnButtonController: No UIDocumentScreenspace found on this object");
            yield break;
        }

        endTurnElement = screenspaceDoc.GetVisualElement("end-turn");
        
        endTurnElement.pickingMode = PickingMode.Position;

        VisualElementUtils.RegisterSelected(endTurnElement, endTurnButtonHandler);
        IconButton endTurnButton = endTurnElement as IconButton;
        endTurnButton.SetIcon(GFGInputAction.END_TURN, ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.END_TURN));
        ControlsManager.Instance.RegisterIconChanger(endTurnButton);
    }

    public void endTurnButtonHandler() {
        if(endTurnButtonEnabled) {
            StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.BEFORE_END_PLAYER_TURN)));
        }
    }
    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info)
    {
        endTurnButtonEnabled = info.newPhase == TurnPhase.PLAYER_TURN;
        if(!endTurnButtonEnabled)
        {
            // endTurnElement.style.backgroundColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
        }
        screenspaceDoc.SetStateDirty();
    }

}