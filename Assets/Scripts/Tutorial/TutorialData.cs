using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TutorialData : MonoBehaviour
{
    [SerializeField]
    public string ID;

    [SerializeReference]
    public List<TutorialStep> Steps;

    public string nextTutorialName;
    public bool isNextTutorialSameScene;
}
