using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedEnemy : DefaultEnemy 
{
    string prefabName = "RedEnemy";

    public override string getPrefabName(){
        return prefabName;
    }
    
}
