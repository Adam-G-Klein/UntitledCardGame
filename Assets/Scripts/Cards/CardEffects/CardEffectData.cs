using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardEffectName {
    Draw,
    Damage,
    Buff
}

[Serializable]
public class CardEffectData
{
    // Okay Luke I'm conflicted.
    // Setting this in the editor every time will be a pain
    // But this looks so much worse and I think it's the only alternative?
    // https://docs.unity3d.com/ScriptReference/Resources.html
    // See CardInfo.Cast() for why this is a problem
    public CardEffectName effectName;
    public int scale;
}
