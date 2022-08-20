using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionInstance : MonoBehaviour
{
    public Companion companion;
    [Space(10)]
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        this.spriteRenderer.sprite = companion.companionType.sprite;
    }
}
