using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(Button))]
[RequireComponent(typeof(BoolGameEventListener))]
public class CardSelectionUIConfirmButton : MonoBehaviour
{

    public VoidGameEvent selectionConfirmedEvent;
    private CardViewUI cardViewUI;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
        cardViewUI = GetComponentInParent<CardViewUI>();
    }
    
    private void OnButtonClick()
    {
        StartCoroutine(buttonClickCoroutine());
    }

    // have to wait for the selectionConfirmedEvent to be raised before destroying 
    // the view, as it destroys this object, the only reference to the event
    // raising coroutine
    private IEnumerator buttonClickCoroutine()
    {
        yield return StartCoroutine(selectionConfirmedEvent.RaiseAtEndOfFrameCoroutine(null));
        cardViewUI.exitView();
    }

    public void OnSelectionValidChanged(bool valid)
    {
        GetComponent<Button>().interactable = valid;
    }

}
