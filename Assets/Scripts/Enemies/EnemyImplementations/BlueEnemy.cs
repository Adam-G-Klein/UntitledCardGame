using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueEnemy : DefaultEnemy
{
    string prefabName = "BlueEnemy";

    public override string getPrefabName(){
        return prefabName;
    }
}
