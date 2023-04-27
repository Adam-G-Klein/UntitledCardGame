using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionInstance : CombatEntityWithDeckInstance
{
    public Minion minion;

    protected override void Start() {
        base.Start();
        CombatEntityManager.Instance.registerMinion(this);
    }
}

