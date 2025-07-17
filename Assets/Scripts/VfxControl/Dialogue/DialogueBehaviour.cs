using UnityEngine;
using UnityEngine.Playables;

public class DialogueBehaviour : PlayableBehaviour
{
    public string dialogueText;
    public float revealSpeed;

    private float elapsedTime;
    private bool isShowing;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var uiController = playerData as DialogueUIController;
        if (uiController == null) return;

        double time = playable.GetTime();
        int charsToShow = Mathf.FloorToInt((float)(revealSpeed * time));
        charsToShow = Mathf.Clamp(charsToShow, 0, dialogueText.Length);

        uiController.SetText(dialogueText.Substring(0, charsToShow));
    }
}
