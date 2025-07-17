using UnityEngine;
using TMPro; // Or UnityEngine.UI if you're not using TextMeshPro

public class DialogueUIController : MonoBehaviour
{
    public TMP_Text dialogueText; // Or public Text dialogueText;

    public void SetText(string text)
    {
        dialogueText.text = text;
    }
}
