using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// no idea what this class should look like right now
// Shouldn't implement entity the same way companions and 
// enemies do for sure though
public class Player 
{
    private int maxHealth = 10;
    private int health = 10;

    public int getHealth()
    {
        return health;
    }

    public int getMaxHealth()
    {
        return maxHealth;
    }

    public int changeHealth(int x)
    {
        health = health + x;
        return health;
    }
}
