using UnityEngine;

/// <summary>
/// 运行时合成的音效，避免依赖外部音频文件。
/// </summary>
public static class ProceduralAudio
{
    const int Rate = 22050;

    public static AudioClip Drone()
    {
        int samples = Rate * 4;
        return Make("drone", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float a = Mathf.Sin(2f * Mathf.PI * 55f * t);
            float b = Mathf.Sin(2f * Mathf.PI * 82.4f * t);
            float c = Mathf.Sin(2f * Mathf.PI * 110f * t + Mathf.Sin(t * 0.7f));
            float n = Hash(i) * 0.08f;
            return (a * 0.35f + b * 0.18f + c * 0.12f + n) * 0.35f;
        });
    }

    public static AudioClip Heartbeat()
    {
        int samples = (int)(Rate * 1.05f);
        return Make("heart", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float beat = Thump(t, 0.02f) + Thump(t, 0.22f) * 0.65f;
            return beat * 0.9f;
        });
    }

    public static AudioClip Scrape()
    {
        int samples = Rate;
        return Make("scrape", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float n = Hash(i * 3 + 11);
            float slow = Mathf.Sin(2f * Mathf.PI * 3.5f * t);
            return n * slow * 0.55f;
        });
    }

    public static AudioClip Moan()
    {
        int samples = (int)(Rate * 1.3f);
        return Make("moan", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float env = Mathf.Sin(Mathf.Clamp01(t / 1.3f) * Mathf.PI);
            float freq = Mathf.Lerp(90f, 140f, t / 1.3f) + Mathf.Sin(t * 18f) * 8f;
            return Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.55f;
        });
    }

    public static AudioClip Key()
    {
        int samples = (int)(Rate * 0.35f);
        return Make("key", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float env = Mathf.Exp(-t * 8f);
            float tone = Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.5f
                + Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.25f;
            return tone * env;
        });
    }

    public static AudioClip Locked()
    {
        int samples = (int)(Rate * 0.22f);
        return Make("locked", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float env = Mathf.Exp(-t * 14f);
            return Mathf.Sin(2f * Mathf.PI * 140f * t) * env * 0.7f;
        });
    }

    public static AudioClip Door()
    {
        int samples = (int)(Rate * 0.45f);
        return Make("door", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float env = Mathf.Exp(-t * 5f);
            return (Hash(i) * 0.6f + Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.3f) * env;
        });
    }

    public static AudioClip Footstep()
    {
        int samples = (int)(Rate * 0.12f);
        return Make("step", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float env = Mathf.Exp(-t * 40f);
            return Hash(i + 9) * env * 0.8f;
        });
    }

    public static AudioClip Pickup()
    {
        int samples = (int)(Rate * 0.18f);
        return Make("pickup", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float env = Mathf.Exp(-t * 10f);
            return Mathf.Sin(2f * Mathf.PI * 620f * t) * env * 0.45f;
        });
    }

    public static AudioClip Death()
    {
        int samples = (int)(Rate * 0.8f);
        return Make("death", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float env = Mathf.Exp(-t * 2.5f);
            float tone = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(200f, 40f, t / 0.8f) * t);
            return (tone * 0.7f + Hash(i) * 0.3f) * env;
        });
    }

    public static AudioClip Win()
    {
        int samples = (int)(Rate * 0.9f);
        return Make("win", samples, delegate (int i)
        {
            float t = i / (float)Rate;
            float env = Mathf.Sin(Mathf.Clamp01(t / 0.9f) * Mathf.PI);
            float tone = Mathf.Sin(2f * Mathf.PI * 523f * t) * 0.3f
                + Mathf.Sin(2f * Mathf.PI * 659f * t) * 0.25f
                + Mathf.Sin(2f * Mathf.PI * 784f * t) * 0.2f;
            return tone * env * 0.6f;
        });
    }

    static float Thump(float time, float center)
    {
        float d = time - center;
        if (d < 0f || d > 0.18f)
            return 0f;
        return Mathf.Sin(2f * Mathf.PI * 48f * d) * Mathf.Exp(-d * 22f);
    }

    static float Hash(int n)
    {
        n = (n << 13) ^ n;
        int value = (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
        return (value / (float)int.MaxValue) * 2f - 1f;
    }

    static AudioClip Make(string clipName, int samples, System.Func<int, float> sample)
    {
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
            data[i] = Mathf.Clamp(sample(i), -1f, 1f);
        AudioClip clip = AudioClip.Create(clipName, samples, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
