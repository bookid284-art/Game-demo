# 第一人称恐怖灰盒场景使用说明

## 快速生成场景
1. 在 Unity 中创建一个空物体 `SceneBuilder`。
2. 挂载脚本 `HospitalSceneBuilder`。
3. 在 Inspector 右上角菜单执行 **Build Hospital Graybox**。
4. 进入 Play，即可得到：医院走廊 + 3 个房间 + 终点门 + 逃生门 + 钥匙 + 怪物 + 昏暗灯光 + 手电筒。

## 各脚本挂载位置
- `HospitalSceneBuilder.cs`：挂在空物体 `SceneBuilder`（负责一键搭建灰盒场景）。
- `BasicFirstPersonController.cs`：挂在 `Player`（WASD 移动 + 鼠标视角）。
- `PlayerInventory.cs`：挂在 `Player`（背包状态，是否持有钥匙）。
- `KeyPickup.cs`：挂在 `Key`（按 `E` 拾取钥匙）。
- `DoorController.cs`：挂在 `EndDoor`（按 `E` 开门，默认需要钥匙）。
- `EscapeDoor.cs`：挂在 `EscapeExit`（按 `E` 逃生，要求有钥匙）。
- `MonsterWatcherAI.cs`：挂在 `BlackHumanoidMonster`（被看见时停止，未被看见时接近玩家）。

## 交互规则
- `E`：交互（拾取钥匙/开门/逃生）。
- 怪物行为：
  - 玩家相机正看向怪物且中间无遮挡：怪物停止。
  - 玩家没看怪物或视线被遮挡：怪物向玩家移动。

## 可调参数（Inspector）
- `BasicFirstPersonController`：`moveSpeed`、`lookSensitivity`。
- `DoorController`：`requiresKey`、`interactDistance`、`openedRotation`。
- `MonsterWatcherAI`：`moveSpeed`、`viewThreshold`、`stopDistance`。
- `EscapeDoor`：`winSceneName`（可选，填了会切场景）。
