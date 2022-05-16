using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultEnemy : MonoBehaviour, Enemy
{
    string prefabName = "DefaultEnemy";
    public virtual string getPrefabName(){
        return prefabName;
    }


}
