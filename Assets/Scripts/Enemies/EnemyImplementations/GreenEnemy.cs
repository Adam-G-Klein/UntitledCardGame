using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenEnemy : DefaultEnemy 
{
    string prefabName = "GreenEnemy";

    public override string getPrefabName(){
        return prefabName;
    }
    
}
