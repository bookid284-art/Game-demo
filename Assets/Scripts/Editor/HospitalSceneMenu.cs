using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 顶部菜单：生成或清除医院灰盒，并打开默认场景。
/// </summary>
public static class HospitalSceneMenu
{
    [MenuItem("Game Demo/Build Hospital Graybox")]
    public static void Build()
    {
        HospitalSceneBuilder builder = Object.FindObjectOfType<HospitalSceneBuilder>();
        if (builder == null)
        {
            GameObject go = new GameObject("HospitalSceneBuilder");
            Undo.RegisterCreatedObjectUndo(go, "Create Hospital Scene Builder");
            builder = go.AddComponent<HospitalSceneBuilder>();
        }
        builder.BuildHospitalGraybox();
    }

    [MenuItem("Game Demo/Clear Hospital Graybox")]
    public static void Clear()
    {
        HospitalSceneBuilder builder = Object.FindObjectOfType<HospitalSceneBuilder>();
        if (builder != null)
            builder.ClearBuiltLevel();
        else
        {
            GameObject existing = GameObject.Find("HospitalRoot");
            if (existing != null)
                Undo.DestroyObjectImmediate(existing);
        }
    }

    [MenuItem("Game Demo/Open Hospital Scene")]
    public static void OpenScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Hospital.unity");
    }
}
