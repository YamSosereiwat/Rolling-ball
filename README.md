# Rolling Ball

Rolling Ball is a 3D physics-based platform game. Guide a rolling ball across a single obstacle course, collect silver orbs, use spring pads, avoid hazards, and reach the finish gate. Collectibles add score but are not required to win.

## Gameplay

### Controls

| Input | Action |
| --- | --- |
| W / Up Arrow | Move forward relative to the camera |
| S / Down Arrow | Move backward relative to the camera |
| A / Left Arrow | Move left |
| D / Right Arrow | Move right |
| Space | Jump while grounded |
| Left Shift | Boost movement speed while held with a movement direction |
| Escape | Pause or resume |

Movement uses Rigidbody physics. The ball accelerates and decelerates toward its target speed, receives reduced control in the air, and rolls from applied torque. Jumping is available only while grounded. The camera follows the ball and turns smoothly toward its direction of travel; it also moves closer when an obstacle blocks its view.

### Orbs and score

The level's `coin-silver` objects are configured as collectible orbs when the scene starts. They receive trigger colliders and collectible behavior at runtime. Orbs spin and bob, award score when touched by the player, and disappear after collection.

The HUD shows the score and a live **Orbs Collected** counter in `collected / total` form. Both the Victory and Game Over screens show the final score and collected-orb count. The finish gate does not require collecting all orbs.

### Finish, hazards, and game states

- **Victory:** entering the designated `FinishPoint` trigger at the finish gate. The finish must be tagged `Finish` and only the player can activate it.
- **Game Over:** touching a `DeadlyObstacle` (the solid dangerous walls or spike-trap death zones), entering the `LosingArea`, or falling below the GameManager's configured fall height (`-15` by default).
- **Safe course pieces:** ordinary walls, ramps, platforms, and grass blocks do not cause contact-based Game Over. The fall-height backup triggers only below the configured boundary.
- **Spring pads:** trigger zones launch the player upward with an impulse.
- **Pause:** Escape toggles between Running and Paused. No pause panel is assigned in the current scene, so pausing stops gameplay without opening a separate menu.
- **Restart:** the end-panel button reloads the active scene.

The GameManager owns the score, orb totals, and game state. Victory and Game Over stop the player's Rigidbody motion and show the result panel.

## Game flow

The project currently launches directly into the playable scene and does not include a title screen or separate Start Game menu.

```text
START → Initialize Game
  ↓
W/A/S/D + Arrow Keys + Space + Left Shift + Escape
  ↓
PlayerController – movement, acceleration, jump, boost, and rolling torque
  ↓
Is Left Shift held while moving?
  ├─ YES → Activate speed boost
  └─ NO  → Normal movement speed
  ↓
Space pressed?
  ├─ YES → Is player grounded?
  │        ├─ YES → Apply jump
  │        └─ NO  → No jump
  └─ NO  → Continue gameplay
  ↓
Collect silver orb → GameManager updates score and orb count → UIManager updates HUD
  ↓
Ball touches spring pad → SpringPad applies upward impulse → Continue gameplay
  ↓
Escape pressed?
  ├─ YES → Toggle Running / Paused
  └─ NO  → Continue gameplay
  ↓
Reach finish gate → Victory
  ↓
Touch deadly obstacle / enter losing area / fall below threshold → Game Over
```

## Project setup

- **Unity:** `6000.3.8f1`
- **Input:** Unity Input System `1.18.0`
- **Rendering:** Universal Render Pipeline `17.3.0`
- **UI:** uGUI `2.0.0`

Open the project root in Unity Hub with the specified Unity version and allow package resolution and asset import to finish. The enabled build scene is `Assets/Scenes/Scenes/SampleScene.unity`; open it and press **Play** to run the game. Use **File > Build Profiles** to configure a platform build.

## Main systems

- `PlayerController` reads keyboard input, controls camera-relative movement, applies acceleration, boost, jump, air control, and rolling torque.
- `CameraFollow` tracks the player, follows horizontal Rigidbody velocity, smooths position and rotation, and sphere-casts to avoid clipping through course geometry.
- `GameManager` tracks score and collectible counts, handles Running/Paused/Victory/Game Over, checks the fall-height backup, and restarts the active scene.
- `CollectibleSceneSetup` finds scene objects whose names begin with `coin-silver` and configures them as collectibles. Update its prefix if collectible objects are renamed.
- `FinishPoint`, `LosingArea`, and `DeadlyObstacle` handle the finish gate and loss conditions.
- `SpringPad` applies the spring impulse. `Obstacle` provides optional move, rotate, and fall-on-touch behaviors for obstacles that are configured with that component.
- `UIManager` updates the HUD, end messages, final score and orb count, and Restart action.

## Project layout

```text
Assets/
  Scenes/Scenes/SampleScene.unity   Main and currently only build scene
  Scripts/                           Gameplay and UI scripts
  kenney_platformer-kit/             Course models and related assets
Packages/manifest.json               Unity package dependencies
ProjectSettings/                    Unity project and build settings
```

Course models use imported colliders where configured in their model import settings. If a newly imported or changed course model does not block the player, check its collider import settings and the player's Ground Mask in the Inspector.
