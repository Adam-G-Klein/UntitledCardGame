using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class CardCastArguments {

    public List<string> targets;

    public CardCastArguments (List<string> targets) { 
        this.targets = targets;
    }
}
