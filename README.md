# Untitled Card Game

Unity project source for Untitled Card Game by Go Face Games

## Dev Tips 

#### "You are trying to create a Monobehavior using the "new" keyword" warning

Unity doesn't get happy when we try and instantiate a class that extends Monobehavior using the "new" keyword. For our current use cases, you can just delete the ": Monobehavior" after the class name, and add an empty constructor. That could look something like this:

```
class ExampleClass {
    public ExampleClass(){}
    ...
}
```

#### Stack Overflow

Recording my foolishness here for all to see, had never managed to do something dumb like this in Unity before. May my folly save you time. Quite simply, I had these two static instantiations at the same time:

```
public class DefaultEncounter : Encounter {
    
    private Room originRoom = new StartRoom();
...
}

public class DefaultRoom : Room
{
    //test data set, overidden at worldGen
    private List<Encounter> encounters = new List<Encounter>()
    {
        // these will be encounters once they're created
        new DefaultEncounter(new Vector2(-7.5f, 4.2f)),
        new DefaultEncounter(new Vector2(2.3f, 4.5f)),
        new DefaultEncounter(new Vector2(8.6f, 0.35f))
    };
```
Don't let it be you :P

#### Literally trying to do anything with a button

Buttons don't seem to be playing nicely at all with DoNotDestroyOnLoad(), this is the second time I've tracked an issue back to it. There's methods of making unity objects clickable/hoverable that we'll use instead of Unity's button script. For now I've just been sticking stuff in KeyInputForTesting.cs and putting the prefab wherever I need it.