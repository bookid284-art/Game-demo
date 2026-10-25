using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 八段夜班、支线标记、胜负与暂停。阅读和翻记录时世界继续走。
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public enum RunState
    {
        Intro,
        Playing,
        Reading,
        Journal,
        Code,
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
    public bool outerOpen;
    public bool MonsterReleased { get; private set; }

    readonly HashSet<string> flags = new HashSet<string>();
    readonly Dictionary<string, string> sequence = new Dictionary<string, string>();

    RunState state = RunState.Intro;
    float introTimer = 1.6f;
    string lastObjective = "";
    KeypadLock activePad;
    string codeBuffer = "";

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
        get
        {
            return state == RunState.Intro || state == RunState.Playing || state == RunState.Reading
                || state == RunState.Journal || state == RunState.Code;
        }
    }

    public bool HasFlag(string id)
    {
        return !string.IsNullOrEmpty(id) && flags.Contains(id);
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
        else if (state == RunState.Reading && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0)))
        {
            EndReading();
            ConsumeInteractThisFrame = true;
        }
        else if (state == RunState.Journal && (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
        {
            EndJournal();
            ConsumeInteractThisFrame = true;
        }
        else if (state == RunState.Code)
        {
            HandleCodeInput();
        }
        else if (state == RunState.Dead || state == RunState.Won)
        {
            if (Input.GetKeyDown(KeyCode.R))
                Restart();
        }
        else if (state == RunState.Playing && Input.GetKeyDown(KeyCode.J))
        {
            BeginJournal();
        }
        else if (state == RunState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            state = RunState.Paused;
            Time.timeScale = 0f;
            UnlockCursor();
            if (GameHUD.Instance != null)
                GameHUD.Instance.ShowPause(true);
        }
        else if (state == RunState.Paused && Input.GetKeyDown(KeyCode.Escape))
        {
            Resume();
        }
        else if (state == RunState.Playing && Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }

        if (state == RunState.Playing && outerOpen && player != null && player.transform.position.z > 104f)
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
        if (state == RunState.Dead || state == RunState.Won || state == RunState.Paused)
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

    public void BeginJournal()
    {
        if (state != RunState.Playing)
            return;
        state = RunState.Journal;
        UnlockCursor();
        if (GameHUD.Instance != null)
            GameHUD.Instance.ShowJournal(JournalText());
    }

    public void EndJournal()
    {
        if (state != RunState.Journal)
            return;
        state = RunState.Playing;
        if (GameHUD.Instance != null)
            GameHUD.Instance.HideJournal();
        LockCursor();
    }

    public void BeginCode(KeypadLock pad)
    {
        if (pad == null || state == RunState.Dead || state == RunState.Won || state == RunState.Paused)
            return;
        activePad = pad;
        codeBuffer = "";
        state = RunState.Code;
        LockCursor();
        if (GameHUD.Instance != null)
            GameHUD.Instance.ShowCode(pad.title, "");
    }

    void HandleCodeInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
        {
            CancelCode();
            ConsumeInteractThisFrame = true;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && codeBuffer.Length > 0)
            codeBuffer = codeBuffer.Substring(0, codeBuffer.Length - 1);

        if (codeBuffer.Length < 4)
        {
            for (int digit = 0; digit <= 9; digit++)
            {
                KeyCode alpha = (KeyCode)((int)KeyCode.Alpha0 + digit);
                KeyCode keypad = (KeyCode)((int)KeyCode.Keypad0 + digit);
                if (Input.GetKeyDown(alpha) || Input.GetKeyDown(keypad))
                    codeBuffer += digit.ToString();
            }
        }

        if (GameHUD.Instance != null && activePad != null)
            GameHUD.Instance.ShowCode(activePad.title, codeBuffer);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            SubmitCode();
    }

    void SubmitCode()
    {
        if (activePad == null)
        {
            CancelCode();
            return;
        }

        if (codeBuffer == activePad.code)
        {
            KeypadLock pad = activePad;
            EndCode();
            pad.Succeed();
            return;
        }

        codeBuffer = "";
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast("密码不对");
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayLocked();
    }

    void CancelCode()
    {
        EndCode();
    }

    void EndCode()
    {
        if (state != RunState.Code)
            return;
        activePad = null;
        codeBuffer = "";
        state = RunState.Playing;
        if (GameHUD.Instance != null)
            GameHUD.Instance.HideCode();
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

    public void SetFlag(string id)
    {
        if (string.IsNullOrEmpty(id) || flags.Contains(id))
            return;
        flags.Add(id);
        if (CountPrefix("linxia") >= 4 && !flags.Contains("linxia_done"))
        {
            flags.Add("linxia_done");
            GiveBattery(1f);
            if (GameHUD.Instance != null)
                GameHUD.Instance.Toast("林夏的备用电池充上了");
        }
        if (CountPrefix("page") >= 5 && !flags.Contains("pages_done"))
        {
            flags.Add("pages_done");
            GiveBattery(1f);
            if (GameHUD.Instance != null)
                GameHUD.Instance.Toast("五张交接残页凑齐了");
        }
    }

    public void SetStamp(string id)
    {
        if (HasFlag(id))
            return;
        SetFlag(id);
        int count = StampCount();
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast("盖章 " + count + "/4");
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayKey();
        HoldObjectiveToast();
    }

    public void HoldObjectiveToast()
    {
        lastObjective = ObjectiveText();
    }

    public void PushToken(string group, string token)
    {
        if (group == "valve")
            PushSequence(group, token, "白,绿,红,蓝,", "valves");
    }

    void PushSequence(string group, string token, string expected, string doneFlag)
    {
        if (HasFlag(doneFlag))
            return;
        string soFar;
        if (!sequence.TryGetValue(group, out soFar))
            soFar = "";
        string next = soFar + token + ",";
        if (expected.StartsWith(next))
        {
            sequence[group] = next;
            if (next == expected)
            {
                SetFlag(doneFlag);
                if (GameHUD.Instance != null)
                    GameHUD.Instance.Toast("阀门归位，手术终端通电了");
                if (GameAudio.Instance != null)
                    GameAudio.Instance.PlayDoor();
            }
            else if (GameHUD.Instance != null)
            {
                GameHUD.Instance.Toast(token + "阀咬住了");
            }
            return;
        }

        sequence[group] = "";
        if (GameHUD.Instance != null)
            GameHUD.Instance.Toast("顺序错了，阀门弹回");
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayLocked();
    }

    public void ReleaseMonster()
    {
        MonsterReleased = true;
        if (monster != null)
            monster.Enrage();
    }

    public void GiveBattery(float amount)
    {
        if (player == null)
            return;
        FlashlightController flashlight = player.GetComponent<FlashlightController>();
        if (flashlight != null)
            flashlight.AddBattery(amount);
    }

    public void RewardDrain()
    {
        if (player == null)
            return;
        FlashlightController flashlight = player.GetComponent<FlashlightController>();
        if (flashlight != null)
            flashlight.drainMultiplier = 0.65f;
    }

    public void RewardStamina()
    {
        if (player != null)
            player.regenMultiplier = 1.45f;
    }

    public void NotifyKeyCollected(string keyId)
    {
        if (keyId == "quiet")
            SetFlag("morgue_list");
    }

    public void NotifyDoorOpened(string doorId)
    {
        if (doorId == "nurse")
            nurseDoorOpen = true;
        else if (doorId == "ward")
            wardDoorOpen = true;
        else if (doorId == "gate")
            gateOpen = true;
        else if (doorId == "outer")
            outerOpen = true;
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
            GameHUD.Instance.HideOverlays();
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
        {
            GameHUD.Instance.HideOverlays();
            GameHUD.Instance.ShowWin(EndingTitle(), EndingBody());
        }
        if (GameAudio.Instance != null)
            GameAudio.Instance.PlayWin();
    }

    public bool TrueEnding()
    {
        return HasFlag("linxia_done") && HasFlag("pharmacy_done") && HasFlag("surgery_done")
            && HasFlag("morgue_list") && HasFlag("quiet_read") && HasFlag("pages_done");
    }

    public string EndingTitle()
    {
        return TrueEnding() ? "你把夜班带走了" : "你逃出了医院";
    }

    public string EndingBody()
    {
        if (TrueEnding())
            return "林夏的记录、名册和安静室里的名字都在你身上。天亮以后，不会再有人说这只是一次夜班。";
        return "疏散单还在口袋里。它停在围栏后面，没有跟出来。六条支线都做完时，离开的方式会不一样。";
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
            return TrueEnding() ? "夜班的记录被你带走了" : "你离开了医院";
        if (!HasFlag("stamp_er"))
            return "一、东侧急诊盖章。药房密码拆在四间记录里";
        if (!HasFlag("orders_read"))
            return "二、西侧行政。门禁拆在休息室和会议室，《零号观察》在档案室";
        if (!HasFlag("stamp_ward"))
        {
            bool hasStorage = inventory != null && inventory.HasKey("storage");
            bool hasExam = inventory != null && inventory.HasKey("exam");
            if (!nurseDoorOpen && !hasStorage)
                return "三、病房章。先拿库房钥匙，打开大厅北门";
            if (!nurseDoorOpen)
                return "三、用库房钥匙打开大厅北门";
            if (!wardDoorOpen && !hasExam)
                return "三、诊室钥匙在东侧检查床。呼叫铃在 301、305、306";
            if (!wardDoorOpen)
                return "三、用诊室钥匙打开病房区";
            return "三、点亮 301、305、306 的呼叫铃，回护士站盖章";
        }
        if (!gateOpen)
        {
            bool hasExit = inventory != null && inventory.HasKey("exit");
            if (!hasExit)
                return "四、307 床侧有装卸区钥匙。闸门还不是出口";
            return "四、打开病房尽头的装卸闸门";
        }
        if (!HasFlag("stamp_or"))
            return "五、装卸区东侧手术室。按《零号观察》扳阀门再盖章";
        if (!HasFlag("stamp_psych"))
            return "六、装卸区西侧精神科。终端密码在《零号观察》";
        if (!HasFlag("auth"))
            return "七、回行政档案室打印疏散单。它会离开病房";
        if (!outerOpen)
            return "八、用疏散单打开北侧围栏";
        return "九、跑过围栏，离开医院";
    }

    public string JournalText()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("主线\n");
        builder.Append(Mark(HasFlag("stamp_er"))).Append(" 急诊章\n");
        builder.Append(Mark(HasFlag("orders_read"))).Append(" 《零号观察》\n");
        builder.Append(Mark(HasFlag("stamp_ward"))).Append(" 病房章\n");
        builder.Append(Mark(gateOpen)).Append(" 装卸闸门\n");
        builder.Append(Mark(HasFlag("stamp_or"))).Append(" 手术章\n");
        builder.Append(Mark(HasFlag("stamp_psych"))).Append(" 精神科章\n");
        builder.Append(Mark(HasFlag("auth"))).Append(" 疏散单\n");
        builder.Append(Mark(outerOpen)).Append(" 北围栏\n\n");
        builder.Append("支线    按 J 关闭，怪物仍在走\n");
        builder.Append(Mark(HasFlag("linxia_done"))).Append(" 林夏的夜班  ").Append(CountPrefix("linxia")).Append("/4  备用电池\n");
        builder.Append(Mark(HasFlag("pharmacy_done"))).Append(" 药房盘点  ").Append(CountPrefix("vial")).Append("/4  手电更耐用\n");
        builder.Append(Mark(HasFlag("surgery_done"))).Append(" 未完成的手术  ").Append(CountPrefix("bag")).Append("/3  体力恢复变快\n");
        builder.Append(Mark(HasFlag("morgue_list"))).Append(" 太平间名册  安静室钥匙\n");
        builder.Append(Mark(HasFlag("pages_done"))).Append(" 交接残页  ").Append(CountPrefix("page")).Append("/5  一整块电池\n");
        builder.Append(Mark(HasFlag("quiet_read"))).Append(" 安静室  另一段结局\n");
        return builder.ToString();
    }

    static string Mark(bool done)
    {
        return done ? "[x]" : "[ ]";
    }

    int CountPrefix(string prefix)
    {
        int count = 0;
        foreach (string flag in flags)
        {
            if (!flag.StartsWith(prefix) || flag.EndsWith("_done") || flag.EndsWith("_read"))
                continue;
            count++;
        }
        return count;
    }

    int StampCount()
    {
        int count = 0;
        if (HasFlag("stamp_er"))
            count++;
        if (HasFlag("stamp_ward"))
            count++;
        if (HasFlag("stamp_or"))
            count++;
        if (HasFlag("stamp_psych"))
            count++;
        return count;
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
