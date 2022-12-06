using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectTargeter : MonoBehaviour
{

    private List<Transform> children;

    void Start(){
        children = new List<Transform>();
        foreach(Transform child in transform){
            children.Add(child);
        }
        hideArrow();
    }

    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info){
        showArrow();
        foreach(Transform child in transform){
            child.position = info.source.transform.position;
        }
    }

    private void showArrow(){
        foreach(Transform child in children){
            child.gameObject.SetActive(true);
        }
    }

    private void hideArrow(){
        foreach(Transform child in children){
            print("child: " + child);
            child.gameObject.SetActive(false);
        }
    }
}