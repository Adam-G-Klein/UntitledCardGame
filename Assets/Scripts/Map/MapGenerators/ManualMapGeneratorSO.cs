using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewManualMapGenerator", 
    menuName = "Map/Map Generator/Manual Map Generator")]
public class ManualMapGeneratorSO : MapGeneratorSO
{
    public Map map;

    public override Map generateMap()
    {
        return map;
    }
}
