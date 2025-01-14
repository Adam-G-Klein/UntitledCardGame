using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/*
    This interface is implementing the visitor pattern for our encounter objects.
    The issue we see without this pattern is that each encounter needs to have
    identical build methods each called on scene load. If we want to make the
    build methods individual at all (passing different parameters to each) then
    we'd have needed to type cast. Using this pattern allows each IEncounterBuilder
    to build each encounter however it likes. The only "bad" part of this implementation
    is that each encounter builder has to implement the build function for something
    it doesn't care at all about.
*/
public interface IEncounterBuilder
{
    public void BuildEnemyEncounter(EnemyEncounter encounter, UIDocumentGameObjectPlacer placer);
    public void BuildShopEncounter(ShopEncounter encounter);
    public UIDocumentGameObjectPlacer placer { get; set; }
    public Transform transform { get; }



}
