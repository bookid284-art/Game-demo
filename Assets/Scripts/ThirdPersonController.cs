using UnityEngine;

/// <summary>
/// 过肩第三人称。身体跟着鼠标水平转向，镜头在右肩后方，贴墙时收近。
/// 行走、冲刺和体力与原来的第一人称相同，关卡路程不会变长。
/// </summary>
[RequireComponent(typeof(CharacterController))]
[DefaultExecutionOrder(-50)]
public class ThirdPersonController : MonoBehaviour
{
    public Transform cameraPivot;
    public Transform cameraHolder;
    public Transform spine;
    public Transform head;
    public Transform armL;
    public Transform armR;
    public Transform legL;
    public Transform legR;
    public Transform hand;
    public Transform flashlightMount;

    public float walkSpeed = 3.6f;
    public float sprintSpeed = 6.8f;
    public float mouseSensitivity = 3.2f;
    public float regenMultiplier = 1f;

    public float shoulderDistance = 2.2f;
    public float minShoulderDistance = 0.62f;

    CharacterController controller;
    Camera view;
    AudioSource breath;
    Vector2 smoothedInput;
    Vector3 pivotBase;
    float yaw;
    float pitch;
    float verticalVelocity;
    float stamina = 1f;
    float regenDelay;
    bool sprintLocked;
    float stepTimer;
    float currentDistance = 3.05f;
    float fear;
    float recoil;
    bool sawFrozen;
    bool pivotReady;
    MonsterWatcherAI monster;

    static readonly RaycastHit[] CameraHits = new RaycastHit[32];
    static readonly Collider[] CameraOverlaps = new Collider[16];
    const float CameraRadius = 0.16f;

    public float Stamina01
    {
        get { return Mathf.Clamp01(stamina); }
    }

    public bool IsSprinting { get; private set; }

    public Vector3 LookDirection
    {
        get
        {
            if (cameraPivot != null)
                return cameraPivot.forward;
            return transform.forward;
        }
    }

    public Vector3 EyePosition
    {
        get { return transform.position + Vector3.up * 1.52f + transform.forward * 0.36f; }
    }

    public float Fear01
    {
        get { return fear; }
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        CapturePivot();
        if (cameraHolder != null)
            view = cameraHolder.GetComponent<Camera>();
        yaw = transform.eulerAngles.y;

        GameObject breathObject = new GameObject("Breath");
        breathObject.transform.SetParent(transform, false);
        breathObject.transform.localPosition = new Vector3(0f, 1.45f, 0.08f);
        breath = breathObject.AddComponent<AudioSource>();
        breath.clip = ProceduralAudio.Breath();
        breath.loop = true;
        breath.spatialBlend = 1f;
        breath.dopplerLevel = 0f;
        breath.minDistance = 0.45f;
        breath.maxDistance = 5.5f;
        breath.rolloffMode = AudioRolloffMode.Linear;
        breath.volume = 0.03f;
        breath.playOnAwake = false;
        if (Application.isPlaying)
            breath.Play();
    }

    void CapturePivot()
    {
        if (pivotReady || cameraPivot == null)
            return;
        pivotBase = cameraPivot.localPosition;
        pivotReady = true;
    }

    void Start()
    {
        if (GameManager.Instance != null)
            monster = GameManager.Instance.monster;
    }

    void Update()
    {
        if (monster == null && GameManager.Instance != null)
            monster = GameManager.Instance.monster;

        bool dead = GameManager.Instance != null && GameManager.Instance.State == GameManager.RunState.Dead;
        bool blocked = GameManager.Instance != null && GameManager.Instance.BlocksPlayer;
        if (!blocked)
            Look();

        Vector2 wish = Vector2.zero;
        if (!blocked)
        {
            wish.x = Input.GetAxisRaw("Horizontal");
            wish.y = Input.GetAxisRaw("Vertical");
            if (wish.sqrMagnitude > 1f)
                wish.Normalize();
        }

        smoothedInput = Vector2.MoveTowards(smoothedInput, wish, 10f * Time.deltaTime);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        UpdateStamina(shift && smoothedInput.sqrMagnitude > 0.04f && !blocked);
        if (!dead)
            Move(blocked);
        UpdateFear();
        AnimateBody();
        AimFlashlight();
        PlaceCamera(dead);
        if (!dead)
            Footsteps();
        UpdateBreath();
    }

