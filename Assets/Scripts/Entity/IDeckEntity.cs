using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDeckEntity
{
    Deck getDeck();
    int getDealtPerTurn();
    void setDealtPerTurn(int dealtPerTurn);

}
