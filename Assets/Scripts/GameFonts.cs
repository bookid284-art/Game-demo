using UnityEngine;

/// <summary>
/// 优先使用工程内嵌的文泉驿子集，其次尝试操作系统里的中文字体。
/// </summary>
public static class GameFonts
{
    static Font cached;

    public static Font Get()
    {
        if (cached != null)
            return cached;

        cached = Resources.Load<Font>("Fonts/GameCJK");
        if (cached != null)
            return cached;

        string[] osFonts =
        {
            "WenQuanYi Micro Hei",
            "Noto Sans CJK SC",
            "Noto Sans SC",
            "Source Han Sans SC",
            "Microsoft YaHei",
            "PingFang SC",
            "Heiti SC",
            "Droid Sans Fallback",
            "Arial Unicode MS"
        };

        try
        {
            cached = Font.CreateDynamicFontFromOSFont(osFonts, 32);
        }
        catch
        {
            cached = null;
        }

        if (cached == null)
            cached = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return cached;
    }
}
