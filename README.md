# Kingdoms-Reborn
 Unity files for Kingdoms Reborn

# Setting up a new scene
The most basic setup for a scene contains the following game objects:
- _GameManager
- RTS Camera
    - FogOfWarPlaneCamera
- Minimap Camera
- Environment
    - Terrain*
    - Light
    - WindZone
    - Minimap Collider
- FogOfWarCamera
- FogOfWarPlane
- {{FACTION_PRE}}Canvas
- EventSystem

These objects are all prefabed under Assets/Prefabs/Game. The Environment prefab, however, will not contain a Terrain. You will need to create a new Terrain object for each scene, so as not to write to the same Terrain data from different scenes.

Canvases for different player factions are denoted by a faction prefix to the Canvas name, e.g. "AraCanvas", "TaroCanvas", "VeruCanvas", and "ZhonCanvas".
The Canvas prefab will also not automatically create an EventSystem object, so you will need to create that manually as well. 

## _GameManager setup
The _GameManager object contains the GameManager, UIManager, and CursorManager scripts. 
### GameManager (Script) setup
- Player Configs: Enter number of players and relevant player number, team number, and faction variables. 
- Fog Of War Plane: Set FogOfWarPlane game object.
### UIManager (Script) setup
- Battle Menu Default: Usually found at Canvas/BattleMenu/background-images/battle-menu-default
- F4 Menu: Set to F4Menu UI object
- Build Menus: Set each build menu from Canvas/BuildMenus

## Minimap Camera setup
- Minimap Controller (Script): Set Map Collider to Environment/Minimap Collider game object.

## Environment setup
- If Environment is brought in as a prefab, make sure to unpack it completely. Again, this is so we are not conflicting Terrains across scenes.

## NavMesh configuration
- Make sure to also bake a new NavMesh for each scene. Window->AI->Navigation->Bake.

## Lighting configuration
- Also make sure to bake a new lightmap for each scene. Window->Rendering->Lighting. 
- Choose an existing lighting configuration, e.g. SampleSceneSettings, or create a new one. Click Environment tab, assign Sun Source as the Environment/Light object, then click Generate Lighting. *Note: it's generally best to uncheck Auto Generate, to save resources.

