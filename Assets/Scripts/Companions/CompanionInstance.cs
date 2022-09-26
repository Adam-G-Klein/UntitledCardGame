using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//stubbing this here for now for testing purposes
public class EventInfo {
    int scale;
    string target;
    public EventInfo(int scale, string target){
        this.scale = scale;
        this.target = target;
    }


}
public class CompanionInstance : MonoBehaviour
{
    public Companion companion;
    [Space(10)]
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        this.spriteRenderer.sprite = companion.companionType.sprite;
    }

    public List<CardInfo> companionDealHandler(EventInfo info){
        return null;


    }
}
