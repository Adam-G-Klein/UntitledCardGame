using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewManualMapGenerator",
    menuName = "Map/Map Generator/Manual Map Generator")]
public class ManualMapGeneratorSO : MapGeneratorSO
{
    public Map map;
    public int playerStartingGold = 0;

    public ShopDataSO shopData;

    public override Map generateMap()
    {
        return map;
    }

    public override int getPlayerStartingGold()
    {
        return playerStartingGold;
    }

    public override ShopDataSO getShopData() {
        return shopData;
    }
}
