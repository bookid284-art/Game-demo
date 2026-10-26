using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 在编辑器里生成医院并做物理检测。命令行：
/// Unity -batchmode -nographics -projectPath . -executeMethod HospitalBatchTest.Run -logFile -
/// </summary>
public static class HospitalBatchTest
{
    const string ResultPath = "/tmp/hospital-test-result.txt";
    const float BodyRadius = 0.32f;

    static readonly List<string> Failures = new List<string>();

    public static void Run()
    {
        Failures.Clear();
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Hospital.unity");
            HospitalSceneBuilder builder = UnityEngine.Object.FindObjectOfType<HospitalSceneBuilder>();
            if (builder == null)
                Fail("场景里没有 HospitalSceneBuilder");
            else
                builder.BuildHospitalGraybox();

            Physics.SyncTransforms();
            WakeManagers();

            TestWalk("装卸区向东进手术走廊", new Vector3(0f, 0.2f, 78f), new Vector3(12f, 0.2f, 78f), true);
            TestWalk("装卸区向西进精神科走廊", new Vector3(0f, 0.2f, 78f), new Vector3(-12f, 0.2f, 78f), true);
            TestWalk("手术走廊", new Vector3(8f, 0.2f, 78f), new Vector3(40f, 0.2f, 78f), true);
            TestWalk("精神科走廊", new Vector3(-8f, 0.2f, 78f), new Vector3(-40f, 0.2f, 78f), true);
            TestWalk("行政走廊", new Vector3(-11f, 0.2f, 6.5f), new Vector3(-42f, 0.2f, 6.5f), true);
            TestWalk("急诊走廊", new Vector3(13f, 0.2f, 6.5f), new Vector3(44f, 0.2f, 6.5f), true);
            TestWalk("病房中线", new Vector3(0f, 0.2f, 42f), new Vector3(0f, 0.2f, 70f), true);
            TestWalk("院子中线", new Vector3(0f, 0.2f, 86f), new Vector3(0f, 0.2f, 106f), true);
            TestWalk("关闭的装卸闸门", new Vector3(0f, 0.2f, 70f), new Vector3(0f, 0.2f, 76f), false);

            SetBlockersEnabled(71.5f, 72.5f, false);
            TestWalk("打开的装卸闸门", new Vector3(0f, 0.2f, 70f), new Vector3(0f, 0.2f, 76f), true);
            SetBlockersEnabled(71.5f, 72.5f, true);

            TestWalk("关闭的北围栏", new Vector3(0f, 0.2f, 80f), new Vector3(0f, 0.2f, 90f), false);
            SetBlockersEnabled(83.5f, 84.5f, false);
            TestWalk("打开的北围栏", new Vector3(0f, 0.2f, 80f), new Vector3(0f, 0.2f, 90f), true);
            SetBlockersEnabled(83.5f, 84.5f, true);

            TestRayHits("库房钥匙", new Vector3(-7.8f, 1.35f, 8.7f), new Vector3(-8.68f, 1.35f, 8.7f), "Key_storage");
            TestRayHits("零号观察", NotePosition("零号观察") + new Vector3(0f, 0f, -1.2f), NotePosition("零号观察"), "Note");
            TestRayHits("疏散打印机", new Vector3(-36.5f, 1.05f, 2.45f), new Vector3(-40.6f, 1.05f, 2.45f), "Printer");
            TestOverlaps();
            TestNavRays();
            TestValveRepeat();
            TestFinishBeatsDeath();
            TestMonsterLeash();
            TestMonsterFence();
            TestPrinterGivesKey();
        }
        catch (Exception exception)
        {
            Fail("测试中断: " + exception);
        }

