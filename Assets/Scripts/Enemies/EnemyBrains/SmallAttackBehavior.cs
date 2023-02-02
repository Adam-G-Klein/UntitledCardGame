using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SmallAttackBehavior: EnemyBehavior {
    public SmallAttackBehavior() {
        enemyBehaviorClassName = "SmallAttackBehavior";
    }

    // intent happens to be the same as the default behavior for now
}
