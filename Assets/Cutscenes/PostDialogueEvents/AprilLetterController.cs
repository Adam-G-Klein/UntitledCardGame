using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AprilLetterController : MonoBehaviour
{
    private Image letterImage;
    private Button letterButton;

    [SerializeField]
    private float letterDisplayTime = 1f;


    void OnEnable() {
        Debug.Log("onenable");
        letterImage = GetComponent<Image>();
        letterButton = GetComponentInChildren<Button>();
        letterButton.onClick.AddListener(HideLetter);
        letterButton.onClick.AddListener(() => DialogueManager.Instance.StartAnyDialogueSequence(0));

    }

    public void DisplayLetter() {
        gameObject.SetActive(true);
        StartCoroutine(DisplayLetterCoroutine());
    }

    public IEnumerator DisplayLetterCoroutine() {
        // TODO, add whatever anims / sounds we want here
        letterImage.enabled = true;
        yield return null;
    }


    // to make it easily callable from a unity-serialized callback
    public void HideLetter() {
        StartCoroutine(HideLetterCoroutine());
    }

    public IEnumerator HideLetterCoroutine() {
        letterImage.enabled = false;
        gameObject.SetActive(false);
        yield return null;
    }

}
