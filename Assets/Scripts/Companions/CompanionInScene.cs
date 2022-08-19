using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionInScene : MonoBehaviour, EntityInScene
{
    private Companion companion;

    public Companion getCompanion() {
        return companion;
    }

    public void setCompanion(Companion companion) {
        this.companion = companion;
    }

    public Entity getEntity() {
        return companion;
    }
}
