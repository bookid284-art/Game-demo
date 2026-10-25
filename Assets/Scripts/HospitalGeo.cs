using UnityEngine;

/// <summary>
/// 用 Unity 内置原始体摆放盒子、圆柱和薄片，并铺上程序贴图。
/// </summary>
public class HospitalGeo
{
    public Transform level;
    public HospitalMaterials mats;
    System.Random rng = new System.Random(11);

    public HospitalGeo(Transform level, HospitalMaterials mats)
    {
        this.level = level;
        this.mats = mats;
    }

    public GameObject Box(Transform parent, string name, Vector3 localCenter, Vector3 size, Material mat, Vector2 tile, bool collider, bool isStatic)
    {
        return Primitive(PrimitiveType.Cube, parent, name, localCenter, size, Quaternion.identity, mat, tile, collider, isStatic);
    }

    public GameObject Sphere(Transform parent, string name, Vector3 localCenter, float radius, Material mat, bool collider)
    {
        return Primitive(PrimitiveType.Sphere, parent, name, localCenter, Vector3.one * (radius * 2f), Quaternion.identity, mat, Vector2.one, collider, false);
    }

    public GameObject Capsule(Transform parent, string name, Vector3 localCenter, float radius, float height, Quaternion localRot, Material mat, bool collider)
    {
        Vector3 scale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
        return Primitive(PrimitiveType.Capsule, parent, name, localCenter, scale, localRot, mat, Vector2.one, collider, false);
    }

    public GameObject Cylinder(Transform parent, string name, Vector3 localCenter, float radius, float height, Quaternion localRot, Material mat, bool collider)
    {
        Vector3 scale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
        return Primitive(PrimitiveType.Cylinder, parent, name, localCenter, scale, localRot, mat, Vector2.one, collider, false);
    }

    public GameObject Quad(Transform parent, string name, Vector3 localCenter, Vector2 size, Quaternion localRot, Material mat, bool collider)
    {
        return Primitive(PrimitiveType.Quad, parent, name, localCenter, new Vector3(size.x, size.y, 1f), localRot, mat, Vector2.one, collider, false);
    }

    GameObject Primitive(PrimitiveType type, Transform parent, string name, Vector3 localCenter, Vector3 scale, Quaternion localRot, Material mat, Vector2 tile, bool collider, bool isStatic)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        Transform target = parent != null ? parent : level;
        go.transform.SetParent(target, false);
        go.transform.localPosition = localCenter;
        go.transform.localRotation = localRot;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null && mat != null)
        {
            renderer.sharedMaterial = mat;
            Color tint = Color.white;
            if (mat == mats.wall)
            {
                float j = 0.93f + (float)rng.NextDouble() * 0.07f;
                tint = new Color(j, j * 0.99f, j * 0.96f);
            }
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_Color", tint);
            block.SetVector("_MainTex_ST", new Vector4(tile.x, tile.y, 0f, 0f));
            renderer.SetPropertyBlock(block);
        }
        if (!collider)
        {
            Collider existing = go.GetComponent<Collider>();
            if (existing != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(existing);
                else
                    Object.DestroyImmediate(existing);
            }
        }
        go.isStatic = isStatic;
        return go;
    }
}
