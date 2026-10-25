using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 生成夜班医院灰盒：走廊、病房、钥匙、门锁、逃生门、玩家和凝视怪物。
/// 组件菜单或顶部菜单 Game Demo/Build Hospital Graybox 可在编辑器里预览。
/// </summary>
[AddComponentMenu("Game Demo/Hospital Scene Builder")]
public class HospitalSceneBuilder : MonoBehaviour
{
    const float WallH = 3.2f;
    const float WallT = 0.2f;

    [Tooltip("进入播放模式时自动生成。会清掉上一次生成的 HospitalRoot。")]
    public bool buildOnAwake = true;

    public float walkSpeed = 3.6f;
    public float sprintSpeed = 6.8f;
    public float mouseSensitivity = 3.2f;
    public float monsterPatrolSpeed = 1.7f;
    public float monsterHuntSpeed = 5.3f;
    public float monsterEnragedSpeed = 6.3f;

    [Header("可选材质覆盖，留空则使用代码生成的贴图")]
    public Material wallMaterial;
    public Material floorMaterial;
    public Material ceilingMaterial;
    public Material doorMaterial;

    struct Gap
    {
        public float center;
        public float width;
        public float height;

        public Gap(float center, float width, float height)
        {
            this.center = center;
            this.width = width;
            this.height = height;
        }
    }

    struct Room
    {
        public string name;
        public float x0, x1, z0, z1;
        public bool ceiling;

        public Room(string name, float x0, float x1, float z0, float z1, bool ceiling)
        {
            this.name = name;
            this.x0 = x0;
            this.x1 = x1;
            this.z0 = z0;
            this.z1 = z1;
            this.ceiling = ceiling;
        }
    }

    Transform level;
    HospitalGeo geo;
    HospitalMaterials mats;
    HospitalNavGraph graph;

    [ContextMenu("Build Hospital Graybox")]
    public void BuildHospitalGraybox()
    {
        if (Application.isPlaying)
            Time.timeScale = 1f;

        ClearBuiltLevel();
        mats = ProceduralArt.Create(wallMaterial, floorMaterial, ceilingMaterial, doorMaterial);
        GameObject root = new GameObject("HospitalRoot");
        root.SetActive(false);
        level = root.transform;
        geo = new HospitalGeo(level, mats);

        BuildRooms();
        BuildWalls();
        BuildLights();
        BuildProps();
        BuildActorsAndDoors();
        ApplyAtmosphere();
        DisableSceneDefaults();

        root.SetActive(true);
        Physics.SyncTransforms();
        int problems = ValidateNav();
        Debug.Log("医院灰盒已生成。导航穿墙警告: " + problems + "。WASD 移动，E 交互，F 手电筒。");

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }

    public void ClearBuiltLevel()
    {
        GameObject existing = GameObject.Find("HospitalRoot");
        if (existing != null)
            DestroyImmediate(existing);
    }

    void Awake()
    {
        if (!Application.isPlaying || !buildOnAwake)
            return;
        BuildHospitalGraybox();
    }

    void BuildRooms()
    {
        Room[] rooms = Rooms();
        for (int i = 0; i < rooms.Length; i++)
        {
            Room room = rooms[i];
            float sx = room.x1 - room.x0;
            float sz = room.z1 - room.z0;
            Vector3 center = new Vector3((room.x0 + room.x1) * 0.5f, -0.1f, (room.z0 + room.z1) * 0.5f);
            geo.Box(level, "Floor", center, new Vector3(sx, 0.2f, sz), mats.floor, new Vector2(sx / 1.6f, sz / 1.6f), true, true);
            if (!room.ceiling)
                continue;
            Vector3 ceil = new Vector3(center.x, WallH + 0.06f, center.z);
            geo.Box(level, "Ceil", ceil, new Vector3(sx, 0.12f, sz), mats.ceiling, new Vector2(sx / 2.2f, sz / 2.2f), true, true);
        }
    }

    void BuildWalls()
    {
        WallX("Wall", -6f, 6f, 0f);
        WallX("Wall", -6f, 6f, 14f, new Gap(0f, 1.6f, 2.35f));
        WallZ("Wall", -6f, 0f, 14f, new Gap(6.5f, 1.5f, 2.35f));
        WallZ("Wall", 6f, 0f, 14f, new Gap(6.5f, 1.5f, 2.35f));

        WallZ("Wall", -12.2f, 3f, 10f);
        WallX("Wall", -12.2f, -6f, 3f);
        WallX("Wall", -12.2f, -6f, 10f);

        WallZ("Wall", 12.2f, 3f, 10f);
        WallX("Wall", 6f, 12.2f, 3f);
        WallX("Wall", 6f, 12.2f, 10f);

        WallZ("Wall", -2f, 14f, 22f);
        WallZ("Wall", 2f, 14f, 22f);

        WallX("Wall", -8f, 8f, 22f, new Gap(0f, 4f, WallH));
        WallX("Wall", -8f, 8f, 38f, new Gap(0f, 1.6f, 2.35f));
        WallZ("Wall", -8f, 22f, 38f, new Gap(30f, 1.5f, 2.35f));
        WallZ("Wall", 8f, 22f, 38f, new Gap(30f, 1.5f, 2.35f));

        WallZ("Wall", -14.5f, 26f, 34f);
        WallX("Wall", -14.5f, -8f, 26f);
        WallX("Wall", -14.5f, -8f, 34f);
        WallZ("Wall", 14.5f, 26f, 34f);
        WallX("Wall", 8f, 14.5f, 26f);
        WallX("Wall", 8f, 14.5f, 34f);

        WallZ("Wall", -2f, 38f, 72f,
            new Gap(45.5f, 1.4f, 2.35f),
            new Gap(55.5f, 1.4f, 2.35f),
            new Gap(65.5f, 1.4f, 2.35f));
        WallZ("Wall", 2f, 38f, 72f,
            new Gap(45.5f, 1.4f, 2.35f),
            new Gap(55.5f, 1.4f, 2.35f),
            new Gap(67f, 1.8f, 2.35f));

        CloseRoom(-10f, -2f, 42f, 49f, false);
        CloseRoom(2f, 10f, 42f, 49f, true);
        CloseRoom(-10f, -2f, 52f, 59f, false);
        CloseRoom(2f, 10f, 52f, 59f, true);
        CloseRoom(-10f, -2f, 62f, 69f, false);

        WallZ("Wall", 12f, 62f, 72f);
        WallX("Wall", 2f, 12f, 62f);
        WallX("Wall", 6f, 12f, 72f);

        WallX("Wall", -6f, 6f, 72f, new Gap(0f, 1.8f, 2.5f));
        WallZ("Wall", -6f, 72f, 84f);
        WallZ("Wall", 6f, 72f, 84f);
        WallX("Wall", -6f, 6f, 84f);

        FrameOnZWall(0f, 14f, 1.6f, 2.35f);
        FrameOnZWall(0f, 38f, 1.6f, 2.35f);
        FrameOnXWall(-8f, 30f, 1.5f, 2.35f);
        FrameOnXWall(8f, 30f, 1.5f, 2.35f);
        FrameOnZWall(0f, 72f, 1.8f, 2.5f);
    }

