using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextRollout : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    [TextArea(5,5)]
    public string text;
    public float seconds;

    // Start is called before the first frame update
    void Start()
    {
        textMeshPro.text = "";
        StartCoroutine(UpdateText());
    }

    private IEnumerator UpdateText() {
        if (textMeshPro.text.Length == text.Length) {
            yield break;
        }
        yield return new WaitForSeconds(seconds);
        textMeshPro.text = text.Substring(0, textMeshPro.text.Length + 1);
        StartCoroutine(UpdateText());
    }
}
