using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeothraAnimationController: MonoBehaviour
{

    [SerializeField] private Transform lhTarg;
    [SerializeField] private Transform lPole;
    [SerializeField] private Transform rhTarg;
    [SerializeField] private Transform rPole;

    [ContextMenu("Test")]
    public void contextMenuTest()
    {
        Debug.Log("testest");

    }
    

}