    void CloseRoom(float x0, float x1, float z0, float z1, bool eastRoom)
    {
        WallX("Wall", x0, x1, z0);
        WallX("Wall", x0, x1, z1);
        if (eastRoom)
            WallZ("Wall", x1, z0, z1);
        else
            WallZ("Wall", x0, z0, z1);
    }

    void BuildLights()
    {
        Color warm = new Color(1f, 0.78f, 0.55f);
        Color sick = new Color(0.7f, 0.9f, 0.75f);
        Color dim = new Color(0.85f, 0.7f, 0.5f);
        Color red = new Color(0.9f, 0.35f, 0.28f);
        Color cool = new Color(0.75f, 0.86f, 1f);

        Lamp(new Vector3(0f, 3.02f, 5.2f), warm, 1.35f, 9f, true, true);
        Lamp(new Vector3(-3.2f, 3.02f, 11f), warm, 0.9f, 7f, false, false);
        Lamp(new Vector3(-9.2f, 3.02f, 6.5f), warm, 0.7f, 6f, true, false);
        Lamp(new Vector3(9.2f, 3.02f, 6.5f), warm, 0.45f, 5f, true, false);
        Lamp(new Vector3(0f, 3.02f, 18f), warm, 0.8f, 7f, false, false);
        Lamp(new Vector3(-3.4f, 3.02f, 27f), sick, 1.1f, 8f, true, true);
        Lamp(new Vector3(3f, 3.02f, 34f), sick, 0.7f, 7f, false, false);
        Lamp(new Vector3(-11.2f, 3.02f, 30f), warm, 0.55f, 6f, false, false);
        Lamp(new Vector3(11.4f, 3.02f, 30f), sick, 0.8f, 6.5f, true, false);
        Lamp(new Vector3(0f, 3.02f, 43f), dim, 0.45f, 6f, true, false);
        Lamp(new Vector3(0f, 3.02f, 52f), dim, 0.22f, 5f, true, false);
        Lamp(new Vector3(0f, 3.02f, 61f), dim, 0.08f, 4f, false, false);
        Lamp(new Vector3(0f, 3.02f, 69f), dim, 0.35f, 6f, true, false);
        Lamp(new Vector3(-6.5f, 3.02f, 45.5f), dim, 0.35f, 5f, false, false);
        Lamp(new Vector3(6.4f, 3.02f, 45.5f), dim, 0.25f, 5f, true, false);
        Lamp(new Vector3(-6.5f, 3.02f, 55.5f), dim, 0.2f, 4.5f, false, false);
        Lamp(new Vector3(6.4f, 3.02f, 55.5f), dim, 0.15f, 4f, false, false);
        Lamp(new Vector3(-6.8f, 3.02f, 65.5f), red, 0.28f, 5f, true, false);
        Lamp(new Vector3(7.4f, 2.9f, 67f), red, 0.55f, 7f, true, true);
        Lamp(new Vector3(0f, 4.4f, 78f), cool, 2.4f, 16f, false, true);
        Lamp(new Vector3(-3.2f, 3.6f, 81f), cool, 1.2f, 8f, false, false);
    }

    void BuildProps()
    {
        Desk(new Vector3(-2.55f, 0f, 9.05f));
        Shelf(new Vector3(-11.45f, 0f, 6.5f));
        Counter(new Vector3(-3.6f, 0f, 27.2f));
        Cart(new Vector3(12.3f, 0f, 31.1f));
        geo.Box(level, "PropCabinet", new Vector3(10.4f, 0.45f, 28.6f), new Vector3(0.55f, 0.9f, 0.42f), mats.metal, Vector2.one, true, true);
        Bed(new Vector3(-7.8f, 0f, 45.5f), 90f, false);
        Bed(new Vector3(7.8f, 0f, 45.5f), -90f, false);
        Bed(new Vector3(-7.8f, 0f, 55.5f), 90f, false);
        Bed(new Vector3(7.8f, 0f, 55.5f), -90f, true);
        Bed(new Vector3(-7.8f, 0f, 65.5f), 90f, true);
        OperatingTable(new Vector3(9.4f, 0f, 64.6f));
        Gurney(new Vector3(1.2f, 0f, 50.2f));
        Wheelchair(new Vector3(-1.25f, 0f, 60.2f));
        IVStand(new Vector3(-6.2f, 0f, 66.6f));
        IVStand(new Vector3(8.6f, 0f, 46.8f));
        RedCross(new Vector3(0f, 2.15f, 0.16f));
        Window(new Vector3(-3.6f, 1.7f, 0.14f));
        Window(new Vector3(3.6f, 1.7f, 0.14f));
        Toilet(new Vector3(10.6f, 0f, 4.2f));
        Ambulance(new Vector3(-3.3f, 0f, 80.2f));
        geo.Cylinder(level, "PropPipe", new Vector3(-1.55f, 2.95f, 55f), 0.05f, 30f, Quaternion.Euler(90f, 0f, 0f), mats.metal, false);
        geo.Quad(level, "PropBlood", new Vector3(-7.5f, 0.025f, 64.6f), new Vector2(1.2f, 0.75f), Quaternion.Euler(90f, 25f, 0f), mats.blood, false);
        geo.Quad(level, "PropBlood", new Vector3(8.2f, 0.025f, 66.4f), new Vector2(1.4f, 0.9f), Quaternion.Euler(90f, -15f, 0f), mats.blood, false);

        Sign("库房", new Vector3(-5.72f, 2.45f, 6.5f), Vector3.left, 0.9f, 48, new Color(0.95f, 0.9f, 0.75f));
        Sign("前台", new Vector3(-2.55f, 1.2f, 8.58f), Vector3.forward, 0.7f, 42, new Color(0.9f, 0.95f, 0.9f));
        Sign("护士站", new Vector3(0f, 2.55f, 21.78f), Vector3.forward, 1.4f, 48, new Color(0.8f, 0.95f, 0.85f));
        Sign("诊室", new Vector3(7.78f, 2.45f, 30f), Vector3.right, 0.8f, 44, new Color(0.85f, 0.92f, 0.95f));
        Sign("病房", new Vector3(0f, 2.6f, 37.75f), Vector3.forward, 1.1f, 48, new Color(0.95f, 0.8f, 0.75f));
        Sign("301", new Vector3(-1.78f, 2.35f, 45.5f), Vector3.left, 0.55f, 40, Color.white);
        Sign("302", new Vector3(1.78f, 2.35f, 45.5f), Vector3.right, 0.55f, 40, Color.white);
        Sign("305", new Vector3(-1.78f, 2.35f, 55.5f), Vector3.left, 0.55f, 40, Color.white);
        Sign("306", new Vector3(1.78f, 2.35f, 55.5f), Vector3.right, 0.55f, 40, Color.white);
        Sign("307", new Vector3(-1.78f, 2.35f, 65.5f), Vector3.left, 0.55f, 40, new Color(1f, 0.45f, 0.4f));
        Sign("太平间", new Vector3(1.78f, 2.35f, 67f), Vector3.right, 0.9f, 36, new Color(1f, 0.4f, 0.35f));
        Sign("出口", new Vector3(0f, 2.75f, 71.7f), Vector3.forward, 1.1f, 56, new Color(0.5f, 1f, 0.65f));

        Note("交接记录", "夜班交接：库房、诊室、307 病房各有一把钥匙。紧急出口在病房走廊尽头。那东西只有在你看得见它的时候才会停住。手电筒别一直开着。", new Vector3(-3.15f, 1.28f, 9.05f));
        Note("护士站备忘", "诊室钥匙在检查床旁边。听到刮擦声就回头，看住它，再移动。不要长时间背对它逃跑。", new Vector3(-1.85f, 1.05f, 27.2f));
        Note("307 床卡", "出口钥匙塞在床垫侧面。它昨晚从这张床上站起来。看着它。", new Vector3(-8.9f, 0.95f, 66.7f));
        Note("卫生间告示", "库房在对面。这里没有钥匙。", new Vector3(9.4f, 1.15f, 8.2f));
    }

