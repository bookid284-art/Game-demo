using UnityEngine;

/// <summary>
/// WASD 移动、鼠标视角、冲刺体力。镜头俯仰挂在子物体上。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class BasicFirstPersonController : MonoBehaviour
{
    public Transform cameraHolder;
    public float walkSpeed = 3.6f;
    public float sprintSpeed = 6.8f;
    public float mouseSensitivity = 3.2f;
    public float eyeHeight = 1.64f;
    public float regenMultiplier = 1f;

    CharacterController controller;
    Vector2 smoothedInput;
    float yaw;
    float pitch;
    float verticalVelocity;
    float stamina = 1f;
    float regenDelay;
    bool sprintLocked;
    float bobPhase;
    Vector3 cameraBaseLocal;

    public float Stamina01
    {
        get { return Mathf.Clamp01(stamina); }
    }

    public bool IsSprinting { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraHolder != null)
            cameraBaseLocal = cameraHolder.localPosition;
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
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
        Move(blocked);
        Bob();
        Footsteps();
    }

    void Look()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -84f, 84f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f);
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

    void Bob()
    {
        if (cameraHolder == null)
            return;
        Vector3 planar = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float speed = planar.magnitude;
        if (controller.isGrounded && speed > 0.4f)
        {
            bobPhase += speed * Time.deltaTime * 1.7f;
            float bob = Mathf.Sin(bobPhase) * (IsSprinting ? 0.045f : 0.028f);
            cameraHolder.localPosition = cameraBaseLocal + new Vector3(0f, bob, 0f);
        }
        else
        {
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, cameraBaseLocal, 8f * Time.deltaTime);
        }
    }

    void Footsteps()
    {
        if (GameAudio.Instance == null)
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

    float stepTimer;
}
