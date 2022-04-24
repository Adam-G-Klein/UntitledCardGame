using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextChanger2 : TextChangerParent 
{
    TextMeshPro text;
    int textNum = 2;

    void Start()
    {
        print("Start 2!");
        text = GetComponent<TextMeshPro>();
        //The cool finding is right here! getText is a function inherited from TextChangerParent
        text.SetText(getText(textNum));
        
    }

    void Update()
    {

        
    }
}