    void BuildActorsAndDoors()
    {
        GameObject graphObject = new GameObject("NavGraph");
        graphObject.transform.SetParent(level, false);
        graph = graphObject.AddComponent<HospitalNavGraph>();

        int lobby = graph.AddNode("lobby", 0f, 6.2f);
        int storage = graph.AddNode("storage", -9.1f, 6.5f);
        int rest = graph.AddNode("rest", 9.1f, 6.5f);
        int preNurse = graph.AddNode("preNurse", 0f, 12.2f);
        int postNurse = graph.AddNode("postNurse", 0f, 16.2f);
        int nurse = graph.AddNode("nurse", 0f, 30f);
        int exam = graph.AddNode("exam", 11.2f, 30f);
        int office = graph.AddNode("office", -11.2f, 30f);
        int preWard = graph.AddNode("preWard", 0f, 35.6f);
        int postWard = graph.AddNode("postWard", 0f, 40.6f);
        int j301 = graph.AddNode("j301", 0f, 45.5f);
        int r301 = graph.AddNode("r301", -6.2f, 45.5f);
        int r302 = graph.AddNode("r302", 6.2f, 45.5f);
        int j305 = graph.AddNode("j305", 0f, 55.5f);
        int r305 = graph.AddNode("r305", -6.2f, 55.5f);
        int r306 = graph.AddNode("r306", 6.2f, 55.5f);
        int j307 = graph.AddNode("j307", 0f, 65.5f);
        int r307 = graph.AddNode("r307", -6.2f, 65.5f);
        int jMorgue = graph.AddNode("jMorgue", 0f, 67f);
        int morgue = graph.AddNode("morgue", 7.2f, 67.4f);
        int preGate = graph.AddNode("preGate", 0f, 70.2f);
        int bay = graph.AddNode("bay", 0f, 78f);

        graph.AddLink(lobby, storage, true);
        graph.AddLink(lobby, rest, true);
        graph.AddLink(lobby, preNurse, true);
        graph.AddLink(preNurse, postNurse, false);
        graph.AddLink(postNurse, nurse, true);
        graph.AddLink(nurse, exam, false);
        graph.AddLink(nurse, office, false);
        graph.AddLink(nurse, preWard, true);
        graph.AddLink(preWard, postWard, false);
        graph.AddLink(postWard, j301, true);
        graph.AddLink(j301, r301, true);
        graph.AddLink(j301, r302, true);
        graph.AddLink(j301, j305, true);
        graph.AddLink(j305, r305, true);
        graph.AddLink(j305, r306, true);
        graph.AddLink(j305, j307, true);
        graph.AddLink(j307, r307, true);
        graph.AddLink(j307, jMorgue, true);
        graph.AddLink(jMorgue, morgue, true);
        graph.AddLink(jMorgue, preGate, true);
        graph.AddLink(preGate, bay, false);

        DoorOnZWall("Door_Nurse", "nurse", 0f, 14f, 1.6f, 2.3f, 100f, "storage", "需要库房钥匙", preNurse, postNurse);
        DoorOnXWall("Door_Office", "office", -8f, 30f, 1.5f, 2.3f, 100f, "", "办公室的门卡住了", nurse, office);
        DoorOnXWall("Door_Exam", "exam", 8f, 30f, 1.5f, 2.3f, -100f, "", "诊室的门卡住了", nurse, exam);
        DoorOnZWall("Door_Ward", "ward", 0f, 38f, 1.6f, 2.3f, 100f, "exam", "需要诊室钥匙", preWard, postWard);
        BuildGate(preGate, bay);

        Key("storage", "库房钥匙", new Vector3(-11.15f, 1.35f, 6.5f), mats.brass);
        Key("exam", "诊室钥匙", new Vector3(12.3f, 1.08f, 31.1f), mats.steel);
        Key("exit", "出口钥匙", new Vector3(-7.8f, 0.92f, 64.65f), mats.keyRed);
        Battery(new Vector3(-1.7f, 1.28f, 9.2f), 0.45f);
        Battery(new Vector3(10.4f, 1.18f, 28.6f), 0.45f);
        Battery(new Vector3(-7.8f, 0.95f, 54.55f), 0.5f);

        BasicFirstPersonController player = BuildPlayer();
        MonsterWatcherAI monster = BuildMonster(player, morgue, new int[]
        {
            postWard, j301, j305, j307, jMorgue, morgue, j307, j305
        });

        GameAudio.CreateConfigured(level);
        GameObject managerObject = new GameObject("GameManager");
        managerObject.transform.SetParent(level, false);
        GameManager manager = managerObject.AddComponent<GameManager>();
        manager.inventory = player.GetComponent<PlayerInventory>();
        manager.player = player;
        manager.monster = monster;

        GameObject hudObject = new GameObject("GameHUD");
        hudObject.transform.SetParent(level, false);
        GameHUD hud = hudObject.AddComponent<GameHUD>();
        hud.monster = monster;
        hud.player = player;
        hud.flashlight = player.GetComponent<FlashlightController>();
        hud.inventory = player.GetComponent<PlayerInventory>();
        hud.interactor = player.GetComponent<PlayerInteractor>();
    }

    BasicFirstPersonController BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.SetParent(level, false);
        player.transform.position = new Vector3(0f, 0.08f, 3.5f);
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.32f;
        controller.center = new Vector3(0f, 0.92f, 0f);
        controller.stepOffset = 0.25f;
        controller.slopeLimit = 50f;
        controller.skinWidth = 0.04f;
        controller.minMoveDistance = 0f;

