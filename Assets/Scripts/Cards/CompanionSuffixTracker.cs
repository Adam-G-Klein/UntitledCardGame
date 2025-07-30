using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;


[System.Serializable]
public class CompanionSuffixCount
{
    public CompanionTypeSO companionTypeSO;
    public int numUsed;
    public CompanionSuffixCount(CompanionTypeSO companionTypeSO)
    {
        this.companionTypeSO = companionTypeSO;
    }

    public CompanionSuffixCount(CompanionSuffixCountSerializable companionSuffixCountSerializable, SORegistry sORegistry)
    {
        this.companionTypeSO = sORegistry.GetAsset<CompanionTypeSO>(companionSuffixCountSerializable.companionTypeGuid);
        this.numUsed = companionSuffixCountSerializable.numUsed;
        Debug.Log("[SuffixTracker] Loaded companion " + companionTypeSO.name + " with suffix count " + numUsed); 
    }
}


[Serializable]
public class CompanionSuffixTracker
{
    public List<CompanionSuffixCount> companionSuffixCounts = new List<CompanionSuffixCount>();

    public CompanionSuffixTracker()
    {
    }

    public CompanionSuffixTracker(CompanionSuffixTrackerSerializable companionSuffixTrackerSerializable, SORegistry registry)
    {
        foreach (CompanionSuffixCountSerializable companionSuffixCountSerializable in companionSuffixTrackerSerializable.companionSuffixCountSerializables)
        {
            companionSuffixCounts.Add(new CompanionSuffixCount(companionSuffixCountSerializable, registry));
        }
    }

    public string GetCompanionName(CompanionTypeSO companionTypeSO)
    {
        // TODO, search through the suffix counts using some list comprehension
        // TODO, increment if needed, add if needed, return index into a big static map

        return "";
    }

}

[System.Serializable]
public class CompanionSuffixCountSerializable
{
    public string companionTypeGuid;
    public int numUsed;
    public CompanionSuffixCountSerializable(CompanionSuffixCount companionSuffixCount)
    {
        companionTypeGuid = companionSuffixCount.companionTypeSO.GUID;
        numUsed = companionSuffixCount.numUsed;
    }
}

[System.Serializable]
public class CompanionSuffixTrackerSerializable
{

    public List<CompanionSuffixCountSerializable> companionSuffixCountSerializables;
    public CompanionSuffixTrackerSerializable(CompanionSuffixTracker tracker)
    {
        foreach(CompanionSuffixCount companionSuffixCount in tracker.companionSuffixCounts) {
            companionSuffixCountSerializables.Add(new CompanionSuffixCountSerializable(companionSuffixCount));
        }
    }

}

public static class SuffixList {
    
// too lazy to if-check the last digit and add "rd" or "st"
// eh maybe that's a good idea
public static readonly List<string> Suffixes = new List<string>
{
    "The 1st",
    "The 2nd",
    "The 3rd",
    "The 4th",
    "The 5th",
    "The 6th",
    "The 7th",
    "The 8th",
    "The 9th",
    "The 10th",
    "The 11th",
    "The 12th",
    "The 13th",
    "The 14th",
    "The 15th",
    "The 16th",
    "The 17th",
    "The 18th",
    "The 19th",
    "The 20th",
    "The 21st",
    "The 22nd",
    "The 23rd",
    "The 24th",
    "The 25th",
    "The 26th",
    "The 27th",
    "The 28th",
    "The 29th",
    "The 30th",
    "The 31st",
    "The 32nd",
    "The 33rd",
    "The 34th",
    "The 35th",
    "The 36th",
    "The 37th",
    "The 38th",
    "The 39th",
    "The 40th",
    "The 41st",
    "The 42nd",
    "The 43rd",
    "The 44th",
    "The 45th",
    "The 46th",
    "The 47th",
    "The 48th",
    "The 49th",
    "The 50th",
    "The 51st",
    "The 52nd",
    "The 53rd",
    "The 54th",
    "The 55th",
    "The 56th",
    "The 57th",
    "The 58th",
    "The 59th",
    "The 60th",
    "The 61st",
    "The 62nd",
    "The 63rd",
    "The 64th",
    "The 65th",
    "The 66th",
    "The 67th",
    "The 68th",
    "The 69th",
    "The 70th",
    "The 71st",
    "The 72nd",
    "The 73rd",
    "The 74th",
    "The 75th",
    "The 76th",
    "The 77th",
    "The 78th",
    "The 79th",
    "The 80th",
    "The 81st",
    "The 82nd",
    "The 83rd",
    "The 84th",
    "The 85th",
    "The 86th",
    "The 87th",
    "The 88th",
    "The 89th",
    "The 90th",
    "The 91st",
    "The 92nd",
    "The 93rd",
    "The 94th",
    "The 95th",
    "The 96th",
    "The 97th",
    "The 98th",
    "The 99th"
};
}