using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestingIntEvent : MonoBehaviour
{
    public TextMeshPro text;

    public void setText(int num)
    {
        text.text = num.ToString();
    }
}
