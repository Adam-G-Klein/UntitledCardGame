using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventBus.Events;

public class DamageEvent : EventBase
{
    private TargetableEntity originEntity;
    private TargetableEntity destinationEntity;
    private int damage;

    public DamageEvent(
            TargetableEntity originEntity,
            TargetableEntity destinationEntity,
            int damage) {
        this.damage = damage;
        this.destinationEntity = destinationEntity;
        this.originEntity = originEntity;
    }

    public TargetableEntity getOriginEntity() {
        return originEntity;
    }

    public TargetableEntity getDestinationEntity() {
        return destinationEntity;
    }

    public int getDamage() {
        return damage;
    }
}
