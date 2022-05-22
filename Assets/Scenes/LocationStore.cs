using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LocationStore : MonoBehaviour
{
    //nest list for use in enemy placement
    private Dictionary<int, List<Vector2>> locDict2D = new Dictionary<int, List<Vector2>>();
    private Dictionary<int, Vector2> locDict1D = new Dictionary<int, Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        //function populates the locDicts as a side-effect
        populateLocDicts();
        // uncomment to debug
        // printDict(locDict);
    }

    public List<Vector2> getLocs(int enemyCount){
        return locDict2D[enemyCount];
    }

    public Vector2 getLoc(int key = 0){
        return locDict1D[key];
    }

    public int getTopLevelCount(){
        return locDict1D.Count;
    }

    void populateLocDicts(){
        Transform child;
        int childKey;
        for(int childInd = 0; childInd < transform.childCount; childInd +=1){
            child = transform.GetChild(childInd);
            // getting an error on the line below?
            // all the child objects of this object need to have integer names
            // so that we can programatically get their children into this map in the right place
            // So yeah make sure all the child objects of the object w this script on it
            // have integer parseable object names
            // (this was in liou of overcomplicating the implementation in the vertical slice)
            childKey = Int32.Parse(child.name);
            locDict2D.Add(childKey, getChildLocs(child));
            locDict1D.Add(childKey, child.position);
        }
    }

    List<Vector2> getChildLocs(Transform parent){
        Transform child;
        List<Vector2> locs = new List<Vector2>();
        for(int childInd = 0; childInd < parent.childCount; childInd +=1){
            child = parent.GetChild(childInd);
            locs.Add(child.position);
        }
        return locs;
    }

    void printDict(Dictionary<int,List<Vector2>>  dict){
        string outstr;
        foreach(KeyValuePair<int, List<Vector2>> kvp in dict){
            outstr = "";
            outstr += "Enemy Count: " + kvp.Key + "\n";
            foreach(Vector2 vec in kvp.Value){
                outstr += "\t" + vec.ToString() + "\n";
            }
        }

    }
}