        player.AddComponent<PlayerInventory>();
        BasicFirstPersonController motor = player.AddComponent<BasicFirstPersonController>();
        motor.walkSpeed = walkSpeed;
        motor.sprintSpeed = sprintSpeed;
        motor.mouseSensitivity = mouseSensitivity;

        GameObject cameraObject = new GameObject("PlayerCamera");
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0f, 1.64f, 0f);
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 68f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 70f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.012f, 0.014f, 0.018f);
        cameraObject.AddComponent<AudioListener>();
        motor.cameraHolder = cameraObject.transform;

        GameObject flashObject = new GameObject("Flashlight");
        flashObject.transform.SetParent(cameraObject.transform, false);
        flashObject.transform.localPosition = new Vector3(0.22f, -0.16f, 0.28f);
        Light spot = flashObject.AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.spotAngle = 52f;
        spot.range = 18f;
        spot.intensity = 2.6f;
        spot.color = new Color(1f, 0.94f, 0.82f);
        spot.shadows = LightShadows.Soft;
        spot.renderMode = LightRenderMode.ForcePixel;
        geo.Cylinder(flashObject.transform, "Prop", new Vector3(0f, 0f, 0.05f), 0.035f, 0.22f, Quaternion.Euler(90f, 0f, 0f), mats.metal, false);
        AddDust(flashObject.transform);

        FlashlightController flashlight = player.AddComponent<FlashlightController>();
        flashlight.spot = spot;
        flashlight.battery = 1f;
        flashlight.isOn = true;

        PlayerInteractor interactor = player.AddComponent<PlayerInteractor>();
        interactor.viewCamera = camera;
        interactor.inventory = player.GetComponent<PlayerInventory>();
        return motor;
    }

    MonsterWatcherAI BuildMonster(BasicFirstPersonController player, int spawnNode, int[] patrol)
    {
        Vector3 spawn = graph.NodePosition(spawnNode);
        GameObject monster = new GameObject("Monster");
        monster.transform.SetParent(level, false);
        monster.transform.position = new Vector3(spawn.x, 0.05f, spawn.z);
        CharacterController controller = monster.AddComponent<CharacterController>();
        controller.height = 1.9f;
        controller.radius = 0.32f;
        controller.center = new Vector3(0f, 0.95f, 0f);
        controller.stepOffset = 0.2f;
        controller.skinWidth = 0.03f;

        Transform visual = new GameObject("Visual").transform;
        visual.SetParent(monster.transform, false);
        geo.Capsule(visual, "PropLeg", new Vector3(-0.12f, 0.48f, 0f), 0.08f, 0.96f, Quaternion.identity, mats.skin, false);
        geo.Capsule(visual, "PropLeg", new Vector3(0.12f, 0.48f, 0f), 0.08f, 0.96f, Quaternion.identity, mats.skin, false);
        geo.Box(visual, "PropGown", new Vector3(0f, 0.82f, 0.02f), new Vector3(0.58f, 0.85f, 0.32f), mats.gown, Vector2.one, false, false);
        geo.Box(visual, "PropTorso", new Vector3(0f, 1.28f, 0f), new Vector3(0.46f, 0.58f, 0.26f), mats.skin, Vector2.one, false, false);

        Transform armL = new GameObject("ArmL").transform;
        armL.SetParent(visual, false);
        armL.localPosition = new Vector3(-0.34f, 1.42f, 0f);
        geo.Capsule(armL, "PropArm", new Vector3(0f, -0.34f, 0f), 0.055f, 0.7f, Quaternion.identity, mats.skin, false);
        Transform armR = new GameObject("ArmR").transform;
        armR.SetParent(visual, false);
        armR.localPosition = new Vector3(0.34f, 1.42f, 0f);
        geo.Capsule(armR, "PropArm", new Vector3(0f, -0.34f, 0f), 0.055f, 0.7f, Quaternion.identity, mats.skin, false);

        Transform head = new GameObject("Head").transform;
        head.SetParent(visual, false);
        head.localPosition = new Vector3(0f, 1.72f, 0.04f);
        geo.Sphere(head, "PropHead", Vector3.zero, 0.16f, mats.skin, false);
        Material eyes = new Material(mats.eye);
        GameObject eyeL = geo.Sphere(head, "PropEye", new Vector3(-0.06f, 0.025f, 0.145f), 0.028f, eyes, false);
        GameObject eyeR = geo.Sphere(head, "PropEye", new Vector3(0.06f, 0.025f, 0.145f), 0.028f, eyes, false);
        eyeL.GetComponent<Renderer>().sharedMaterial = eyes;
        eyeR.GetComponent<Renderer>().sharedMaterial = eyes;

        GameObject eyeLight = new GameObject("EyeLight");
        eyeLight.transform.SetParent(head, false);
        eyeLight.transform.localPosition = new Vector3(0f, 0.02f, 0.16f);
        Light glow = eyeLight.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.range = 2.4f;
        glow.intensity = 0.45f;
        glow.color = new Color(1f, 0.2f, 0.12f);

        MonsterWatcherAI ai = monster.AddComponent<MonsterWatcherAI>();
        ai.player = player.transform;
        ai.playerCamera = player.cameraHolder.GetComponent<Camera>();
        ai.graph = graph;
        ai.armL = armL;
        ai.armR = armR;
        ai.head = head;
        ai.eyeMaterial = eyes;
        ai.patrolNodes = patrol;
        ai.patrolSpeed = monsterPatrolSpeed;
        ai.huntSpeed = monsterHuntSpeed;
        ai.enragedSpeed = monsterEnragedSpeed;
        AudioSource voice = monster.AddComponent<AudioSource>();
        ai.BindVoice(voice);
        return ai;
    }

    void DoorOnZWall(string name, string doorId, float x, float z, float width, float height, float openAngle, string keyId, string hint, int navA, int navB)
    {
        GameObject door = new GameObject(name);
        door.transform.SetParent(level, false);
        Transform pivot = new GameObject("Pivot").transform;
        pivot.SetParent(door.transform, false);
        float hingeX = x - width * 0.5f;
        pivot.position = new Vector3(hingeX, 0f, z);
        float panelW = width - 0.08f;
        float thick = 0.07f;
        geo.Box(pivot, "PropDoor", new Vector3(panelW * 0.5f, 0.55f, 0f), new Vector3(panelW, 1.1f, thick), mats.door, new Vector2(1f, 1f), false, false);
        geo.Box(pivot, "PropDoor", new Vector3(0.07f, 1.5f, 0f), new Vector3(0.14f, 0.8f, thick), mats.door, Vector2.one, false, false);
        geo.Box(pivot, "PropDoor", new Vector3(panelW - 0.07f, 1.5f, 0f), new Vector3(0.14f, 0.8f, thick), mats.door, Vector2.one, false, false);
        geo.Box(pivot, "PropDoor", new Vector3(panelW * 0.5f, (1.9f + height) * 0.5f, 0f), new Vector3(panelW, height - 1.9f, thick), mats.door, Vector2.one, false, false);
        geo.Quad(pivot, "PropGlass", new Vector3(panelW * 0.5f, 1.5f, 0f), new Vector2(panelW - 0.34f, 0.72f), Quaternion.identity, mats.glass, false);
        Material plate = string.IsNullOrEmpty(keyId) ? mats.exitGlow : mats.keyRed;
        geo.Box(pivot, "PropLock", new Vector3(panelW * 0.82f, 1.05f, 0.05f), new Vector3(0.08f, 0.12f, 0.02f), plate, Vector2.one, false, false);

        List<Collider> blockers = new List<Collider>();
        blockers.Add(MakeBlocker(door.transform, new Vector3(x, 0.55f, z), new Vector3(width - 0.04f, 1.1f, 0.36f)));
        blockers.Add(MakeBlocker(door.transform, new Vector3(x, (1.9f + height) * 0.5f, z), new Vector3(width - 0.04f, height - 1.9f, 0.36f)));
        blockers.Add(MakeBlocker(door.transform, new Vector3(x - width * 0.5f + 0.12f, 1.5f, z), new Vector3(0.22f, 0.8f, 0.36f)));
        blockers.Add(MakeBlocker(door.transform, new Vector3(x + width * 0.5f - 0.12f, 1.5f, z), new Vector3(0.22f, 0.8f, 0.36f)));

        DoorController controller = door.AddComponent<DoorController>();
        controller.doorId = doorId;
        controller.requiredKeyId = keyId;
        controller.lockedHint = hint;
        controller.pivot = pivot;
        controller.openAngle = openAngle;
        controller.blockers = blockers.ToArray();
        controller.nav = graph;
        controller.navA = navA;
        controller.navB = navB;
    }

    void DoorOnXWall(string name, string doorId, float x, float z, float width, float height, float openAngle, string keyId, string hint, int navA, int navB)
    {
        GameObject door = new GameObject(name);
        door.transform.SetParent(level, false);
        Transform pivot = new GameObject("Pivot").transform;
        pivot.SetParent(door.transform, false);
        float hingeZ = z - width * 0.5f;
        pivot.position = new Vector3(x, 0f, hingeZ);
        float panelW = width - 0.08f;
        float thick = 0.07f;
        geo.Box(pivot, "PropDoor", new Vector3(0f, 0.55f, panelW * 0.5f), new Vector3(thick, 1.1f, panelW), mats.door, Vector2.one, false, false);
        geo.Box(pivot, "PropDoor", new Vector3(0f, 1.5f, 0.07f), new Vector3(thick, 0.8f, 0.14f), mats.door, Vector2.one, false, false);
        geo.Box(pivot, "PropDoor", new Vector3(0f, 1.5f, panelW - 0.07f), new Vector3(thick, 0.8f, 0.14f), mats.door, Vector2.one, false, false);
        geo.Box(pivot, "PropDoor", new Vector3(0f, (1.9f + height) * 0.5f, panelW * 0.5f), new Vector3(thick, height - 1.9f, panelW), mats.door, Vector2.one, false, false);
        geo.Quad(pivot, "PropGlass", new Vector3(0f, 1.5f, panelW * 0.5f), new Vector2(0.72f, panelW - 0.34f), Quaternion.Euler(0f, 90f, 0f), mats.glass, false);

        List<Collider> blockers = new List<Collider>();
        blockers.Add(MakeBlocker(door.transform, new Vector3(x, 0.55f, z), new Vector3(0.36f, 1.1f, width - 0.04f)));
        blockers.Add(MakeBlocker(door.transform, new Vector3(x, (1.9f + height) * 0.5f, z), new Vector3(0.36f, height - 1.9f, width - 0.04f)));
        blockers.Add(MakeBlocker(door.transform, new Vector3(x, 1.5f, z - width * 0.5f + 0.12f), new Vector3(0.36f, 0.8f, 0.22f)));
        blockers.Add(MakeBlocker(door.transform, new Vector3(x, 1.5f, z + width * 0.5f - 0.12f), new Vector3(0.36f, 0.8f, 0.22f)));

        DoorController controller = door.AddComponent<DoorController>();
        controller.doorId = doorId;
        controller.requiredKeyId = keyId;
        controller.lockedHint = hint;
        controller.pivot = pivot;
        controller.openAngle = openAngle;
        controller.blockers = blockers.ToArray();
        controller.nav = graph;
        controller.navA = navA;
        controller.navB = navB;
        controller.consumeKey = !string.IsNullOrEmpty(keyId);
    }

    void BuildGate(int navA, int navB)
    {
        GameObject gate = new GameObject("EscapeDoor");
        gate.transform.SetParent(level, false);
        Transform panel = new GameObject("Panel").transform;
        panel.SetParent(gate.transform, false);
        panel.position = new Vector3(0f, 0f, 72f);
        for (int i = 0; i < 6; i++)
        {
            float x = -0.72f + i * 0.29f;
            geo.Box(panel, "PropBar", new Vector3(x, 1.2f, 0f), new Vector3(0.06f, 2.4f, 0.06f), mats.metal, Vector2.one, false, false);
        }
        geo.Box(panel, "PropBar", new Vector3(0f, 2.45f, 0f), new Vector3(1.7f, 0.1f, 0.08f), mats.metal, Vector2.one, false, false);
        geo.Box(panel, "PropSign", new Vector3(0f, 2.15f, 0.05f), new Vector3(0.7f, 0.28f, 0.02f), mats.exitGlow, Vector2.one, false, false);
        Collider blocker = MakeBlocker(gate.transform, new Vector3(0f, 1.2f, 72f), new Vector3(1.7f, 2.5f, 0.4f));

        EscapeDoor escape = gate.AddComponent<EscapeDoor>();
        escape.requiredKeyId = "exit";
        escape.lockedHint = "紧急出口锁着，需要出口钥匙";
        escape.panel = panel;
        escape.blockers = new Collider[] { blocker };
        escape.nav = graph;
        escape.navA = navA;
        escape.navB = navB;
    }

    Collider MakeBlocker(Transform parent, Vector3 center, Vector3 size)
    {
        GameObject go = geo.Box(parent, "DoorBlocker", center, size, mats.door, Vector2.one, true, false);
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;
        return go.GetComponent<Collider>();
    }

    void Key(string id, string display, Vector3 position, Material mat)
    {
        GameObject host = new GameObject("Key_" + id);
        host.transform.SetParent(level, false);
        host.transform.position = position;
        BoxCollider box = host.AddComponent<BoxCollider>();
        box.size = new Vector3(0.4f, 0.4f, 0.4f);
        KeyPickup pickup = host.AddComponent<KeyPickup>();
        pickup.keyId = id;
        pickup.displayName = display;
        geo.Cylinder(host.transform, "PropShaft", new Vector3(0f, 0f, 0.05f), 0.018f, 0.16f, Quaternion.Euler(90f, 0f, 0f), mat, false);
        geo.Box(host.transform, "PropBow", new Vector3(0f, 0f, -0.06f), new Vector3(0.07f, 0.09f, 0.035f), mat, Vector2.one, false, false);
        GameObject glowObject = new GameObject("Glow");
        glowObject.transform.SetParent(host.transform, false);
        Light glow = glowObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.range = 1.8f;
        glow.intensity = 0.8f;
        glow.color = mat.color;
    }

    void Battery(Vector3 position, float amount)
    {
        GameObject host = new GameObject("Battery");
        host.transform.SetParent(level, false);
        host.transform.position = position;
        BoxCollider box = host.AddComponent<BoxCollider>();
        box.size = new Vector3(0.35f, 0.35f, 0.35f);
        BatteryPickup pickup = host.AddComponent<BatteryPickup>();
        pickup.amount = amount;
        geo.Cylinder(host.transform, "PropCell", Vector3.zero, 0.05f, 0.16f, Quaternion.identity, mats.exitGlow, false);
    }

    void Note(string title, string body, Vector3 position)
    {
        GameObject host = new GameObject("Note");
        host.transform.SetParent(level, false);
        host.transform.position = position;
        BoxCollider box = host.AddComponent<BoxCollider>();
        box.size = new Vector3(0.5f, 0.28f, 0.4f);
        NoteInteractable note = host.AddComponent<NoteInteractable>();
        note.title = title;
        note.body = body;
        geo.Box(host.transform, "PropPaper", new Vector3(0f, -0.08f, 0f), new Vector3(0.32f, 0.02f, 0.22f), mats.paper, Vector2.one, false, false);
    }

    void Lamp(Vector3 position, Color color, float intensity, float range, bool flicker, bool forcePixel)
    {
        GameObject go = new GameObject("Lamp");
        go.transform.SetParent(level, false);
        go.transform.position = position;
        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
        if (forcePixel)
            light.renderMode = LightRenderMode.ForcePixel;
        if (flicker)
        {
            FlickerLight flickerLight = go.AddComponent<FlickerLight>();
            flickerLight.baseIntensity = intensity;
        }
        geo.Box(go.transform, "PropFixture", new Vector3(0f, 0.1f, 0f), new Vector3(0.72f, 0.06f, 0.28f), mats.metal, Vector2.one, false, true);
    }

    void Sign(string content, Vector3 position, Vector3 lookDirection, float width, int fontSize, Color color)
    {
        GameObject go = new GameObject("Sign");
        go.transform.SetParent(level, false);
        go.transform.position = position;
        if (lookDirection.sqrMagnitude < 0.001f)
            lookDirection = Vector3.forward;
        go.transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400f, 90f);
        rect.localScale = Vector3.one * (width / 400f);
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(go.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.font = GameFonts.Get();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform textRect = text.rectTransform;
        textRect.sizeDelta = rect.sizeDelta;
        textRect.localScale = Vector3.one;
    }

    void Desk(Vector3 position)
    {
        geo.Box(level, "PropDesk", position + new Vector3(0f, 0.52f, 0f), new Vector3(2.2f, 1.04f, 0.78f), mats.wood, new Vector2(1.5f, 1f), true, true);
        geo.Box(level, "PropMonitor", position + new Vector3(0.45f, 1.18f, 0f), new Vector3(0.42f, 0.28f, 0.06f), mats.metal, Vector2.one, false, true);
        geo.Box(level, "PropScreen", position + new Vector3(0.45f, 1.18f, -0.04f), new Vector3(0.32f, 0.18f, 0.02f), mats.screen, Vector2.one, false, true);
    }

    void Shelf(Vector3 position)
    {
        geo.Box(level, "PropShelf", position + new Vector3(0f, 0.9f, 0f), new Vector3(0.4f, 1.8f, 2.1f), mats.metal, Vector2.one, true, true);
        geo.Box(level, "PropBox", position + new Vector3(0.05f, 0.55f, -0.5f), new Vector3(0.28f, 0.22f, 0.28f), mats.paper, Vector2.one, false, true);
        geo.Box(level, "PropBox", position + new Vector3(0.05f, 1.15f, 0.4f), new Vector3(0.26f, 0.2f, 0.32f), mats.fabric, Vector2.one, false, true);
    }

    void Counter(Vector3 position)
    {
        geo.Box(level, "PropCounter", position + new Vector3(0f, 0.55f, 0f), new Vector3(2.3f, 1.1f, 3.4f), mats.wood, new Vector2(1.2f, 2f), true, true);
    }

    void Cart(Vector3 position)
    {
        geo.Box(level, "PropCart", position + new Vector3(0f, 0.75f, 0f), new Vector3(0.7f, 0.08f, 0.5f), mats.metal, Vector2.one, true, true);
        geo.Cylinder(level, "PropLeg", position + new Vector3(-0.25f, 0.36f, -0.16f), 0.03f, 0.72f, Quaternion.identity, mats.metal, true);
        geo.Cylinder(level, "PropLeg", position + new Vector3(0.25f, 0.36f, -0.16f), 0.03f, 0.72f, Quaternion.identity, mats.metal, true);
        geo.Cylinder(level, "PropLeg", position + new Vector3(-0.25f, 0.36f, 0.16f), 0.03f, 0.72f, Quaternion.identity, mats.metal, true);
        geo.Cylinder(level, "PropLeg", position + new Vector3(0.25f, 0.36f, 0.16f), 0.03f, 0.72f, Quaternion.identity, mats.metal, true);
    }

    void Bed(Vector3 position, float yaw, bool messy)
    {
        Transform bed = new GameObject("PropBed").transform;
        bed.SetParent(level, false);
        bed.position = position;
        bed.rotation = Quaternion.Euler(0f, yaw, 0f);
        geo.Box(bed, "PropFrame", new Vector3(0f, 0.32f, 0f), new Vector3(1.0f, 0.32f, 2.05f), mats.metal, Vector2.one, true, false);
        geo.Box(bed, "PropMattress", new Vector3(0f, 0.55f, 0f), new Vector3(0.92f, 0.16f, 1.9f), mats.sheet, Vector2.one, true, false);
        geo.Box(bed, "PropPillow", new Vector3(0f, 0.68f, -0.72f), new Vector3(0.5f, 0.08f, 0.32f), mats.paper, Vector2.one, false, false);
        if (messy)
            geo.Box(bed, "PropBlanket", new Vector3(0.08f, 0.66f, 0.25f), new Vector3(0.88f, 0.05f, 1.05f), mats.fabric, Vector2.one, false, false);
    }

    void OperatingTable(Vector3 position)
    {
        geo.Box(level, "PropTable", position + new Vector3(0f, 0.8f, 0f), new Vector3(0.7f, 0.08f, 1.8f), mats.metal, Vector2.one, true, true);
        geo.Box(level, "PropBase", position + new Vector3(0f, 0.38f, 0f), new Vector3(0.35f, 0.7f, 0.35f), mats.metal, Vector2.one, true, true);
    }

    void Gurney(Vector3 position)
    {
        geo.Box(level, "PropGurney", position + new Vector3(0f, 0.55f, 0f), new Vector3(0.62f, 0.08f, 1.55f), mats.sheet, Vector2.one, true, true);
        geo.Box(level, "PropFrame", position + new Vector3(0f, 0.32f, 0f), new Vector3(0.55f, 0.28f, 1.6f), mats.metal, Vector2.one, true, true);
    }

    void Wheelchair(Vector3 position)
    {
        geo.Box(level, "PropSeat", position + new Vector3(0f, 0.48f, 0f), new Vector3(0.5f, 0.06f, 0.5f), mats.fabric, Vector2.one, true, true);
        geo.Box(level, "PropBack", position + new Vector3(0f, 0.78f, -0.22f), new Vector3(0.5f, 0.5f, 0.06f), mats.fabric, Vector2.one, true, true);
        geo.Cylinder(level, "PropWheel", position + new Vector3(-0.28f, 0.28f, 0f), 0.26f, 0.05f, Quaternion.Euler(0f, 0f, 90f), mats.metal, true);
        geo.Cylinder(level, "PropWheel", position + new Vector3(0.28f, 0.28f, 0f), 0.26f, 0.05f, Quaternion.Euler(0f, 0f, 90f), mats.metal, true);
    }

    void IVStand(Vector3 position)
    {
        geo.Cylinder(level, "PropPole", position + new Vector3(0f, 1f, 0f), 0.02f, 2f, Quaternion.identity, mats.metal, false);
        geo.Box(level, "PropBag", position + new Vector3(0.08f, 1.7f, 0f), new Vector3(0.12f, 0.22f, 0.08f), mats.fabric, Vector2.one, false, false);
    }

    void RedCross(Vector3 position)
    {
        geo.Box(level, "PropCross", position, new Vector3(0.16f, 0.62f, 0.04f), mats.cross, Vector2.one, false, true);
        geo.Box(level, "PropCross", position, new Vector3(0.62f, 0.16f, 0.04f), mats.cross, Vector2.one, false, true);
    }

    void Window(Vector3 position)
    {
        geo.Box(level, "PropFrame", position, new Vector3(1.15f, 0.08f, 0.05f), mats.metal, Vector2.one, false, true);
        geo.Box(level, "PropFrame", position + new Vector3(0f, 0.7f, 0f), new Vector3(1.15f, 0.08f, 0.05f), mats.metal, Vector2.one, false, true);
        geo.Box(level, "PropFrame", position + new Vector3(-0.55f, 0.35f, 0f), new Vector3(0.08f, 0.7f, 0.05f), mats.metal, Vector2.one, false, true);
        geo.Box(level, "PropFrame", position + new Vector3(0.55f, 0.35f, 0f), new Vector3(0.08f, 0.7f, 0.05f), mats.metal, Vector2.one, false, true);
        geo.Quad(level, "PropGlass", position + new Vector3(0f, 0.35f, 0f), new Vector2(1f, 0.62f), Quaternion.identity, mats.glass, false);
    }

    void Toilet(Vector3 position)
    {
        geo.Box(level, "PropToilet", position + new Vector3(0f, 0.35f, 0f), new Vector3(0.45f, 0.4f, 0.6f), mats.paint, Vector2.one, true, true);
        geo.Box(level, "PropTank", position + new Vector3(0f, 0.75f, -0.22f), new Vector3(0.4f, 0.4f, 0.18f), mats.paint, Vector2.one, true, true);
    }

    void Ambulance(Vector3 position)
    {
        Transform van = new GameObject("PropAmbulance").transform;
        van.SetParent(level, false);
        van.position = position;
        geo.Box(van, "PropBody", new Vector3(0f, 0.95f, 0f), new Vector3(1.7f, 1.45f, 3.8f), mats.paint, Vector2.one, true, true);
        geo.Box(van, "PropStripe", new Vector3(0f, 1.15f, 0f), new Vector3(1.74f, 0.16f, 3.82f), mats.cross, Vector2.one, false, true);
        geo.Box(van, "PropCab", new Vector3(0f, 0.95f, 1.55f), new Vector3(1.6f, 1.2f, 0.9f), mats.glass, Vector2.one, false, true);
        geo.Cylinder(van, "PropWheel", new Vector3(-0.75f, 0.32f, 1.2f), 0.32f, 0.18f, Quaternion.Euler(0f, 0f, 90f), mats.metal, true);
        geo.Cylinder(van, "PropWheel", new Vector3(0.75f, 0.32f, 1.2f), 0.32f, 0.18f, Quaternion.Euler(0f, 0f, 90f), mats.metal, true);
        geo.Cylinder(van, "PropWheel", new Vector3(-0.75f, 0.32f, -1.2f), 0.32f, 0.18f, Quaternion.Euler(0f, 0f, 90f), mats.metal, true);
        geo.Cylinder(van, "PropWheel", new Vector3(0.75f, 0.32f, -1.2f), 0.32f, 0.18f, Quaternion.Euler(0f, 0f, 90f), mats.metal, true);
    }

    void AddDust(Transform parent)
    {
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
            shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        if (shader == null)
            return;
        try
        {
            ConfigureDust(parent, shader);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("手电筒尘埃粒子未启用: " + exception.Message);
        }
    }

    static void ConfigureDust(Transform parent, Shader shader)
    {
        ParticleSystem particles = parent.gameObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = 0.55f;
        main.startSpeed = 0.2f;
        main.startSize = 0.012f;
        main.startColor = new Color(1f, 0.95f, 0.85f, 0.22f);
        main.maxParticles = 48;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 16f;
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 16f;
        shape.radius = 0.02f;
        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(shader);
    }

    void WallX(string name, float x0, float x1, float z, params Gap[] gaps)
    {
        if (x1 < x0)
        {
            float swap = x0;
            x0 = x1;
            x1 = swap;
        }
        System.Array.Sort(gaps, delegate (Gap a, Gap b) { return a.center.CompareTo(b.center); });
        float cursor = x0;
        for (int i = 0; i < gaps.Length; i++)
        {
            float g0 = Mathf.Max(x0, gaps[i].center - gaps[i].width * 0.5f);
            float g1 = Mathf.Min(x1, gaps[i].center + gaps[i].width * 0.5f);
            if (g0 > cursor + 0.02f)
                SolidX(name, cursor, g0, z, 0f, WallH);
            if (g1 > g0 + 0.02f && gaps[i].height < WallH - 0.05f)
                SolidX(name, g0, g1, z, gaps[i].height, WallH);
            cursor = Mathf.Max(cursor, g1);
        }
        if (x1 > cursor + 0.02f)
            SolidX(name, cursor, x1, z, 0f, WallH);
    }

    void WallZ(string name, float x, float z0, float z1, params Gap[] gaps)
    {
        if (z1 < z0)
        {
            float swap = z0;
            z0 = z1;
            z1 = swap;
        }
        System.Array.Sort(gaps, delegate (Gap a, Gap b) { return a.center.CompareTo(b.center); });
        float cursor = z0;
        for (int i = 0; i < gaps.Length; i++)
        {
            float g0 = Mathf.Max(z0, gaps[i].center - gaps[i].width * 0.5f);
            float g1 = Mathf.Min(z1, gaps[i].center + gaps[i].width * 0.5f);
            if (g0 > cursor + 0.02f)
                SolidZ(name, x, cursor, g0, 0f, WallH);
            if (g1 > g0 + 0.02f && gaps[i].height < WallH - 0.05f)
                SolidZ(name, x, g0, g1, gaps[i].height, WallH);
            cursor = Mathf.Max(cursor, g1);
        }
        if (z1 > cursor + 0.02f)
            SolidZ(name, x, cursor, z1, 0f, WallH);
    }

    void SolidX(string name, float x0, float x1, float z, float y0, float y1)
    {
        float length = x1 - x0;
        float height = y1 - y0;
        if (length < 0.02f || height < 0.02f)
            return;
        geo.Box(level, name, new Vector3((x0 + x1) * 0.5f, (y0 + y1) * 0.5f, z), new Vector3(length, height, WallT), mats.wall, new Vector2(length / 2.2f, height / 2.2f), true, true);
    }

    void SolidZ(string name, float x, float z0, float z1, float y0, float y1)
    {
        float length = z1 - z0;
        float height = y1 - y0;
        if (length < 0.02f || height < 0.02f)
            return;
        geo.Box(level, name, new Vector3(x, (y0 + y1) * 0.5f, (z0 + z1) * 0.5f), new Vector3(WallT, height, length), mats.wall, new Vector2(length / 2.2f, height / 2.2f), true, true);
    }

    void FrameOnZWall(float x, float z, float width, float height)
    {
        float left = x - width * 0.5f;
        float right = x + width * 0.5f;
        geo.Box(level, "Frame", new Vector3(left, height * 0.5f, z), new Vector3(0.08f, height, WallT + 0.06f), mats.metal, Vector2.one, false, true);
        geo.Box(level, "Frame", new Vector3(right, height * 0.5f, z), new Vector3(0.08f, height, WallT + 0.06f), mats.metal, Vector2.one, false, true);
        geo.Box(level, "Frame", new Vector3(x, height, z), new Vector3(width, 0.08f, WallT + 0.06f), mats.metal, Vector2.one, false, true);
    }

    void FrameOnXWall(float x, float z, float width, float height)
    {
        float south = z - width * 0.5f;
        float north = z + width * 0.5f;
        geo.Box(level, "Frame", new Vector3(x, height * 0.5f, south), new Vector3(WallT + 0.06f, height, 0.08f), mats.metal, Vector2.one, false, true);
        geo.Box(level, "Frame", new Vector3(x, height * 0.5f, north), new Vector3(WallT + 0.06f, height, 0.08f), mats.metal, Vector2.one, false, true);
        geo.Box(level, "Frame", new Vector3(x, height, z), new Vector3(WallT + 0.06f, 0.08f, width), mats.metal, Vector2.one, false, true);
    }

    void ApplyAtmosphere()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.04f;
        RenderSettings.fogColor = new Color(0.015f, 0.017f, 0.022f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.05f, 0.052f, 0.06f);
        QualitySettings.pixelLightCount = 8;
        QualitySettings.shadowDistance = 28f;
    }

    void DisableSceneDefaults()
    {
        Camera[] cameras = Object.FindObjectsOfType<Camera>();
        for (int i = 0; i < cameras.Length; i++)
        {
            if (level != null && cameras[i].transform.IsChildOf(level))
                continue;
            cameras[i].gameObject.SetActive(false);
        }

        Light[] lights = Object.FindObjectsOfType<Light>();
        for (int i = 0; i < lights.Length; i++)
        {
            if (level != null && lights[i].transform.IsChildOf(level))
                continue;
            if (lights[i].type == LightType.Directional)
                lights[i].gameObject.SetActive(false);
        }
    }

    int ValidateNav()
    {
        if (graph == null)
            return 0;
        int problems = 0;
        for (int i = 0; i < graph.linkA.Count; i++)
        {
            Vector3 a = graph.NodePosition(graph.linkA[i]) + Vector3.up;
            Vector3 b = graph.NodePosition(graph.linkB[i]) + Vector3.up;
            Vector3 delta = b - a;
            float distance = delta.magnitude;
            if (distance < 0.01f)
                continue;
            RaycastHit[] hits = Physics.RaycastAll(a, delta / distance, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int h = 0; h < hits.Length; h++)
            {
                string hitName = hits[h].collider.name;
                if (!hitName.StartsWith("Wall") && !hitName.StartsWith("Floor") && !hitName.StartsWith("Ceil") && !hitName.StartsWith("Frame"))
                    continue;
                problems++;
                Debug.LogWarning("导航被几何体挡住: " + graph.names[graph.linkA[i]] + " -> " + graph.names[graph.linkB[i]] + " / " + hitName);
            }
        }
        return problems;
    }

    static Room[] Rooms()
    {
        return new Room[]
        {
            new Room("Lobby", -6f, 6f, 0f, 14f, true),
            new Room("Storage", -12.2f, -6f, 3f, 10f, true),
            new Room("Restroom", 6f, 12.2f, 3f, 10f, true),
            new Room("Corridor", -2f, 2f, 14f, 22f, true),
            new Room("Nurse", -8f, 8f, 22f, 38f, true),
            new Room("Office", -14.5f, -8f, 26f, 34f, true),
            new Room("Exam", 8f, 14.5f, 26f, 34f, true),
            new Room("Ward", -2f, 2f, 38f, 72f, true),
            new Room("R301", -10f, -2f, 42f, 49f, true),
            new Room("R302", 2f, 10f, 42f, 49f, true),
            new Room("R305", -10f, -2f, 52f, 59f, true),
            new Room("R306", 2f, 10f, 52f, 59f, true),
            new Room("R307", -10f, -2f, 62f, 69f, true),
            new Room("Morgue", 2f, 12f, 62f, 72f, true),
            new Room("Bay", -6f, 6f, 72f, 84f, false)
        };
    }

    void OnDrawGizmos()
    {
        if (GameObject.Find("HospitalRoot") != null)
            return;
        Room[] rooms = Rooms();
        for (int i = 0; i < rooms.Length; i++)
        {
            Room room = rooms[i];
            Vector3 center = new Vector3((room.x0 + room.x1) * 0.5f, 0.2f, (room.z0 + room.z1) * 0.5f);
            Vector3 size = new Vector3(room.x1 - room.x0, 0.4f, room.z1 - room.z0);
            Gizmos.color = new Color(0.35f, 0.7f, 0.85f, 0.35f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = new Color(0.6f, 0.85f, 1f, 0.9f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
