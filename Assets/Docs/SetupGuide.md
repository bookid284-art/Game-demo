# 夜班医院 — 使用说明

这是一个第一人称恐怖灰盒原型。医院、病床、怪物和手电筒都由代码用 Unity 自带的方块、球体、胶囊和圆柱搭出来。墙面和地面贴图也在生成关卡时画出来，不需要先去资源商店下载素材。

## 环境

- 建议使用 Unity 2022.3 LTS。用更新的编辑器打开时，按提示升级工程即可。
- 必须使用内置渲染管线（Built-in）。不要把工程切到 URP 或 HDRP，否则 Standard 着色器会变成洋红色。
- 第一次打开会导入脚本和字体，等右下角进度走完再点 Play。

## 开始游戏

1. 用 Unity Hub 打开仓库根目录。
2. 打开场景 `Assets/Scenes/Hospital.unity`。
3. 点击 Play。

播放开始时，`HospitalSceneBuilder` 会清掉旧的 `HospitalRoot` 并重新生成整座医院。场景视图里还没生成时，能看到淡蓝色的房间线框。

也可以在不进入播放模式时预览：

- 选中 `HospitalSceneBuilder`，在组件标题的菜单里点 **Build Hospital Graybox**。
- 或使用顶部菜单 **Game Demo / Build Hospital Graybox**。
- **Game Demo / Clear Hospital Graybox** 会删掉已经生成的关卡。

编辑器里生成的贴图会写到 `Assets/Art/Generated`（已被 git 忽略）。点 Play 时关卡会再生成一次，所以在生成结果上做的手工改动不会保留。想手摆关卡时，先关掉组件上的 `Build On Awake`。

## 操作

| 操作 | 按键 |
| --- | --- |
| 移动 | WASD 或方向键 |
| 视角 | 鼠标 |
| 冲刺 | Shift（消耗体力，耗尽后要缓一下） |
| 交互 | E（拾取、阅读、开门） |
| 手电筒 | F 或鼠标右键 |
| 暂停 | Esc |
| 失败或通关后重开 | R，或点界面按钮 |

鼠标灵敏度在 `HospitalSceneBuilder` 的 `Mouse Sensitivity` 上。改完后需要重新生成关卡，或者在播放时直接改 Player 上的 `BasicFirstPersonController`。

## 怎么过关

医院是一条往北的单线：大厅、护士站、病房走廊、紧急出口。

1. 大厅西侧库房里有库房钥匙，用来开大厅北门。
2. 护士站东侧诊室里有诊室钥匙，用来开病房区的门。诊室和办公室的门不用钥匙，靠近按 E 即可。
3. 307 病房床边有出口钥匙。拿到它之后，走廊尽头的闸门才能打开。
4. 打开闸门，走进外面的停车处。

前台、护士站和 307 床卡可以按 E 阅读。阅读时你不能移动，但怪物还在走。

卫生间是空的，没有钥匙。

## 怪物

病房深处有一个站着的病人。规则只有一条：

- 它出现在屏幕里，而且中间没有墙挡住时，会停住。
- 你把视线移开，或者它被墙挡住，它就继续追。
- 贴到你身上、而你当时又没看着它，游戏结束。
- 盯着它的时候撞上去不会立刻死亡，可以贴着它挪开。
- 门锁上时它过不来。病房门中间有一扇窗，隔着门也能看见它。
- 你逃出大门之后，它停在门口，不会跟到外面。

冲刺比它快，但体力有限。走路比它慢。正确的节奏是看着它停住，再撤到侧门或转过拐角。

手电筒会耗电。大厅前台、诊室柜子上和 305 病房附近有电池。灯灭了并不影响“看着它就会停”的判定，只是你更难看清它。

## 脚本分别做什么

- `HospitalSceneBuilder`：生成关卡、玩家、怪物、门和 UI。
- `BasicFirstPersonController`：行走、冲刺、鼠标视角。
- `FlashlightController`：手电筒和电量。
- `PlayerInteractor`：屏幕中心射线，按 E 交互。
- `PlayerInventory`：钥匙。
- `KeyPickup` / `BatteryPickup` / `NoteInteractable`：可拾取和可阅读物体。
- `DoorController`：普通门。`requiredKeyId` 为空表示不用钥匙。
- `EscapeDoor`：紧急出口闸门。
- `MonsterWatcherAI`：视线冻结和追击。
- `HospitalNavGraph`：走廊路点。关门的边不会被寻路走过。
- `GameManager` / `GameHUD` / `GameAudio`：流程、界面和合成音效。

生成结束后，Console 里如果出现“导航被几何体挡住”，说明某条路点直线穿进了墙或大型道具，需要改路点或挪开道具。

## 换成资源商店里的免费材质

建模仍然是代码里的原始体。如果想用 Unity Asset Store 的免费贴图，把材质拖到 `HospitalSceneBuilder` 上的这些槽里，再重新生成：

- `Wall Material`
- `Floor Material`
- `Ceiling Material`
- `Door Material`

这些槽留空时，使用代码生成的脏墙面、地砖和门。Asset Store 的免费包通常不允许把原包再上传到公开仓库，所以工程里没有直接塞进那些资源。

## 常见问题

- 画面全是洋红色：工程被切到了 URP。在 Project Settings 里改回 Built-in，或重新用 Built-in 模板打开。
- 中文变成方框：确认 `Assets/Resources/Fonts/GameCJK.ttf` 已导入。没有这套字体时，脚本会尝试系统里的微软雅黑、苹方或文泉驿。
- 视角太快或太慢：改 `Mouse Sensitivity`。输入使用旧版 Input Manager，鼠标轴的灵敏度是 0.1。
- 点了 Play 没有人物：看 Hierarchy 里有没有 `HospitalRoot`，以及 Console 有没有脚本报错。`Build On Awake` 需要保持勾选。