    void Look()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -42f, 48f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void UpdateStamina(bool wantSprint)
    {
        if (stamina <= 0.01f)
            sprintLocked = true;
        if (stamina >= 0.28f)
            sprintLocked = false;

        IsSprinting = wantSprint && !sprintLocked;
        if (IsSprinting)
        {
            stamina -= 0.23f * Time.deltaTime;
            regenDelay = 0.8f;
        }
        else
        {
            regenDelay -= Time.deltaTime;
            if (regenDelay <= 0f)
                stamina = Mathf.MoveTowards(stamina, 1f, 0.4f * regenMultiplier * Time.deltaTime);
        }

        stamina = Mathf.Clamp01(stamina);
    }

    void Move(bool blocked)
    {
        bool grounded = controller.isGrounded;
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2.2f;
        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        float speed = IsSprinting ? sprintSpeed : walkSpeed;
        Vector3 planar = transform.right * smoothedInput.x + transform.forward * smoothedInput.y;
        if (blocked)
            planar = Vector3.zero;
        Vector3 motion = planar * speed;
        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);
    }

    void UpdateFear()
    {
        float target = 0f;
        bool frozen = false;
        if (monster != null && monster.IsHunting)
        {
            float near = Mathf.Clamp01(1f - monster.DistanceToPlayer / 14f);
            frozen = monster.IsFrozen;
            target = frozen ? near * 0.45f : near;
            if (!frozen)
            {
                Vector3 flat = monster.transform.position - transform.position;
                flat.y = 0f;
                if (flat.sqrMagnitude > 0.04f && Vector3.Dot(transform.forward, flat.normalized) < 0.15f)
                    target = Mathf.Clamp01(target + 0.35f);
            }
        }

        fear = Mathf.MoveTowards(fear, target, Time.deltaTime * 1.8f);
        if (sawFrozen && !frozen && monster != null && monster.DistanceToPlayer < 16f)
            recoil = 0.16f;
        sawFrozen = frozen && monster != null && monster.DistanceToPlayer < 18f;
        float recoilDt = Time.deltaTime > 0f ? Time.deltaTime : 0f;
        recoil = Mathf.MoveTowards(recoil, 0f, recoilDt * 0.55f);
    }

    void AnimateBody()
    {
        Vector3 planar = controller == null
            ? Vector3.zero
            : new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float speed = planar.magnitude;
        bool moving = controller != null && controller.isGrounded && speed > 0.35f;
        float phase = Time.time * (IsSprinting ? 9.2f : 6.1f);
        float swing = moving ? Mathf.Sin(phase) : 0f;
        float arm = swing * (IsSprinting ? 34f : 24f);
        float leg = swing * (IsSprinting ? 28f : 20f);
        if (armL != null)
            armL.localRotation = Quaternion.Euler(arm, 0f, 7f);
        if (armR != null)
            armR.localRotation = Quaternion.Euler(-arm * 0.65f, 0f, -7f);
        if (legL != null)
            legL.localRotation = Quaternion.Euler(-leg, 0f, 0f);
        if (legR != null)
            legR.localRotation = Quaternion.Euler(leg, 0f, 0f);

        float hunch = Mathf.Lerp(2.5f, 12f, fear);
        if (spine != null)
            spine.localRotation = Quaternion.Euler(hunch, 0f, 0f);
        if (head != null)
        {
            float shiver = fear * Mathf.Sin(Time.time * 17f) * 1.6f;
            head.localRotation = Quaternion.Euler(pitch * 0.82f - hunch, shiver, 0f);
        }
    }

    void AimFlashlight()
    {
        if (flashlightMount == null)
            return;
        Vector3 aim = LookDirection;
        Vector3 origin = hand != null ? hand.position : transform.position + Vector3.up * 1.15f + transform.right * 0.24f;
        float tremble = fear * 0.012f;
        Vector3 shake = new Vector3(Mathf.Sin(Time.time * 19f), Mathf.Sin(Time.time * 27f), 0f) * tremble;
        flashlightMount.position = origin + shake + aim * 0.06f;
        if (aim.sqrMagnitude > 0.001f)
            flashlightMount.rotation = Quaternion.LookRotation(aim, Vector3.up);
    }

    void PlaceCamera(bool dead)
    {
        if (cameraPivot == null || cameraHolder == null)
            return;
        CapturePivot();

        bool intro = GameManager.Instance != null && GameManager.Instance.State == GameManager.RunState.Intro;
        float desired = shoulderDistance;
        if (intro)
            desired = 3.15f;
        else if (dead)
            desired = shoulderDistance + 0.95f;
        else if (monster != null && monster.IsFrozen && fear > 0.05f)
            desired -= 0.28f * fear;
        else
            desired += 0.55f * fear;
        if (IsSprinting)
            desired += 0.08f;

        Vector3 back = -cameraPivot.forward;
        float allowed = ResolveDistance(cameraPivot.position, back, desired);
        float dt = dead ? Time.unscaledDeltaTime : Time.deltaTime;
        float blend = 1f - Mathf.Exp(-8f * Mathf.Max(dt, 0f));
        currentDistance = Mathf.Lerp(currentDistance, allowed, blend);

        float breathe = Mathf.Sin(Time.time * (1.15f + fear)) * (0.01f + fear * 0.012f);
        cameraHolder.localPosition = new Vector3(0f, breathe - recoil, -currentDistance);
        cameraHolder.localRotation = Quaternion.identity;

        if (view != null)
            view.fieldOfView = Mathf.Lerp(view.fieldOfView, 66f, blend);

        float drop = intro ? -0.18f : (monster != null && monster.IsFrozen ? 0.05f * fear : -0.06f * fear);
        cameraPivot.localPosition = pivotBase + new Vector3(0f, drop, 0f);
    }

    float ResolveDistance(Vector3 pivot, Vector3 back, float desired)
    {
        float allowed = desired;
        int count = Physics.SphereCastNonAlloc(
            pivot,
            CameraRadius,
            back,
            CameraHits,
            desired,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        float nearest = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (IsOwnCollider(CameraHits[i].collider))
                continue;
            if (CameraHits[i].distance < nearest)
                nearest = CameraHits[i].distance;
        }
        const float floorDistance = 0.3f;
        if (nearest < float.MaxValue)
            allowed = Mathf.Clamp(nearest - 0.05f, floorDistance, desired);

        for (int step = 0; step < 12; step++)
        {
            if (!PointBlocked(pivot + back * allowed))
                break;
            allowed = Mathf.Max(floorDistance, allowed - 0.08f);
        }
        return allowed;
    }

    bool PointBlocked(Vector3 point)
    {
        int count = Physics.OverlapSphereNonAlloc(
            point,
            CameraRadius,
            CameraOverlaps,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            if (!IsOwnCollider(CameraOverlaps[i]))
                return true;
        }
        return false;
    }

    bool IsOwnCollider(Collider collider)
    {
        return collider != null && collider.GetComponentInParent<ThirdPersonController>() != null;
    }

    void Footsteps()
    {
        if (GameAudio.Instance == null || controller == null)
            return;
        Vector3 planar = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        if (!controller.isGrounded || planar.magnitude < 0.8f)
            return;
        if (GameManager.Instance != null && GameManager.Instance.BlocksPlayer)
            return;
        float interval = IsSprinting ? 0.32f : 0.48f;
        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            stepTimer = interval;
            GameAudio.Instance.PlayFootstep();
        }
    }

    void UpdateBreath()
    {
        if (breath == null)
            return;
        float target = 0.028f + fear * 0.09f;
        if (IsSprinting)
            target += 0.04f;
        if (monster != null && monster.IsFrozen && fear > 0.2f)
            target += 0.03f;
        breath.volume = Mathf.MoveTowards(breath.volume, target, Time.deltaTime * 0.6f);
        breath.pitch = Mathf.Lerp(0.96f, 1.06f, fear);
    }
}
