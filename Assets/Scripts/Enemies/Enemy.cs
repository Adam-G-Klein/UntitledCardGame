using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Enemy : Entity
{
    string getPrefabName();
    string getId();
    void setId(string id);
    void buildEnemy(GameObject prefab, Vector2 location);
}
