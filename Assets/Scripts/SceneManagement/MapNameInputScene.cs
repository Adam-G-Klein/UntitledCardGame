using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNameInputScene : GameScene 
{
    
    private string sceneString = "Scenes/Menus/MapNameInput";
    public virtual string getSceneString()
    {
        return sceneString;
    }

    public virtual void build()
    {
        //nothing necessary here, MapManager is present in the scene
        // and will  kick itself off in the Start method
    }

}
