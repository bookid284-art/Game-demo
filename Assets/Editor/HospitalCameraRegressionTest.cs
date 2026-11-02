using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Unity -batchmode -nographics -projectPath . -executeMethod HospitalCameraRegressionTest.Run -logFile camera-tests.log
/// Uses the production camera placement and Unity Physics in a disposable scene.
/// Run in a separate batch-mode editor; this method replaces the scene and exits.
/// </summary>
public static class HospitalCameraRegressionTest
{
    static int failures;
    const float Radius = 0.16f;
    const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    public static void Run()
    {
        failures = 0;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Check("immediate rear-wall retraction", TestRearWall);
        Check("right shoulder beside ward wall", () => TestShoulder(0f, 1f));
        Check("rotated shoulder beside left wall", () => TestShoulder(180f, -1f));
        Check("corner with camera pitch and recoil", TestCorner);
        Debug.Log("Hospital camera regression: 4 cases, " + failures + " failures");
        EditorApplication.Exit(failures == 0 ? 0 : 2);
    }

    static void Check(string label, Action test)
    {
        try { test(); Debug.Log("PASS: " + label); }
        catch (Exception e) { failures++; Debug.LogError("FAIL: " + label + "\n" + e); }
    }

    sealed class Fixture : IDisposable
    {
        public readonly GameObject root = new GameObject("Camera test fixture");
        public readonly ThirdPersonController motor;

        public Fixture()
        {
            GameObject actor = new GameObject("Player");
            actor.transform.SetParent(root.transform, false);
            CharacterController body = actor.AddComponent<CharacterController>();
            body.radius = 0.32f;
            body.height = 1.8f;
            body.center = new Vector3(0f, 0.92f, 0f);
            motor = actor.AddComponent<ThirdPersonController>();
            motor.cameraPivot = new GameObject("Pivot").transform;
            motor.cameraPivot.SetParent(actor.transform, false);
            motor.cameraPivot.localPosition = new Vector3(0.4f, 1.52f, 0.08f);
            motor.cameraHolder = new GameObject("Camera").transform;
            motor.cameraHolder.SetParent(motor.cameraPivot, false);
            Set("currentDistance", 2.2f);
        }

        public Collider Wall(Vector3 center, Vector3 size)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.SetParent(root.transform, false);
            wall.transform.position = center;
            wall.transform.localScale = size;
            Physics.SyncTransforms();
            return wall.GetComponent<Collider>();
        }

        public void Place()
        {
            Physics.SyncTransforms();
            typeof(ThirdPersonController).GetMethod("PlaceCamera", PrivateInstance)
                .Invoke(motor, new object[] { false });
        }

        public void Set(string name, object value)
        {
            typeof(ThirdPersonController).GetField(name, PrivateInstance).SetValue(motor, value);
        }

        public void ClearOf(Collider wall)
        {
            Assert(Vector3.Distance(motor.cameraHolder.position, wall.ClosestPoint(motor.cameraHolder.position))
                >= Radius - 0.001f, "Camera sphere intersects wall");
            Assert(Vector3.Distance(motor.cameraPivot.position, wall.ClosestPoint(motor.cameraPivot.position))
                >= Radius - 0.001f, "Shoulder pivot sphere intersects wall");
        }

        public void Dispose() { UnityEngine.Object.DestroyImmediate(root); }
    }

    static void TestRearWall()
    {
        using (Fixture f = new Fixture())
        {
            Collider wall = f.Wall(new Vector3(0f, 1.5f, -0.9f), new Vector3(6f, 3f, 0.2f));
            f.Place();
            f.ClearOf(wall);
            Assert(f.motor.cameraHolder.position.z >= -0.64f, "Camera stayed behind the wall on the first frame");
        }
    }

    static void TestShoulder(float yaw, float side)
    {
        using (Fixture f = new Fixture())
        {
            f.motor.transform.position = new Vector3(1.58f * side, 0f, 0f);
            f.motor.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Collider wall = f.Wall(new Vector3(2f * side, 1.5f, 0f), new Vector3(0.2f, 3f, 10f));
            for (int frame = 0; frame < 20; frame++)
            {
                f.Place();
                f.ClearOf(wall);
                Assert(f.motor.cameraHolder.position.x * side <= 1.74f,
                    "Camera crossed to the far side of the wall");
            }
            UnityEngine.Object.DestroyImmediate(wall.gameObject);
            f.Place();
            Assert(Mathf.Abs(f.motor.cameraPivot.localPosition.x - 0.4f) < 0.001f,
                "Original shoulder offset was lost after collision recovery");
        }
    }

    static void TestCorner()
    {
        using (Fixture f = new Fixture())
        {
            f.motor.transform.position = new Vector3(1.58f, 0f, 0f);
            Collider side = f.Wall(new Vector3(2f, 1.5f, 0f), new Vector3(0.2f, 3f, 10f));
            Collider back = f.Wall(new Vector3(0f, 1.5f, -0.5f), new Vector3(6f, 3f, 0.2f));
            foreach (float pitch in new float[] { -42f, 0f, 48f })
            {
                f.motor.cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                f.Set("recoil", 0.16f);
                f.Set("currentDistance", 2.2f);
                f.Place();
                f.ClearOf(side);
                f.ClearOf(back);
                Assert(f.motor.cameraHolder.position.z >= -0.24f, "Camera crossed rear wall at a tight corner");
            }
        }
    }

    static void Assert(bool ok, string message)
    {
        if (!ok) throw new InvalidOperationException(message);
    }
}
