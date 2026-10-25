using UnityEngine;

/// <summary>
/// 环境底噪、心跳和一次性音效。
/// </summary>
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    public AudioClip droneClip;
    public AudioClip heartbeatClip;
    public AudioClip scrapeClip;
    public AudioClip moanClip;
    public AudioClip keyClip;
    public AudioClip lockedClip;
    public AudioClip doorClip;
    public AudioClip footstepClip;
    public AudioClip pickupClip;
    public AudioClip deathClip;
    public AudioClip winClip;

    AudioSource ambient;
    AudioSource heart;
    AudioSource sfx;
    float heartTarget;

    public static GameAudio CreateConfigured(Transform parent)
    {
        GameObject go = new GameObject("GameAudio");
        go.transform.SetParent(parent, false);
        GameAudio audio = go.AddComponent<GameAudio>();
        audio.AssignClips();
        return audio;
    }

    public void AssignClips()
    {
        if (droneClip != null)
            return;
        droneClip = ProceduralAudio.Drone();
        heartbeatClip = ProceduralAudio.Heartbeat();
        scrapeClip = ProceduralAudio.Scrape();
        moanClip = ProceduralAudio.Moan();
        keyClip = ProceduralAudio.Key();
        lockedClip = ProceduralAudio.Locked();
        doorClip = ProceduralAudio.Door();
        footstepClip = ProceduralAudio.Footstep();
        pickupClip = ProceduralAudio.Pickup();
        deathClip = ProceduralAudio.Death();
        winClip = ProceduralAudio.Win();
    }

    void Awake()
    {
        Instance = this;
        AssignClips();
        ambient = gameObject.AddComponent<AudioSource>();
        heart = gameObject.AddComponent<AudioSource>();
        sfx = gameObject.AddComponent<AudioSource>();
        ambient.loop = true;
        ambient.spatialBlend = 0f;
        ambient.volume = 0.22f;
        ambient.clip = droneClip;
        ambient.Play();

        heart.loop = true;
        heart.spatialBlend = 0f;
        heart.volume = 0f;
        heart.clip = heartbeatClip;
        heart.Play();

        sfx.spatialBlend = 0f;
        sfx.playOnAwake = false;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        heart.volume = Mathf.MoveTowards(heart.volume, heartTarget, Time.deltaTime * 1.5f);
    }

    public void SetHeartbeat(float amount)
    {
        heartTarget = Mathf.Clamp01(amount);
    }

    public void PlayKey()
    {
        sfx.PlayOneShot(keyClip, 0.7f);
    }

    public void PlayLocked()
    {
        sfx.PlayOneShot(lockedClip, 0.65f);
    }

    public void PlayDoor()
    {
        sfx.PlayOneShot(doorClip, 0.55f);
    }

    public void PlayFootstep()
    {
        sfx.PlayOneShot(footstepClip, 0.28f);
    }

    public void PlayPickup()
    {
        sfx.PlayOneShot(pickupClip, 0.5f);
    }

    public void PlayDeath()
    {
        sfx.PlayOneShot(deathClip, 0.8f);
    }

    public void PlayWin()
    {
        sfx.PlayOneShot(winClip, 0.7f);
    }
}
