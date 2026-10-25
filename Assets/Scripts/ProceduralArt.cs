using UnityEngine;

/// <summary>
/// 代码生成的医院贴图与材质。建模仍使用 Unity 内置原始体。
/// 若在检视面板指定了材质，会改用外部资源（例如 Asset Store 免费材质）。
/// </summary>
public class HospitalMaterials
{
    public Material wall;
    public Material floor;
    public Material ceiling;
    public Material door;
    public Material metal;
    public Material wood;
    public Material fabric;
    public Material sheet;
    public Material skin;
    public Material gown;
    public Material paper;
    public Material paint;
    public Material glass;
    public Material blood;
    public Material brass;
    public Material steel;
    public Material keyRed;
    public Material eye;
    public Material cross;
    public Material exitGlow;
    public Material screen;
}

public static class ProceduralArt
{
    public static HospitalMaterials Create(Material wallOverride, Material floorOverride, Material ceilingOverride, Material doorOverride)
    {
        EnsureFolder();
        HospitalMaterials mats = new HospitalMaterials();
        Texture2D wallTex = SaveTex(WallTex(512), "Wall");
        Texture2D floorTex = SaveTex(FloorTex(512), "Floor");
        Texture2D ceilingTex = SaveTex(CeilingTex(256), "Ceiling");
        Texture2D doorTex = SaveTex(DoorTex(256), "Door");
        Texture2D metalTex = SaveTex(NoiseTex(128, new Color(0.45f, 0.48f, 0.5f), new Color(0.22f, 0.23f, 0.25f), 4), "Metal");
        Texture2D woodTex = SaveTex(WoodTex(128), "Wood");
        Texture2D fabricTex = SaveTex(NoiseTex(128, new Color(0.35f, 0.42f, 0.48f), new Color(0.16f, 0.2f, 0.26f), 9), "Fabric");
        Texture2D sheetTex = SaveTex(NoiseTex(128, new Color(0.72f, 0.78f, 0.8f), new Color(0.45f, 0.55f, 0.62f), 3), "Sheet");
        Texture2D skinTex = SaveTex(NoiseTex(128, new Color(0.78f, 0.74f, 0.68f), new Color(0.45f, 0.55f, 0.48f), 13), "Skin");
        Texture2D gownTex = SaveTex(NoiseTex(64, new Color(0.62f, 0.7f, 0.66f), new Color(0.32f, 0.4f, 0.38f), 6), "Gown");
        Texture2D paperTex = SaveTex(NoiseTex(64, new Color(0.82f, 0.78f, 0.68f), new Color(0.62f, 0.58f, 0.48f), 2), "Paper");

        mats.wall = SaveMat(Standard(wallTex, new Color(0.78f, 0.76f, 0.72f), 0.06f, 0f), "WallMat");
        mats.floor = SaveMat(Standard(floorTex, new Color(0.62f, 0.64f, 0.63f), 0.18f, 0f), "FloorMat");
        mats.ceiling = SaveMat(Standard(ceilingTex, new Color(0.7f, 0.7f, 0.68f), 0.04f, 0f), "CeilingMat");
        mats.door = SaveMat(Standard(doorTex, new Color(0.55f, 0.66f, 0.7f), 0.28f, 0.05f), "DoorMat");
        mats.metal = SaveMat(Standard(metalTex, Color.white, 0.62f, 0.72f), "MetalMat");
        mats.wood = SaveMat(Standard(woodTex, Color.white, 0.18f, 0f), "WoodMat");
        mats.fabric = SaveMat(Standard(fabricTex, Color.white, 0.04f, 0f), "FabricMat");
        mats.sheet = SaveMat(Standard(sheetTex, Color.white, 0.08f, 0f), "SheetMat");
        mats.skin = SaveMat(Standard(skinTex, Color.white, 0.16f, 0f), "SkinMat");
        mats.gown = SaveMat(Standard(gownTex, Color.white, 0.05f, 0f), "GownMat");
        mats.paper = SaveMat(Standard(paperTex, Color.white, 0.05f, 0f), "PaperMat");
        mats.paint = SaveMat(Standard(null, new Color(0.82f, 0.83f, 0.84f), 0.2f, 0f), "PaintMat");
        mats.glass = SaveMat(Glass(), "GlassMat");
        mats.blood = SaveMat(Blood(), "BloodMat");
        mats.brass = SaveMat(Emissive(new Color(0.85f, 0.62f, 0.18f), 1.4f, 0.45f, 0.6f), "BrassMat");
        mats.steel = SaveMat(Emissive(new Color(0.65f, 0.78f, 0.88f), 1.1f, 0.55f, 0.7f), "SteelMat");
        mats.keyRed = SaveMat(Emissive(new Color(0.75f, 0.08f, 0.07f), 1.6f, 0.35f, 0.2f), "KeyRedMat");
        mats.eye = SaveMat(Emissive(new Color(0.15f, 0.01f, 0.01f), 2.2f, 0.2f, 0f), "EyeMat");
        mats.cross = SaveMat(Emissive(new Color(0.7f, 0.05f, 0.05f), 1.3f, 0.25f, 0f), "CrossMat");
        mats.exitGlow = SaveMat(Emissive(new Color(0.15f, 0.85f, 0.35f), 1.8f, 0.2f, 0f), "ExitMat");
        mats.screen = SaveMat(Emissive(new Color(0.15f, 0.45f, 0.28f), 1.5f, 0.15f, 0f), "ScreenMat");

        if (wallOverride != null) mats.wall = wallOverride;
        if (floorOverride != null) mats.floor = floorOverride;
        if (ceilingOverride != null) mats.ceiling = ceilingOverride;
        if (doorOverride != null) mats.door = doorOverride;
        return mats;
    }

