using UnityEngine;

public class HospitalSceneBuilder : MonoBehaviour
{
    [ContextMenu("Build Hospital Graybox")]
    public void Build()
    {
        ClearChildren();

        GameObject environmentRoot = new GameObject("Environment");
        environmentRoot.transform.SetParent(transform);

        BuildCorridor(environmentRoot.transform);
        BuildRooms(environmentRoot.transform);
        BuildDoors(environmentRoot.transform);
        BuildLights(environmentRoot.transform);

        GameObject player = BuildPlayer();
        GameObject key = BuildKey(environmentRoot.transform);
        GameObject monster = BuildMonster(environmentRoot.transform, player.transform);

        monster.name = "BlackHumanoidMonster";
        key.name = "Key";
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void BuildCorridor(Transform parent)
    {
        CreateCube("Floor", new Vector3(0f, 0f, 20f), new Vector3(4f, 0.2f, 40f), parent, new Color(0.35f, 0.35f, 0.38f));
        CreateCube("Ceiling", new Vector3(0f, 3f, 20f), new Vector3(4f, 0.2f, 40f), parent, new Color(0.25f, 0.25f, 0.3f));
        CreateCube("LeftWall", new Vector3(-2f, 1.5f, 20f), new Vector3(0.2f, 3f, 40f), parent, new Color(0.6f, 0.6f, 0.63f));
        CreateCube("RightWall", new Vector3(2f, 1.5f, 20f), new Vector3(0.2f, 3f, 40f), parent, new Color(0.6f, 0.6f, 0.63f));
    }

    private void BuildRooms(Transform parent)
    {
        BuildRoom(parent, new Vector3(-6f, 0f, 8f), "Room_A");
        BuildRoom(parent, new Vector3(6f, 0f, 18f), "Room_B");
        BuildRoom(parent, new Vector3(-6f, 0f, 28f), "Room_C");
    }

    private void BuildRoom(Transform parent, Vector3 origin, string roomName)
    {
        Transform room = new GameObject(roomName).transform;
        room.SetParent(parent);

        CreateCube("Floor", origin + new Vector3(0f, 0f, 0f), new Vector3(8f, 0.2f, 8f), room, new Color(0.3f, 0.3f, 0.33f));
        CreateCube("Ceiling", origin + new Vector3(0f, 3f, 0f), new Vector3(8f, 0.2f, 8f), room, new Color(0.2f, 0.2f, 0.25f));
        CreateCube("Wall_Back", origin + new Vector3(0f, 1.5f, -4f), new Vector3(8f, 3f, 0.2f), room, new Color(0.55f, 0.55f, 0.58f));
        CreateCube("Wall_Left", origin + new Vector3(-4f, 1.5f, 0f), new Vector3(0.2f, 3f, 8f), room, new Color(0.55f, 0.55f, 0.58f));
        CreateCube("Wall_Right", origin + new Vector3(4f, 1.5f, 0f), new Vector3(0.2f, 3f, 8f), room, new Color(0.55f, 0.55f, 0.58f));
    }

    private void BuildDoors(Transform parent)
    {
        GameObject endDoor = CreateCube("EndDoor", new Vector3(0f, 1f, 39f), new Vector3(1.5f, 2f, 0.15f), parent, new Color(0.15f, 0.15f, 0.15f));
        endDoor.AddComponent<DoorController>();

        GameObject exitDoor = CreateCube("EscapeExit", new Vector3(0f, 1f, 41f), new Vector3(2f, 2f, 0.15f), parent, new Color(0.05f, 0.4f, 0.1f));
        exitDoor.AddComponent<EscapeDoor>();
    }

    private void BuildLights(Transform parent)
    {
        RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.07f);

        for (int i = 0; i < 4; i++)
        {
            GameObject lamp = new GameObject($"FlickerLamp_{i + 1}");
            lamp.transform.SetParent(parent);
            lamp.transform.position = new Vector3(0f, 2.8f, 6f + i * 9f);

            Light lightComp = lamp.AddComponent<Light>();
            lightComp.type = LightType.Point;
            lightComp.range = 8f;
            lightComp.intensity = 0.8f;
            lightComp.color = new Color(0.7f, 0.75f, 0.85f);
        }
    }

    private GameObject BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 1f, 1f);

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.25f;

        player.AddComponent<PlayerInventory>();
        player.AddComponent<BasicFirstPersonController>();

        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.SetParent(player.transform);
        cameraObj.transform.localPosition = new Vector3(0f, 0.75f, 0f);

        Camera cam = cameraObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.01f, 0.01f, 0.01f);

        Light flashlight = cameraObj.AddComponent<Light>();
        flashlight.type = LightType.Spot;
        flashlight.range = 20f;
        flashlight.spotAngle = 55f;
        flashlight.intensity = 2.2f;
        flashlight.color = new Color(0.85f, 0.85f, 0.8f);

        BasicFirstPersonController controller = player.GetComponent<BasicFirstPersonController>();
        controller.SetCameraRoot(cameraObj.transform);

        return player;
    }

    private GameObject BuildKey(Transform parent)
    {
        GameObject key = CreateCube("Key", new Vector3(-6f, 1f, 8f), new Vector3(0.25f, 0.1f, 0.5f), parent, Color.yellow);
        key.AddComponent<KeyPickup>();
        return key;
    }

    private GameObject BuildMonster(Transform parent, Transform player)
    {
        GameObject monster = new GameObject("Monster");
        monster.transform.SetParent(parent);
        monster.transform.position = new Vector3(0f, 0f, 34f);

        CreateCube("Body", new Vector3(0f, 1.1f, 0f), new Vector3(0.8f, 1.4f, 0.4f), monster.transform, Color.black);
        CreateCube("Head", new Vector3(0f, 2f, 0f), new Vector3(0.5f, 0.5f, 0.5f), monster.transform, Color.black);
        CreateCube("LeftArm", new Vector3(-0.55f, 1.1f, 0f), new Vector3(0.25f, 1.2f, 0.25f), monster.transform, Color.black);
        CreateCube("RightArm", new Vector3(0.55f, 1.1f, 0f), new Vector3(0.25f, 1.2f, 0.25f), monster.transform, Color.black);
        CreateCube("LeftLeg", new Vector3(-0.2f, 0.3f, 0f), new Vector3(0.25f, 0.8f, 0.25f), monster.transform, Color.black);
        CreateCube("RightLeg", new Vector3(0.2f, 0.3f, 0f), new Vector3(0.25f, 0.8f, 0.25f), monster.transform, Color.black);

        BoxCollider collider = monster.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 1f, 0f);
        collider.size = new Vector3(1.5f, 2.5f, 1f);

        MonsterWatcherAI ai = monster.AddComponent<MonsterWatcherAI>();
        ai.SetTarget(player);

        return monster;
    }

    private GameObject CreateCube(string name, Vector3 position, Vector3 scale, Transform parent, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.localPosition = position;
        cube.transform.localScale = scale;

        Renderer renderer = cube.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        renderer.material = mat;

        return cube;
    }
}
