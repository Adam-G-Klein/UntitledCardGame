using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TutorialData : MonoBehaviour
{
    [SerializeField]
    public int ID;

    [SerializeReference]
    public List<TutorialStep> Steps;
}
