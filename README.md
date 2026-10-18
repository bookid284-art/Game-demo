# Game-demo

## Overview
This repository is a Unity-based first-person horror prototype and graybox demo project. It includes a hospital-style level generator, basic movement and camera control, a collectible key system, locked doors, an escape gate, and a monster AI that reacts to whether the player is looking at it.

## Features
- HospitalSceneBuilder: builds a hospital-style graybox level using Unity primitives.
- BasicFirstPersonController: standard WASD movement and mouse-look first-person controller with flashlight support.
- MonsterWatcherAI: monster chases the player when not seen and stops when the player looks at it.
- Interaction system: key pickup, locked door requiring a key, and escape door logic.
- Documentation: Assets/Docs/SetupGuide.md includes Chinese setup instructions for the project.

## Requirements
- Unity Editor (recommended LTS version such as 2020.3, 2021.3, or 2022.x)
- A machine capable of building and running C# Unity projects

## Quick Start
1. Clone the repository:
   git clone https://github.com/bookid284-art/Game-demo.git
2. Open the project in Unity.
3. Create an empty scene or open an existing one.
4. Add an empty GameObject and attach HospitalSceneBuilder.
5. Use the component menu action "Build Hospital Graybox" to generate the scene.
6. Press Play to test the gameplay using WASD + mouse and E for interaction.

## Project Structure
- Assets/Scripts/: gameplay scripts and scene builder logic
- Assets/Docs/: usage guide and setup documentation
- ProjectSettings/: Unity project configuration

## Notes
- This project is a prototype and currently has no automated tests.
- The AI and interaction systems are intentionally simple and suitable for extension.

## Contributing
Pull requests and issues are welcome. Please include a short description and testing notes when submitting changes.

## License
No license is currently specified in the repository. Add a LICENSE file if you want to publish the project under a formal open-source license.
