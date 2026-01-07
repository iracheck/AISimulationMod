# Dynamic AI Simulation "GangWarSandbox"
**[Download on NexusMods](https://www.nexusmods.com/gta5/mods/1430)**

> Started as a latent research interest in optimization & AI, GangWarSandbox is now a fully-fledged combat utility for Grand Theft Auto V.
> Light development resumed as of December 2026, now open-source. 

Create your battlefield anywhere on the map and watch the AI fight for control of the map, then join in yourself for extra chaos.. <br>

### Tech Stack
Language: C#, .NET 4.8 <br>
Other: ScriptHookV, ScriptHookVDotNet, Native Library, LemonUI

### Technical Achievements

### Dependencies
- ScriptHookV
- ScriptHookVDotNet 3.7-nightly
- LemonUI 2.2 (or newer)
  
### How to Use
- Ensure all dependencies are installed
- Unpack mod file and drag everything within into your [gta5-path]/scripts folder.
- Run the game and use F10 to begin

### Open Source Information
See README. Generally, I don't have time to work on this anymore due to university so feel free to make modifications and publish them, as long as you don't claim my work as your own.

## Gamemode API
This repo also features an extensible API in the form of the "Gamemode" system. Gamemodes can selectively control AI behavior, squad targets, spawning rules, and user-configurable settings while reusing the core simulation engine, and without overwriting any code outside of the gamemode structure. It's still a WIP, but its totally functional.

Here's an example of how to create a gamemode:
### Creating a Custom Gamemode

GangWarSandbox exposes a plugin-style `Gamemode` API that allows developers to define custom rules, spawn logic, and AI behavior without modifying core systems. This is currently an Internal API only-- in the future it will work for multiple 

### Create a Gamemode Class
The constructor for the gamemode class takes the following:
arg0 = name, what appears in the selector
arg1 = description, what appears in the menu
arg2 = max teams, this is not necessary but it allows you to force a greater number of teams than are supported. Recommended to be less than 4 but nothing can stop you :)

```csharp
public class ExampleGamemode : Gamemode
{
    public ExampleGamemode()
        : base("Example", "Endless waves of enemies.", 4)
    {
        CaptureProgressMultiplier = 0.5f;
        PedHealthMultiplier = 1.2f;
    }
}
```

### Overrides
Beyond that, you must override the existing functions within the `Gamemode` class in order to implement new functionalities. The base class already includes a few important helpers, for example setting starting relationships or helping the AI recieve targets. However, in order to create something that is actually unique, there are a couple of very important methods that you might want to touch (and plenty more that you can discover for yourself, pending a real guide on this):
- `OnTick` is called 30 times per second
- `OnTickBattleRunning` is the same as above, but it only happens when the game is started
- `OnPlayerDeath` occurs when the player dies.
- `InitializeGamemode` is called as soon as the gamemode is first selected by the user
- `TerminateGamemode` is called as soon as a different gamemode is selected by the user
- `OnStart` is called as soon as the start button is pressed
- `OnEnd` is called as soon as the end button is pressed

### Parameters
These are the parameters that the user may (or may not, if thats what you want) modify.
```csharp
public GamemodeSpawnMethod SpawnMethod = GamemodeSpawnMethod.Spawnpoint; // options: "Spawnpoint", "Random"

// below is the default values. You can prevent users from modifying them by modifying the "EnableParameter_[x]" field.
public bool SpawnVehicles { get; set; } = true;
public bool SpawnWeaponizedVehicles { get; set; } = false;
public bool SpawnHelicopters { get; set; } = false;
public bool FogOfWar { get; set; } = true;
public float UnitCountMultiplier = 1;

// code-facing only
public float CaptureProgressMultiplier { get; set; } = 1.0f;
public float PedHealthMultiplier { get; set; } = 1.0f;
public bool HasTier4Ped = true;
```

## In Game Customization
Currently, you can create your own custom Factions and Vehicle Sets for those faction. The mod tries to parse everything within a given folder (e.g. ...scripts/GangWarSandbox/Factions) as what it expects to be in that folder, so be sure you are using the correct folders. To create a new Faction or VehicleSet, create a new .ini file and follow the instructions given in the Creation Guide provided.<br>

File Paths:
> scripts/GangWarSandbox/Factions  
> scripts/GangWarSandbox/VehicleSets

You can use the following links to find ped or vehicle models: <br>
https://docs.fivem.net/docs/game-references/vehicle-references/vehicle-models/  <br>
https://wiki.rage.mp/wiki/Peds <br>
https://docs.fivem.net/docs/game-references/weapon-models/


__Note__ <br>
Use the name of the model for peds and vehicles, e.g. "hc_driver" or "issi2" <br>
Use the name of the hash for weapons: e.g. "WEAPON_PISTOL" "WEAPON_MINIGUN"
