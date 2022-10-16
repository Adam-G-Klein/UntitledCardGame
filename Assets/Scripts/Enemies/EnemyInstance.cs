using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstance : MonoBehaviour {
    public int currentHealth;
    public EnemyTypeSO enemyType;
    [Space(5)]
    public SpriteRenderer spriteRenderer;
    
    // Start is called before the first frame update
    void Start() {
        this.currentHealth = enemyType.maxHealth;
        this.spriteRenderer.sprite = enemyType.sprite;
    }
}
