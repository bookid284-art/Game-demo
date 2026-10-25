# Game-demo

## Overview
This repository is a Unity-based first-person horror prototype and graybox demo project. It includes a hospital-style level generator, basic movement and camera control, a collectible key system, locked doors, an escape gate, and a monster AI that reacts to whether the player is looking at it.

The playable scene is `Assets/Scenes/Hospital.unity`. Pressing Play generates the hospital at runtime. Geometry is built from Unity primitives in code. Wall, floor, and prop textures are generated in code as well, so the project runs without an Asset Store download.

## Features
- HospitalSceneBuilder: builds a hospital-style graybox level using Unity primitives.
- BasicFirstPersonController: standard WASD movement and mouse-look first-person controller with flashlight support.
- MonsterWatcherAI: monster chases the player when not seen and stops when the player looks at it.
- Interaction system: key pickup, locked door requiring a key, and escape door logic.
- Documentation: Assets/Docs/SetupGuide.md includes Chinese setup instructions for the project.

## Requirements
- Unity Editor 2022.3 LTS or a newer editor that can upgrade this project (Built-in Render Pipeline, not URP)
- A machine capable of building and running C# Unity projects

## Quick Start
1. Clone the repository:
   git clone https://github.com/bookid284-art/Game-demo.git
2. Open the project in Unity.
3. Open `Assets/Scenes/Hospital.unity`.
4. Press Play. The builder generates the hospital automatically.
5. Move with WASD and the mouse. Interact with E. Toggle the flashlight with F.

To preview the level in the editor before playing, select the HospitalSceneBuilder object and use the component menu **Build Hospital Graybox**, or use the top menu **Game Demo / Build Hospital Graybox**. Playing again rebuilds the level from code.

## Controls
- WASD: move
- Mouse: look
- Shift: sprint (uses stamina)
- E: pick up keys, read notes, open doors
- F or right mouse: toggle flashlight
- J: night checklist (the world does not pause)
- Number keys and Enter: door codes
- Esc: pause, or cancel a code
- R: restart after winning or dying

## How to escape
The night is nine steps, aimed at a blind playthrough of about five to six hours if you read the notes and finish the side tasks. Skipping the side tasks is shorter. Nothing in the level makes you wait on a timer.

1. East emergency wing: assemble the pharmacy code from four room notes, then stamp the terminal.
2. West administration: the keypad code is split between the break room and the meeting room. The archive holds *Observation Zero*, which has the surgery valve order and the psychiatry code.
3. Ward: storage key, exam key, then the call buttons in rooms 301, 305, and 306, then the nurse-station stamp.
4. The key in room 307 opens the loading gate only.
5. East of the loading bay: set the valves, then stamp surgery.
6. West of the bay: enter the psychiatry code, then stamp.
7. Return to the archive printer. Printing the evacuation slip releases the figure from the ward.
8. Open the north fence with the slip.
9. Run past the fence.

Press J for the checklist. Side tasks (Lin Xia's notes, pharmacy inventory, unfinished surgery, morgue tags, handover pages, and the quiet room) change the ending. The figure freezes while it is on screen and not behind a wall. Until the slip is printed, it will not follow you through the loading gate.

## Project Structure
- Assets/Scripts/: gameplay scripts and scene builder logic
- Assets/Docs/: usage guide and setup documentation
- Assets/Scenes/Hospital.unity: default scene
- Assets/Resources/Fonts/: subset font used by the Chinese UI
- ProjectSettings/: Unity project configuration

## Notes
- This project is a prototype and currently has no automated tests.
- The AI and interaction systems are intentionally simple and suitable for extension.
- Optional material slots on HospitalSceneBuilder (`wallMaterial`, `floorMaterial`, `ceilingMaterial`, `doorMaterial`) replace the generated textures. Free Asset Store materials can be assigned there before generating the level.
- Keep the project on the Built-in Render Pipeline. The generated materials use the Standard shader.

## Contributing
Pull requests and issues are welcome. Please include a short description and testing notes when submitting changes.

## License
No license is currently specified in the repository. Add a LICENSE file if you want to publish the project under a formal open-source license. The bundled UI font is a subset of WenQuanYi Micro Hei; see Assets/Docs/ThirdPartyNotices.md.
