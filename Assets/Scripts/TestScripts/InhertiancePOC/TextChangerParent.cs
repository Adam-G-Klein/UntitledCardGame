using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextChangerParent : MonoBehaviour 
{
    private static Dictionary<int, string> wordDict = new Dictionary<int, string>(){
        {1, "text 1!"},
        {2, "text 2!"}
    };

    public string getText(int textNum){
        return wordDict[textNum];
    }
    
}
