using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: have a discussion about Entity, I think it 
// may still be useful for UI stuff like this health bar
public class EnemyInstance : MonoBehaviour, Entity {
    public int currentHealth;
    public int baseAttackDamage;
    public EnemyTypeSO enemyType;
    [Space(5)]
    public SpriteRenderer spriteRenderer;
    public string id = Id.newGuid();
    
    // Start is called before the first frame update
    void Start() {
        this.currentHealth = enemyType.maxHealth;
        this.baseAttackDamage = enemyType.baseAttackDamage;
        this.spriteRenderer.sprite = enemyType.sprite;
    }

    public void cardEffectEventHandler(CardEffectEventInfo item){
        if(!item.targets.Contains(this.id)){
            return;
        }
        switch(item.effectName) {
            case CardEffectName.Draw:
                Debug.LogWarning("omg an enemy is drawing cards what happened");
                break;
            case CardEffectName.Damage:
                currentHealth -= item.scale;
                break;
            case CardEffectName.Buff:
                baseAttackDamage += item.scale;
                break;

        }

    }

    public int getHealth(){
        return currentHealth;
    }

    public int getMaxHealth() {
        return enemyType.maxHealth;
    }

}
