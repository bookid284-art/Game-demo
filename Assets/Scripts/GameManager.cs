using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 流程、目标文案、胜负与暂停。阅读笔记时世界继续走，玩家不能动。
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public enum RunState
    {
        Intro,
        Playing,
        Reading,
        Paused,
        Dead,
        Won
    }

    public static GameManager Instance { get; private set; }

    public PlayerInventory inventory;
    public BasicFirstPersonController player;
    public MonsterWatcherAI monster;
    public bool nurseDoorOpen;
    public bool wardDoorOpen;
    public bool gateOpen;

    RunState state = RunState.Intro;
    float introTimer = 1.6f;
    string lastObjective = "";

    public RunState State
    {
        get { return state; }
    }

    public bool ConsumeInteractThisFrame { get; private set; }

    public bool BlocksPlayer
    {
        get { return state != RunState.Playing; }
    }

    public bool WorldRunning
    {
        get { return state == RunState.Intro || state == RunState.Playing || state == RunState.Reading; }
    }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        LockCursor();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        ConsumeInteractThisFrame = false;
        if (state == RunState.Intro)
        {
            introTimer -= Time.deltaTime;
            if (introTimer <= 0f)
                state = RunState.Playing;
        }

        if (state == RunState.Reading && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0)))
        {
            EndReading();
            ConsumeInteractThisFrame = true;
            return;
        }

        if (state == RunState.Dead || state == RunState.Won)
        {
            if (Input.GetKeyDown(KeyCode.R))
                Restart();
            return;
        }

        if (state == RunState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            state = RunState.Paused;
            Time.timeScale = 0f;
            UnlockCursor();
            if (GameHUD.Instance != null)
                GameHUD.Instance.ShowPause(true);
            return;
        }

        if (state == RunState.Paused && Input.GetKeyDown(KeyCode.Escape))
            Resume();

        if (state == RunState.Playing && Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            LockCursor();

        if (state == RunState.Playing && gateOpen && player != null && player.transform.position.z > 76.5f)
            Win();

        string objective = ObjectiveText();
        if (objective != lastObjective)
        {
            lastObjective = objective;
            if (GameHUD.Instance != null && state == RunState.Playing)
                GameHUD.Instance.Toast(objective);
        }

        if (GameAudio.Instance != null && monster != null)
        {
            float distance = monster.DistanceToPlayer;
            float heart = 0f;
            if (monster.IsHunting && !monster.IsFrozen && distance < 12f)
                heart = Mathf.Clamp01(1f - distance / 12f);
            GameAudio.Instance.SetHeartbeat(heart);
        }
    }

    public void BeginReading()
    {
        if (state == RunState.Dead || state == RunState.Won)
            return;
        state = RunState.Reading;
        UnlockCursor();
    }

    public void EndReading()
    {
        if (state != RunState.Reading)
            return;
        state = RunState.Playing;
        if (GameHUD.Instance != null)
            GameHUD.Instance.HideNote();
        LockCursor();
    }

    public void Resume()
    {
        if (state != RunState.Paused)
            return;
        state = RunState.Playing;
        Time.timeScale = 1f;
        if (GameHUD.Instance != null)
            GameHUD.Instance.ShowPause(false);
        LockCursor();
    }

    public void NotifyKeyCollected(string keyId)
    {
        if (keyId == "exit" && monster != null)
            monster.Enrage();
    }

    public void NotifyDoorOpened(string doorId)
    {
        if (doorId == "nurse")
            nurseDoorOpen = true;
        else if (doorId == "ward")
            wardDoorOpen = true;
        else if (doorId == "gate")
            gateOpen = true;
    }

    public void Kill()
    {
        if (state == RunState.Dead || state == RunState.Won)
            return;
        state = RunState.Dead;
        Time.timeScale = 0f;
        UnlockCursor();
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.HideNote();
            GameHUD.Instance.ShowDeath();
        }
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayDeath();
    }

    public void Win()
    {
        if (state == RunState.Dead || state == RunState.Won)
            return;
        state = RunState.Won;
        Time.timeScale = 0f;
        UnlockCursor();
        if (GameHUD.Instance != null)
            GameHUD.Instance.ShowWin();
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayWin();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.name);
    }

    public string ObjectiveText()
    {
        if (state == RunState.Won)
            return "你离开了医院";
        bool hasStorage = inventory != null && inventory.HasKey("storage");
        bool hasExam = inventory != null && inventory.HasKey("exam");
        bool hasExit = inventory != null && inventory.HasKey("exit");
        if (!hasStorage && !nurseDoorOpen)
            return "在西侧库房找到库房钥匙";
        if (!nurseDoorOpen)
            return "用库房钥匙打开大厅北门";
        if (!hasExam && !wardDoorOpen)
            return "在东侧诊室找到诊室钥匙";
        if (!wardDoorOpen)
            return "用诊室钥匙打开病房区的门";
        if (!hasExit && !gateOpen)
            return "在 307 病房找到出口钥匙。看着它，它才会停下";
        if (!gateOpen)
            return "打开走廊尽头的紧急出口";
        return "穿过出口，离开医院";
    }

    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
