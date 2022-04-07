using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextChanger1 : TextChangerParent
{

    TextMeshPro text;
    int textNum = 1;
    // Start is called before the first frame update
    void Start()
    {
        
        print("Start 1!");
        text = GetComponent<TextMeshPro>();
        //The cool finding is right here! getText is a function inherited from TextChangerParent
        text.SetText(getText(textNum));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
