# Kingdoms-Reborn
 Unity files for Kingdoms Reborn

# Anatomy of a Scene

This is the anatomy of a basic scene. Prefabs are bolded. Find these prefabs in Assets/Prefabs/Game. The ">" character here represents a prefab with at least one child object. The following "-" characters represent the children themselves. Note, every object is assumed to have a Transform component.

| Hierarchy              | Inspector                 | Important Properties                                                 |
|------------------------|---------------------------|----------------------------------------------------------------------|
| StartPosition01        |                           |                                                                      |
| StartPosition02        |                           |                                                                      |
| **_GameManager**       | GameManager               |                                                                      |
|                        | UIManager                 |                                                                      |
|                        | CursorManager             |                                                                      |
|                        | AudioSource               | Spatial blend: 2D 100%                                               |
| **Minimap Camera**     | Camera                    | Culling Mask:  Minimap (layer)                                       |
|                        |                           | Target Texture: minimap-texture (Render texture)                     |
|                        | MinimapController         |                                                                      |
| > **RTS Camera**       | Camera                    | Transform Rotation: 75                                               |
|                        | RTSCamera                 |                                                                      |
|                        | Post-process Layer        |                                                                      |
|                        | Post-process Volume       |                                                                      |
|                        | AudioSource               |                                                                      |
|                        | AudioListener             |                                                                      |
| - **FogOfWarPlaneCamera**  | Camera                    | Clear Flags: Depth only                                              |
|                        |                           | Culling Mask: Fog Of War Plane                                       |
| **FogOfWarCamera**     | Camera                    | Clear Flags: Solid color                                             |
|                        |                           | Background: #0000FF                                                  |
|                        |                           | Culling Mask: Fog Of War                                             |
|                        |                           | Target Texture: FogOfWarRenderTexture                                |
| **FogOfWarPlane**      | MeshRenderer              | Material: FogOfWarPlane (Material); Shader Custom/FogOfWarMaskShader |
| **MapEdgePlane**       | (See FogOfWarPlane)       |                                                                      |
| **MapEdgeCamera**      | (See FogOfWarCamera)      |                                                                      |
| **MapEdgeRevealMask**  | MeshRenderer              | Material: sprite with color #00FF00                                  |
| **Canvas**             |                           |                                                                      |
| EventSystem            |                           |                                                                      |
| > Environment          |                           |                                                                      |
| - Terrain              | Terrain; Terrain Collider |                                                                      |
| - Terrain details?     |                           |                                                                      |
| - Sky?                 |                           |                                                                      |
| - (Water)              |                           |                                                                      |
| - Light                | Directional Light         |                                                                      |
| - WindZone             | WindZone                  |                                                                      |
| - MinimapCollider      | BoxCollider               |                                                                      |

Note the non-prefabbed objects: the start positions and the environment. Obviously prefabbing start positions is not necessary, and the environment cannot be prefabed because every scene (map) is distinct in it's environment setup. So, for example, a new Terrain object is needed for each scene, so as not to write to the same Terrain data from different scenes.

Canvases for different player factions are denoted by a faction prefix to the Canvas name, e.g. "AraCanvas", "TaroCanvas", "VeruCanvas", and "ZhonCanvas".
The Canvas prefab will also not automatically create an EventSystem object, so you will need to create that manually as well. 

# Anatomy of a Unit

E.g. components of Archer prefab:
Tag: Friendly | Layer: Unit
- Animator
- RigidBody
    - All units require a RigidBody with Is Kinematic checked.
- CapsuleCollider
    - All units require a Collider with Is Trigger unchecked.
- BaseUnit (Script)
    - Primary controller of the Unit. Inherits RTSUnit (Script). 
- Builder (Script)
- AttackBehavior (Script)
- ProjectileLauncher (Script)
    - Special attack behavior for projectile weapon. Works with AttackBehavior. 
- HumanoidUnitAnimator (Script)
    - Controls the Animator Controller. 
- UnitAudioManager (Script)
- AudioSource 
    - Spatial blend: 3D sound 100%, logarithmic rolloff. 
- NavMeshAgent
- NavMeshObstacle
- LineRenderer

## Unit children structure
- Unit
    - select-ring
    - minimap-icon: Layer (Minimap)
    - launch-point (only with ProjectileLauncher)
    - inner-trigger: Layer (Inner Trigger). Used to determine collisions with other units (bumping)
    - radar-trigger: Layer (Radar Trigger). Used to determine what units are within this unit's radar range
    - blood-system
    - fog-of-war-mask: Layer (Fog Of War)
    - MeleeWeaponScript on bone representing melee weapon: Layer (MeleeWeapon)

<img src="/Assets/Images/Documentation/melee-weapon-placement.PNG" height="300">

# NavMesh configuration
- Make sure to also bake a new NavMesh for each scene. Window->AI->Navigation->Bake.

# Lighting configuration
- Also make sure to bake a new lightmap for each scene. Window->Rendering->Lighting. 
- Choose an existing lighting configuration, e.g. SampleSceneSettings, or create a new one. Click Environment tab, assign Sun Source as the Environment/Light object, then click Generate Lighting. *Note: it's generally best to uncheck Auto Generate, to save resources.

# Types of Prefabs
All units have different states of coming into existence within the game. When a player clicks a unit in a Mage Builder's menu, a transparent form of that unit is instantiated that represents where the player may place the unit for construction. This is called a **Ghost** unit. 

Once construction begins on a Ghost, the unit becomes an "Intangible Mass" as it is conjured into existence. This type of unit is called an **Intangible** unit. All Intangibles eventually conjure into the unit's final form, which we will call the **RTSUnit**. 

## Intangibles
Some units begin at the Intangible state, and therefore do not have a Ghost prefab associated. For example, any unit being conjured by a Factory.

<img src="/Assets/Images/Documentation/Slide3.PNG" height="300">

## Ghosts
Units queued from Builders begin in the Ghost state, so the player can place the unit where desired.

<img src="/Assets/Images/Documentation/Slide1.PNG" height="300">

## Debris/Deads
Factories and other stationary defensive units, such as Strongholds, use an extra prefab for the Debris that is instantiated after the RTSUnit is destroyed.

<img src="/Assets/Images/Documentation/Slide2.PNG">

# Code Structure
## DarienEngine
- The DarienEngine is a namespace encompassing a variety of classes, functions, and constants that form the backbone of the game's functionality.

# Inheritance Structure
## RTSUnit
Most unit behavior is derived from RTSUnit. However, player units and AI units have distinct functionalities. Unit prefabs utilize either the BaseUnit or BaseUnitAI scipts to enable their behavior in the RTS engine.

<img src="/Assets/Images/Documentation/Slide5.jpg" height="300">

## UnitBuilder
Build behavior is separated logically between the Factory/Builder classes and AI/Player classes. Because multiple inheritance is not supported in C#, four separate classes model the different types of builders, and each derive from a parent encapsulating common functionality. For clarity of nomenclature, Factories are considered builders that are stationary and Builders are considered units that can move and can build.

<img src="/Assets/Images/Documentation/Slide4.jpg" height="300">

## Inventory
Inventory is also handled differently between players and AIs.

<img src="/Assets/Images/Documentation/Slide6.jpg" height="300">
