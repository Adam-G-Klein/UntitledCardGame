using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// implemented by both companions and minions 
// for the purpose of consolidated card dealing code
public interface CombatEntityWithDeck : CombatEntityBaseStats
{
    Deck getDeck();

}