        StringBuilder report = new StringBuilder();
        report.Append("failures=").Append(Failures.Count).Append('\n');
        for (int i = 0; i < Failures.Count; i++)
            report.Append("- ").Append(Failures[i]).Append('\n');
        if (Failures.Count == 0)
            report.Append("医院物理与流程检测通过\n");
        File.WriteAllText(ResultPath, report.ToString());
        if (Failures.Count == 0)
            Debug.Log(report.ToString());
        else
            Debug.LogError(report.ToString());
        EditorApplication.Exit(Failures.Count == 0 ? 0 : 2);
    }

    static void WakeManagers()
    {
        GameManager manager = UnityEngine.Object.FindObjectOfType<GameManager>();
        if (manager == null)
        {
            Fail("没有 GameManager");
            return;
        }
        if (GameManager.Instance == null)
            Invoke(manager, "Awake");
    }

    static void TestWalk(string label, Vector3 from, Vector3 to, bool expectClear)
    {
        string blocker;
        bool clear = Walk(from, to, out blocker);
        if (clear != expectClear)
        {
            if (expectClear)
                Fail(label + " 被挡住: " + blocker);
            else
                Fail(label + " 应该被挡住，但是走通了");
        }
    }

    static bool Walk(Vector3 from, Vector3 to, out string blocker)
    {
        Vector3 delta = to - from;
        delta.y = 0f;
        float distance = delta.magnitude;
        if (distance < 0.01f)
        {
            blocker = "";
            return true;
        }
        Vector3 direction = delta / distance;
        Vector3 pos = from;
        float step = 0.2f;
        for (float traveled = 0f; traveled < distance - 0.001f; traveled += step)
        {
            float pace = Mathf.Min(step, distance - traveled);
            Vector3 lower = pos + new Vector3(0f, 0.40f, 0f);
            Vector3 upper = pos + new Vector3(0f, 1.70f, 0f);
            RaycastHit hit;
            if (Physics.CapsuleCast(lower, upper, BodyRadius, direction, out hit, pace, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                blocker = hit.collider.name + " @ " + hit.point.ToString("F2");
                return false;
            }
            pos += direction * pace;
        }
        blocker = "";
        return true;
    }

    static void TestRayHits(string label, Vector3 from, Vector3 to, string expectedName)
    {
        if (float.IsNaN(to.x))
        {
            Fail(label + " 找不到目标");
            return;
        }
        Vector3 delta = to - from;
        float distance = delta.magnitude;
        if (distance < 0.05f)
        {
            Fail(label + " 射线太短");
            return;
        }
        RaycastHit hit;
        if (!Physics.Raycast(from, delta / distance, out hit, distance + 0.4f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Fail(label + " 射线没有打到任何碰撞体");
            return;
        }
        if (hit.collider.name != expectedName && hit.collider.transform.root.name != expectedName
            && !hit.collider.transform.name.StartsWith(expectedName)
            && (hit.collider.transform.parent == null || !hit.collider.transform.parent.name.StartsWith(expectedName)))
            Fail(label + " 先打到 " + hit.collider.name + "，期望 " + expectedName);
    }

    static Vector3 NotePosition(string title)
    {
        NoteInteractable[] notes = UnityEngine.Object.FindObjectsOfType<NoteInteractable>();
        for (int i = 0; i < notes.Length; i++)
        {
            if (notes[i].title == title)
                return notes[i].transform.position;
        }
        return new Vector3(float.NaN, float.NaN, float.NaN);
    }

    static void TestOverlaps()
    {
        MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
        int checkedColliders = 0;
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (!(behaviours[i] is IInteractable))
                continue;
            Collider[] own = behaviours[i].GetComponents<Collider>();
            for (int c = 0; c < own.Length; c++)
            {
                Collider collider = own[c];
                if (collider == null || !collider.enabled)
                    continue;
                checkedColliders++;
                Vector3 half = collider.bounds.extents * 0.96f;
                if (half.x < 0.02f || half.y < 0.02f || half.z < 0.02f)
                    continue;
                Collider[] hits = Physics.OverlapBox(collider.bounds.center, half, Quaternion.identity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                for (int h = 0; h < hits.Length; h++)
                {
                    Collider other = hits[h];
                    if (other == null || other == collider)
                        continue;
                    if (other.transform.IsChildOf(behaviours[i].transform))
                        continue;
                    if (other.GetComponentInParent<IInteractable>() != null)
                        continue;
                    string name = other.name;
                    if (name.StartsWith("Floor") || name.StartsWith("Ceil"))
                        continue;
                    Fail(behaviours[i].name + " 嵌进 " + name);
                }
            }
        }
        if (checkedColliders < 20)
            Fail("可交互碰撞体只有 " + checkedColliders + " 个");
    }

    static void TestNavRays()
    {
        HospitalNavGraph graph = UnityEngine.Object.FindObjectOfType<HospitalNavGraph>();
        if (graph == null)
        {
            Fail("没有导航图");
            return;
        }
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
                if (problems <= 8)
                    Fail("导航穿墙 " + graph.names[graph.linkA[i]] + " -> " + graph.names[graph.linkB[i]] + " / " + hitName);
            }
        }
    }

    static void TestValveRepeat()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null)
        {
            Fail("GameManager.Instance 为空");
            return;
        }
        manager.PushToken("valve", "白");
        manager.PushToken("valve", "白");
        manager.PushToken("valve", "绿");
        manager.PushToken("valve", "红");
        manager.PushToken("valve", "蓝");
        if (!manager.HasFlag("valves"))
            Fail("重复扳动白阀之后，顺序没有走完");
    }

    static void TestFinishBeatsDeath()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null || manager.player == null)
        {
            Fail("通关判定缺少玩家");
            return;
        }
        Vector3 original = manager.player.transform.position;
        manager.outerOpen = true;
        manager.player.transform.position = new Vector3(0f, 0.08f, 105f);
        manager.Kill();
        if (manager.State != GameManager.RunState.Won)
            Fail("越过终点时 Kill 没有改判通关，当前状态 " + manager.State);
        manager.player.transform.position = original;
    }

    static void TestMonsterLeash()
    {
        GameManager manager = GameManager.Instance;
        MonsterWatcherAI monster = UnityEngine.Object.FindObjectOfType<MonsterWatcherAI>();
        if (manager == null || monster == null || manager.player == null)
        {
            Fail("怪物牵引测试缺少对象");
            return;
        }
        OpenLink("preGate", "bay");
        manager.gateOpen = true;
        SetHunting(monster, true);
        Vector3 playerOrigin = manager.player.transform.position;
        Vector3 monsterOrigin = monster.transform.position;
        manager.player.transform.position = new Vector3(0f, 0.08f, 90f);
        monster.transform.position = new Vector3(0f, 0.05f, 76f);
        Vector3 target = ChooseTarget(monster);
        if (target.z > 80f)
            Fail("闸门以北时怪物仍朝玩家走，目标 z=" + target.z.ToString("F2"));
        manager.player.transform.position = playerOrigin;
        monster.transform.position = monsterOrigin;
    }

    static void TestMonsterFence()
    {
        GameManager manager = GameManager.Instance;
        MonsterWatcherAI monster = UnityEngine.Object.FindObjectOfType<MonsterWatcherAI>();
        if (manager == null || monster == null || manager.player == null)
        {
            Fail("围栏测试缺少对象");
            return;
        }
        manager.outerOpen = true;
        manager.ReleaseMonster();
        SetHunting(monster, true);
        Vector3 playerOrigin = manager.player.transform.position;
        Vector3 monsterOrigin = monster.transform.position;
        manager.player.transform.position = new Vector3(0f, 0.08f, 100f);
        monster.transform.position = new Vector3(0f, 0.05f, 78f);
        Vector3 target = ChooseTarget(monster);
        if (target.z > 84f)
            Fail("怪物跟着玩家穿过北围栏，目标 z=" + target.z.ToString("F2"));

        monster.transform.position = new Vector3(4f, 0.05f, 96f);
        target = ChooseTarget(monster);
        if (target.z > 97f || Mathf.Abs(target.x) > 0.2f)
            Fail("院子里的怪物没有先回到围栏中线，目标 " + target.ToString("F2"));
        manager.player.transform.position = playerOrigin;
        monster.transform.position = monsterOrigin;
    }

    static void TestPrinterGivesKey()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null)
            return;
        manager.SetFlag("stamp_er");
        manager.SetFlag("stamp_ward");
        manager.SetFlag("stamp_or");
        manager.SetFlag("stamp_psych");
        EvacPrinter printer = UnityEngine.Object.FindObjectOfType<EvacPrinter>();
        PlayerInteractor interactor = UnityEngine.Object.FindObjectOfType<PlayerInteractor>();
        if (printer == null || interactor == null)
        {
            Fail("找不到打印机或交互器");
            return;
        }
        printer.Interact(interactor);
        if (manager.inventory == null || !manager.inventory.HasKey("auth"))
            Fail("打印机没有给出疏散单");
        if (!manager.MonsterReleased)
            Fail("打印之后怪物没有被放开");
    }

    static void SetHunting(MonsterWatcherAI monster, bool hunting)
    {
        FieldInfo field = typeof(MonsterWatcherAI).GetField("hunting", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != null)
            field.SetValue(monster, hunting);
    }

    static Vector3 ChooseTarget(MonsterWatcherAI monster)
    {
        MethodInfo method = typeof(MonsterWatcherAI).GetMethod("ChooseTarget", BindingFlags.Instance | BindingFlags.NonPublic);
        if (method == null)
        {
            Fail("找不到 ChooseTarget");
            return Vector3.zero;
        }
        return (Vector3)method.Invoke(monster, null);
    }

    static void OpenLink(string a, string b)
    {
        HospitalNavGraph graph = UnityEngine.Object.FindObjectOfType<HospitalNavGraph>();
        if (graph == null)
            return;
        int ia = -1;
        int ib = -1;
        for (int i = 0; i < graph.names.Count; i++)
        {
            if (graph.names[i] == a)
                ia = i;
            if (graph.names[i] == b)
                ib = i;
        }
        if (ia >= 0 && ib >= 0)
            graph.SetOpen(ia, ib, true);
    }

    static void SetBlockersEnabled(float z0, float z1, bool enabled)
    {
        Collider[] colliders = UnityEngine.Object.FindObjectsOfType<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].name != "DoorBlocker")
                continue;
            float z = colliders[i].transform.position.z;
            if (z >= z0 && z <= z1)
                colliders[i].enabled = enabled;
        }
    }

    static void Invoke(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method != null)
            method.Invoke(target, null);
    }

    static void Fail(string message)
    {
        Failures.Add(message);
        Debug.LogError(message);
    }
}
