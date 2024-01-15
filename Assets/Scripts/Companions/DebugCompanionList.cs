using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// WIP - not currently used
// A list of companion types you want to have while testing
// Allows you to not have stats and deck changes carry over between
// test runs in the editor
[System.Serializable]
public class DebugCompanionList
{

    public List<CompanionTypeSO> companions;
}
