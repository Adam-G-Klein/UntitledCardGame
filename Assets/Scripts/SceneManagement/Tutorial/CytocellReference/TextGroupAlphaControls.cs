using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextGroupAlphaControls : MonoBehaviour
{

    //public List<TextMeshProUGUI> texts;
    public float displayTime = 0.7f;
    private TextMeshProUGUI[] textComps;
    public float initAlpha = 0;
    private bool testDisplaying = false;

    // Start is called before the first frame update
    void Start()
    {
        textComps = transform.GetComponentsInChildren<TextMeshProUGUI>();
        print("textcomps length: " + textComps.Length);

        foreach (TextMeshProUGUI tm in textComps)
        {
            tm.alpha = initAlpha;
        }
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (testDisplaying)
            {
                testDisplaying = false;
                hideAll();
            }
            else
            {
                testDisplaying = true;
                displayAll();
            }
        }
    }

    public void displayAll()
    {
        tweenAlphaTo(1, displayTime);
    }

    public void hideAll()
    {
        tweenAlphaTo(0, displayTime);
    }

    public void tweenAlphaTo(float to, float time)
    {
        foreach (TextMeshProUGUI tm in textComps)
        {
            LeanTween.value(
                gameObject, tm.alpha, to, time)
                .setOnUpdate((float val) =>
                {
                    tm.alpha = val;
                });
        }
    }
}