    static Shader LitShader()
    {
        Shader shader = Shader.Find("Standard");
        if (shader != null)
            return shader;
        shader = Shader.Find("Diffuse");
        if (shader != null)
            return shader;
        shader = Shader.Find("Legacy Shaders/Diffuse");
        if (shader != null)
            return shader;
        Debug.LogError("找不到 Standard 着色器。请使用 Built-in 渲染管线打开本工程，不要切换到 URP。");
        return Shader.Find("Unlit/Color");
    }

    static Material Standard(Texture2D tex, Color tint, float smoothness, float metallic)
    {
        Material mat = new Material(LitShader());
        mat.color = tint;
        if (tex != null)
            mat.mainTexture = tex;
        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", smoothness);
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);
        return mat;
    }

    static Material Emissive(Color color, float emission, float smoothness, float metallic)
    {
        Material mat = Standard(null, color, smoothness, metallic);
        mat.EnableKeyword("_EMISSION");
        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", color * emission);
        return mat;
    }

    static Material Glass()
    {
        Material mat = Standard(null, new Color(0.55f, 0.7f, 0.75f, 0.28f), 0.9f, 0f);
        mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        return mat;
    }

    static Material Blood()
    {
        Material mat = Standard(null, new Color(0.25f, 0.02f, 0.02f, 0.82f), 0.15f, 0f);
        mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        return mat;
    }

    static Texture2D WallTex(int size)
    {
        Texture2D tex = NewTex(size);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)size;
                float v = y / (float)size;
                float n = Fbm(u * 6f, v * 6f, 2);
                Color color = Color.Lerp(new Color(0.62f, 0.6f, 0.55f), new Color(0.78f, 0.76f, 0.7f), n);
                if (v < 0.1f)
                    color = Color.Lerp(new Color(0.28f, 0.32f, 0.3f), color, v / 0.1f);
                float streak = Mathf.PerlinNoise(u * 2.5f, v * 14f);
                if (streak > 0.78f)
                    color *= 0.82f;
                if (Fbm(u * 18f, v * 18f, 8) > 0.72f)
                    color = Color.Lerp(color, new Color(0.32f, 0.36f, 0.28f), 0.45f);
                pixels[y * size + x] = color;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D FloorTex(int size)
    {
        Texture2D tex = NewTex(size);
        Color[] pixels = new Color[size * size];
        int tile = size / 4;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int tx = x / tile;
                int ty = y / tile;
                float lx = (x % tile) / (float)tile;
                float ly = (y % tile) / (float)tile;
                bool grout = lx < 0.045f || ly < 0.045f || lx > 0.955f || ly > 0.955f;
                float variation = Hash01(tx, ty, 3);
                Color tileColor = Color.Lerp(new Color(0.42f, 0.45f, 0.44f), new Color(0.55f, 0.57f, 0.55f), variation);
                tileColor *= 0.9f + Fbm(x * 0.05f, y * 0.05f, 4) * 0.2f;
                if (Hash01(tx, ty, 9) > 0.82f)
                    tileColor = Color.Lerp(tileColor, new Color(0.22f, 0.2f, 0.18f), 0.55f);
                Color color = grout ? new Color(0.12f, 0.12f, 0.12f) : tileColor;
                pixels[y * size + x] = color;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D CeilingTex(int size)
    {
        Texture2D tex = NewTex(size);
        Color[] pixels = new Color[size * size];
        int tile = size / 4;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float lx = (x % tile) / (float)tile;
                float ly = (y % tile) / (float)tile;
                bool seam = lx < 0.03f || ly < 0.03f;
                Color color = seam
                    ? new Color(0.45f, 0.44f, 0.4f)
                    : Color.Lerp(new Color(0.75f, 0.74f, 0.68f), new Color(0.62f, 0.58f, 0.48f), Fbm(x * 0.04f, y * 0.04f, 5));
                pixels[y * size + x] = color;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D DoorTex(int size)
    {
        Texture2D tex = NewTex(size);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)size;
                float v = y / (float)size;
                bool frame = u < 0.08f || u > 0.92f || v < 0.06f || v > 0.94f;
                bool panel = (u > 0.18f && u < 0.82f) && ((v > 0.12f && v < 0.45f) || (v > 0.55f && v < 0.9f));
                Color color = new Color(0.5f, 0.62f, 0.66f);
                if (frame)
                    color *= 0.75f;
                if (panel)
                    color *= 0.88f;
                color *= 0.94f + Hash01(x / 8, y / 8, 1) * 0.08f;
                pixels[y * size + x] = color;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D WoodTex(int size)
    {
        return NoiseTex(size, new Color(0.45f, 0.32f, 0.2f), new Color(0.28f, 0.18f, 0.1f), 11);
    }

    static Texture2D NoiseTex(int size, Color a, Color b, int seed)
    {
        Texture2D tex = NewTex(size);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = Fbm(x / (float)size * 5f, y / (float)size * 5f, seed);
                pixels[y * size + x] = Color.Lerp(a, b, n);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    static Texture2D NewTex(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        tex.anisoLevel = 4;
        return tex;
    }

    static float Fbm(float x, float y, int seed)
    {
        float value = 0f;
        float amplitude = 0.5f;
        float frequency = 1f;
        for (int i = 0; i < 4; i++)
        {
            value += Smooth(x * frequency, y * frequency, seed + i * 17) * amplitude;
            frequency *= 2f;
            amplitude *= 0.5f;
        }
        return value;
    }

    static float Smooth(float x, float y, int seed)
    {
        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);
        float tx = x - x0;
        float ty = y - y0;
        tx = tx * tx * (3f - 2f * tx);
        ty = ty * ty * (3f - 2f * ty);
        float a = Hash01(x0, y0, seed);
        float b = Hash01(x0 + 1, y0, seed);
        float c = Hash01(x0, y0 + 1, seed);
        float d = Hash01(x0 + 1, y0 + 1, seed);
        return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, ty), ty);
    }

    static float Hash01(int x, int y, int seed)
    {
        int n = x * 374761393 + y * 668265263 + seed * 1442695041;
        n = (n ^ (n >> 13)) * 1274126177;
        return ((n ^ (n >> 16)) & 0x7fffffff) / (float)int.MaxValue;
    }

    static void EnsureFolder()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;
        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Art"))
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Art");
        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Art/Generated"))
            UnityEditor.AssetDatabase.CreateFolder("Assets/Art", "Generated");
#endif
    }

    static Texture2D SaveTex(Texture2D tex, string file)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            string path = "Assets/Art/Generated/" + file + ".asset";
            UnityEditor.AssetDatabase.DeleteAsset(path);
            UnityEditor.AssetDatabase.CreateAsset(tex, path);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
#endif
        return tex;
    }

    static Material SaveMat(Material mat, string file)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            string path = "Assets/Art/Generated/" + file + ".mat";
            UnityEditor.AssetDatabase.DeleteAsset(path);
            UnityEditor.AssetDatabase.CreateAsset(mat, path);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
        }
#endif
        return mat;
    }
}
