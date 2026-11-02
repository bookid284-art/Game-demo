using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Isolated tests of the production AI methods, including real Physics raycasts.
/// Unity -batchmode -nographics -projectPath . -executeMethod HospitalAIRegressionTest.Run -logFile -
/// Run in a disposable batch-mode editor: this replaces the current scene and exits.
/// </summary>
public static class HospitalAIRegressionTest
{
    const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    static int failures;
    static int cases;

    public static void Run()
    {
        failures = 0;
        cases = 0;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        foreach (int fps in new int[] { 15, 30, 60, 144 })
        {
            foreach (float speed in new float[] { 1.7f, 5.3f, 6.3f })
            {
                int rate = fps;
                float pace = speed;
                RunCase("long corridor " + rate + " FPS / " + pace, () => TestLongCorridor(rate, pace));
            }
        }
        RunCase("patrol exits room via corridor", TestPatrol);
        RunCase("changed goal preserves junction", TestChangedGoal);
        RunCase("closed link stays closed", TestClosedLink);
        RunCase("catch requires clear sight", TestCatch);
        Debug.Log("Hospital AI regression: " + cases + " cases, " + failures + " failures");
        EditorApplication.Exit(failures == 0 ? 0 : 2);
    }

    static void RunCase(string label, Action test)
    {
        cases++;
        try
        {
            test();
            Debug.Log("PASS: " + label);
        }
        catch (Exception exception)
        {
            failures++;
            Debug.LogError("FAIL: " + label + "\n" + exception);
        }
    }

    sealed class Fixture : IDisposable
    {
        public readonly GameObject root = new GameObject("AI regression fixture");
        public readonly HospitalNavGraph graph;
        public readonly MonsterWatcherAI monster;

        public Fixture()
        {
            graph = root.AddComponent<HospitalNavGraph>();
            GameObject actor = new GameObject("Monster");
            actor.transform.SetParent(root.transform);
            monster = actor.AddComponent<MonsterWatcherAI>();
            monster.graph = graph;
        }

