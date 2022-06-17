using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventBus.Events;

public class DamageEvent : EventBase
{
    private Entity originEntity;
    private Entity destinationEntity;
    private int damage;

    public DamageEvent(
            Entity originEntity,
            Entity destinationEntity,
            int damage) {
        this.damage = damage;
        this.destinationEntity = destinationEntity;
        this.originEntity = originEntity;
    }

    public Entity getOriginEntity() {
        return originEntity;
    }

    public Entity getDestinationEntity() {
        return destinationEntity;
    }

    public int getDamage() {
        return damage;
    }
}
