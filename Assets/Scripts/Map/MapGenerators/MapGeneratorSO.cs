using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapGeneratorSO: ScriptableObject {
    public abstract Map generateMap();

    public abstract ShopDataSO getShopData();
}