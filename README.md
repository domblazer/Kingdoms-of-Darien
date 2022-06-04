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

# Code Structure
## DarienEngine
- The DarienEngine is a namespace encompassing a variety of classes, functions, and constants that form the backbone of the game's functionality.

# Scene Setup

| Hierarchy              | Inspector                 | Important Properties                                                 |
|------------------------|---------------------------|----------------------------------------------------------------------|
| StartPosition01        |                           |                                                                      |
| StartPosition02        |                           |                                                                      |
| _GameManager           | GameManager               |                                                                      |
|                        | UIManager                 |                                                                      |
|                        | CursorManager             |                                                                      |
|                        | AudioSource               | Spatial blend: 2D 100%                                               |
| Minimap Camera         | Camera                    | Culling Mask:  Minimap (layer)                                       |
|                        |                           | Target Texture: minimap-texture (Render texture)                     |
|                        | MinimapController         |                                                                      |
| > RTS Camera           | Camera                    | Transform Rotation: 75                                               |
|                        | RTSCamera                 |                                                                      |
|                        | Post-process Layer        |                                                                      |
|                        | Post-process Volume       |                                                                      |
|                        | AudioSource               |                                                                      |
|                        | AudioListener             |                                                                      |
| - FogOfWarPlaneCamera  | Camera                    | Clear Flags: Depth only                                              |
|                        |                           | Culling Mask: Fog Of War Plane                                       |
| FogOfWarCamera         | Camera                    | Clear Flags: Solid color                                             |
|                        |                           | Background: #0000FF                                                  |
|                        |                           | Culling Mask: Fog Of War                                             |
|                        |                           | Target Texture: FogOfWarRenderTexture                                |
| FogOfWarPlane          | MeshRenderer              | Material: FogOfWarPlane (Material); Shader Custom/FogOfWarMaskShader |
| Canvas                 |                           |                                                                      |
| EventSystem            |                           |                                                                      |
| > Environment          |                           |                                                                      |
| - Terrain              | Terrain; Terrain Collider |                                                                      |
| - Terrain details?     |                           |                                                                      |
| - Sky?                 |                           |                                                                      |
| - (Water)              |                           |                                                                      |
| - Light                | Directional Light         |                                                                      |
| - WindZone             | WindZone                  |                                                                      |
| - MinimapCollider      | BoxCollider               |                                                                      |
| MapEdgePlane           | (See FogOfWarPlane)       |                                                                      |
| MapEdgeCamera          | (See FogOfWarCamera)      |                                                                      |
| MapEdgeRevealMask      | MeshRenderer              | Material: sprite with color #00FF00                                  |