        public Vector3 Target(Vector3 goal, bool replan)
        {
            // Supply a deterministic clock in Edit Mode instead of depending
            // on the editor's Time.deltaTime. The actual routing method runs.
            Set(monster, "replanTimer", replan ? 0f : float.MaxValue);
            return (Vector3)Call(monster, "PathTowardWorld", goal);
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    static void TestLongCorridor(int fps, float speed)
    {
        using (Fixture f = new Fixture())
        {
            int a = f.graph.AddNode("a", 0f, 0f);
            int b = f.graph.AddNode("b", 0f, 10f);
            int c = f.graph.AddNode("c", 0f, 20f);
            f.graph.AddLink(a, b, true);
            f.graph.AddLink(b, c, true);
            Simulate(f, f.graph.NodePosition(c), fps, speed);
            Assert(f.monster.transform.position.z > 19.5f, "AI kept returning to a previous waypoint");
        }
    }

    static void Simulate(Fixture f, Vector3 goal, int fps, float speed)
    {
        float elapsed = 0.4f;
        float dt = 1f / fps;
        for (int frame = 0; frame < 30 * fps; frame++)
        {
            bool replan = elapsed >= 0.4f;
            if (replan)
                elapsed = 0f;
            Vector3 target = f.Target(goal, replan);
            Vector3 position = f.monster.transform.position;
            if (Vector3.Distance(position, target) > 0.3f)
                f.monster.transform.position = Vector3.MoveTowards(position, target, speed * dt);
            elapsed += dt;
        }
    }

    static void TestPatrol()
    {
        using (Fixture f = new Fixture())
        {
            // Coordinates from the generated ward: the direct diagonal crosses
            // the morgue wall, while the graph first leads through its doorway.
            int morgue = f.graph.AddNode("morgue", 7.2f, 67.4f);
            int junction = f.graph.AddNode("jMorgue", 0f, 67f);
            int ward = f.graph.AddNode("postWard", 0f, 40.6f);
            f.graph.AddLink(morgue, junction, true);
            f.graph.AddLink(junction, ward, true);
            f.monster.transform.position = f.graph.NodePosition(morgue);
            f.monster.patrolNodes = new int[] { ward, morgue };
            Vector3 target = (Vector3)Call(f.monster, "PatrolPoint");
            Assert(Vector3.Distance(target, f.graph.NodePosition(junction)) < 0.01f,
                "Patrol skipped the room exit and headed through a wall");
        }
    }

    static void TestChangedGoal()
    {
        using (Fixture f = new Fixture())
        {
            int a = f.graph.AddNode("junction", 0f, 0f);
            int b = f.graph.AddNode("north", 0f, 10f);
            int c = f.graph.AddNode("west", -10f, 0f);
            f.graph.AddLink(a, b, true);
            f.graph.AddLink(a, c, true);
            f.Target(f.graph.NodePosition(b), true);
            f.monster.transform.position = new Vector3(0f, 0f, 2f);
            Vector3 target = f.Target(f.graph.NodePosition(c), true);
            Assert(Vector3.Distance(target, Vector3.zero) < 0.01f,
                "Changed route cut across a corner instead of returning to the junction");
        }
    }

    static void TestClosedLink()
    {
        using (Fixture f = new Fixture())
        {
            int a = f.graph.AddNode("a", 0f, 0f);
            int b = f.graph.AddNode("beforeDoor", 0f, 10f);
            int c = f.graph.AddNode("behindDoor", 0f, 20f);
            f.graph.AddLink(a, b, true);
            f.graph.AddLink(b, c, false);
            Simulate(f, f.graph.NodePosition(c), 60, 5.3f);
            Assert(Mathf.Abs(f.monster.transform.position.z - 10f) < 0.5f,
                "AI crossed a closed link or failed to approach it");
        }
    }

    static void TestCatch()
    {
        using (Fixture f = new Fixture())
        {
            GameObject player = new GameObject("Player");
            player.transform.SetParent(f.root.transform);
            player.transform.position = new Vector3(1f, 0f, 0f);
            f.monster.player = player.transform;
            Set(f.monster, "hunting", true);
            Set(f.monster, "frozen", false);
            Physics.SyncTransforms();
            Assert((bool)Call(f.monster, "CanCatchPlayer"), "Nearby unobstructed player must be catchable");

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.SetParent(f.root.transform);
            wall.transform.position = new Vector3(0.5f, 1.5f, 0f);
            wall.transform.localScale = new Vector3(0.2f, 3f, 4f);
            Physics.SyncTransforms();
            Assert(!(bool)Call(f.monster, "CanCatchPlayer"), "AI caught player through a solid wall");
            UnityEngine.Object.DestroyImmediate(wall);
            Physics.SyncTransforms();
            Assert((bool)Call(f.monster, "CanCatchPlayer"), "Removing the wall should permit catching again");

            Set(f.monster, "frozen", true);
            Assert(!(bool)Call(f.monster, "CanCatchPlayer"), "Observed AI must not catch player");
            Set(f.monster, "frozen", false);
            Set(f.monster, "hunting", false);
            Assert(!(bool)Call(f.monster, "CanCatchPlayer"), "Patrolling AI must not catch player");
            Set(f.monster, "hunting", true);
            player.transform.position = new Vector3(2f, 0f, 0f);
            Assert(!(bool)Call(f.monster, "CanCatchPlayer"), "Distant player must not be caught");
        }
    }

    static object Call(object target, string method, params object[] args)
    {
        MethodInfo info = target.GetType().GetMethod(method, PrivateInstance);
        if (info == null)
            throw new MissingMethodException(target.GetType().Name, method);
        return info.Invoke(target, args);
    }

    static void Set(object target, string field, object value)
    {
        FieldInfo info = target.GetType().GetField(field, PrivateInstance);
        if (info == null)
            throw new MissingFieldException(target.GetType().Name, field);
        info.SetValue(target, value);
    }

    static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
