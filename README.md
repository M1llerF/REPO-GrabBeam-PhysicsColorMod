# REPO-GrabBeam-PhysicsColorMod

A dynamic beam recoloring mod for the game REPO.  
**Beam colors now react to object mass and impact force**, allowing players to visually estimate the weight and handling difficulty of grabbed objects.

---

## Features

- Changes grab beam colors based on the **mass** of the object being held.
- Increases **brightness, saturation, and transparency** based on the **impact force** of the object.
- Special color effects when **grabbing players** or **entering rotation mode**.
- **Works in multiplayer**: beam colors update properly for both the local player and other players.
- **Efficiently optimized** for minimal CPU and GPU overhead (uses smart caching and batching).

---

## Installation

1. Install [BepInEx](https://github.com/BepInEx/BepInEx) for REPO if you haven't already.
2. Download the latest release `.zip` from this repository.
3. Extract the `.dll` file into your `BepInEx/plugins/` folder.
4. Launch REPO.  
5. Beam recoloring will be automatically enabled.

---

## Configuration

The mod generates a config file at:
`REPO\BepInEx\config`


You can customize:

- **Weight thresholds** (light vs heavy objects)
- **Beam base colors** (for light, medium, heavy weights)
- **Force scaling factors** (how much impact strength affects beam appearance)
- **Enable or disable debug logging**

---

## How It Works

- **Mass Detection**: Objects with a Rigidbody are categorized as light, medium, or heavy based on mass.
- **Impact Force Detection**: Objects with fragility and break force attributes modify beam brightness, saturation, and transparency dynamically.
- **Player Handling**: Grabbing a player uses a dedicated color.
- **Rotation Handling**: Rotating an object shifts beam colors temporarily.


---

## Credits

- Developed by M1llerF  

---

## License

This mod is released under the [MIT License](LICENSE).  
Feel free to modify, share, and expand upon it with attribution.

---

