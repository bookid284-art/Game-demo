# Game-demo

## Overview
This repository is a Unity-based third-person horror prototype and graybox demo project. It includes a hospital-style level generator, an over-the-shoulder camera that follows the night nurse, a collectible key system, locked doors, an escape gate, and a monster AI that stops only when it is centered in her view.

The playable scene is `Assets/Scenes/Hospital.unity`. Pressing Play generates the hospital at runtime. Geometry is built from Unity primitives in code. Wall, floor, and prop textures are generated in code as well, so the project runs without an Asset Store download.

## Features
- HospitalSceneBuilder: builds a hospital-style graybox level using Unity primitives.
- ThirdPersonController: WASD movement and mouse-look, with the camera behind the nurse's right shoulder. The flashlight is in her hand.
- MonsterWatcherAI: the patient chases until it is in the center of the frame and in her line of sight. The dark space beside her body does not freeze it.
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
5. Move with WASD and the mouse. The camera stays behind her and pulls in against walls. Interact with E. Toggle the flashlight with F.

To preview the level in the editor before playing, select the HospitalSceneBuilder object and use the component menu **Build Hospital Graybox**, or use the top menu **Game Demo / Build Hospital Graybox**. Playing again rebuilds the level from code.

## Controls
- WASD: move
- Mouse: turn her, and the camera follows from behind her right shoulder
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

Press J for the checklist. Side tasks (Lin Xia's notes, pharmacy inventory, unfinished surgery, morgue tags, handover pages, and the quiet room) change the ending. The figure freezes while it sits in the center of the frame and a wall is not between her eyes and it. Standing beside her, at the edge of the wider third-person view, is not enough. Until the slip is printed, it will not follow you through the loading gate.

## Project Structure
- Assets/Scripts/: gameplay scripts and scene builder logic
- Assets/Docs/: usage guide and setup documentation
- Assets/Scenes/Hospital.unity: default scene
- Assets/Resources/Fonts/: subset font used by the Chinese UI
- ProjectSettings/: Unity project configuration

## Notes
- This project is a prototype. Batch-mode regression checks are described below.
- The AI and interaction systems are intentionally simple and suitable for extension.
- Optional material slots on HospitalSceneBuilder (`wallMaterial`, `floorMaterial`, `ceilingMaterial`, `doorMaterial`) replace the generated textures. Free Asset Store materials can be assigned there before generating the level.
- Keep the project on the Built-in Render Pipeline. The generated materials use the Standard shader.

## Regression checks
Run these commands from the project directory with Unity 2022.3 LTS (`Unity` means the full path to your Unity Editor executable). Use a separate batch-mode editor process with the project closed in the GUI:

```sh
Unity -batchmode -nographics -projectPath . -executeMethod HospitalAIRegressionTest.Run -logFile ai-regression.log
Unity -batchmode -nographics -projectPath . -executeMethod HospitalCameraRegressionTest.Run -logFile camera-regression.log
Unity -batchmode -nographics -projectPath . -executeMethod HospitalBatchTest.Run -logFile hospital-regression.log
```

`HospitalAIRegressionTest` exercises the production AI routing methods in an isolated scene: long corridors at 15/30/60/144 FPS and three speeds, patrol routing through room exits, changed goals at junctions, closed doors, and wall-blocked catches using Unity Physics. It exits with code 0 on success or 2 on failure. Movement in the routing cases is simulated; it does not replace a Play Mode test of CharacterController collision and steering.

`HospitalCameraRegressionTest` places the production third-person camera against walls and checks that the shoulder and boom stay clear.

`HospitalBatchTest` builds the full hospital and checks geometry, navigation, and progression. Its report path is `/tmp/hospital-test-result.txt`, so run that suite on macOS/Linux.

## Contributing
Pull requests and issues are welcome. Please include a short description and testing notes when submitting changes.

## License
No license is currently specified in the repository. Add a LICENSE file if you want to publish the project under a formal open-source license. The bundled UI font is a subset of WenQuanYi Micro Hei; see Assets/Docs/ThirdPartyNotices.md.
