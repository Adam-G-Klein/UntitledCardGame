# Untitled Card Game

Unity project source for Untitled Card Game by Go Face Games

## Add a status effect
* Add displayedEffect in CombatEffectProcedure.
* Add mapping displayed->combateffect in CombatEffectProcedure
* Add CombatEffect in CombatEffectEvent
* Add StatusEffect in StatusEffectDisplay
* Add mapping to status effect in CombatEffectEvent
* Add Initial value in CombatEntityInEncounterStats
* Add prefab to StatusEffectDisplays prefab, setting the enum value on the StatusEffectDisplay component
* (optional) add wearing off in CombatEntityFriendly
* (optional) add onDeath trigger in CombatEntityFriendly
* (optional) add effect on stats/effects in CombatEntityInEncounterStats

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

#### Things to try if your intellisense breaks (no error squigglies or function recommendations)

1. Reinstall the .NET SDK and restart your machine. Download [here](https://dotnet.microsoft.com/en-us/download)

2. In Unity, go to Edit -> Preferences -> External Tools
    - Make sure External Script Editor is pointed to your VsCode installation (The Visual Studio Code executable can be found at /Applications/Visual Studio Code.app on macOS, %localappdata%\Programs\Microsoft VS Code\Code.exe on Windows by default.)
    - Check all of the checkboxes under "create .csproj files for" and then click "Regenerate Project Files"

3. Reinstall VsCode

4. Reinstall Unity
