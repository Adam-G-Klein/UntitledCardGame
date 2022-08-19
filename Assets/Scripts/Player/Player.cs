using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
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
