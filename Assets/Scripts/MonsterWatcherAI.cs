using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 病人零号。出现在玩家视野里并且没有被墙挡住时会停住，否则追击。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MonsterWatcherAI : MonoBehaviour
{
    public Transform player;
    public Camera playerCamera;
    public HospitalNavGraph graph;
    public Transform armL;
    public Transform armR;
    public Transform head;
    public Material eyeMaterial;
    public int[] patrolNodes;
    public float patrolSpeed = 1.8f;
    public float huntSpeed = 5.4f;
    public float enragedSpeed = 6.4f;

    CharacterController controller;
    AudioSource voice;
    bool hunting;
    bool enraged;
    bool frozen;
    bool wasFrozen;
    float verticalVelocity;
    float replanTimer;
    float moanTimer;
    int patrolIndex;
    int pathIndex;
    List<int> path = new List<int>();
    static readonly RaycastHit[] Hits = new RaycastHit[32];

    public bool IsFrozen
    {
        get { return frozen; }
    }

    public bool IsHunting
    {
        get { return hunting; }
    }

    public float DistanceToPlayer
    {
        get { return player == null ? 999f : HorizontalDistance(player.position); }
    }

    public void Enrage()
    {
        enraged = true;
        hunting = true;
        replanTimer = 0f;
        if (GameAudio.Instance != null && voice != null)
            voice.PlayOneShot(GameAudio.Instance.moanClip, 0.9f);
    }

    public void BindVoice(AudioSource source)
    {
        voice = source;
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        moanTimer = 4f;
        if (voice != null && GameAudio.Instance != null)
        {
            voice.clip = GameAudio.Instance.scrapeClip;
            voice.loop = true;
            voice.spatialBlend = 1f;
            voice.minDistance = 1.2f;
            voice.maxDistance = 18f;
            voice.rolloffMode = AudioRolloffMode.Linear;
            voice.volume = 0f;
            voice.Play();
        }
    }

    void Update()
    {
        if (player == null)
            return;
        if (GameManager.Instance != null && !GameManager.Instance.WorldRunning)
            return;

        float dt = Time.deltaTime;
        frozen = IsObserved();
        if (wasFrozen && !frozen && DistanceToPlayer < 16f && GameHUD.Instance != null)
            GameHUD.Instance.Toast("它动了");
        wasFrozen = frozen;

        if (!hunting && (player.position.z > 39.2f || DistanceToPlayer < 8f))
        {
            hunting = true;
            replanTimer = 0f;
            if (voice != null && GameAudio.Instance != null)
                voice.PlayOneShot(GameAudio.Instance.moanClip, 0.8f);
        }

        Vector3 target = ChooseTarget();
        Vector3 to = target - transform.position;
        to.y = 0f;
        bool moving = false;
        if (!frozen && to.magnitude > 0.3f)
        {
            Vector3 direction = Steer(to.normalized);
            float speed = hunting ? (enraged ? enragedSpeed : huntSpeed) : patrolSpeed;
            if (hunting && !enraged && DistanceToPlayer < 4.2f)
                speed *= 1.12f;
            Move(direction, speed, dt);
            Face(direction);
            moving = true;
        }
        else
        {
            Settle(dt);
            Vector3 face = player.position - transform.position;
            face.y = 0f;
            if (face.sqrMagnitude > 0.01f)
                Face(face.normalized);
        }

        Animate(moving);
        UpdateVoice(moving);
        UpdateEyes();

        if (hunting && !frozen && DistanceToPlayer < 1.12f && GameManager.Instance != null)
            GameManager.Instance.Kill();

        moanTimer -= dt;
        if (moanTimer <= 0f)
        {
            moanTimer = Random.Range(7f, 13f);
            if (voice != null && GameAudio.Instance != null && DistanceToPlayer < 22f)
                voice.PlayOneShot(GameAudio.Instance.moanClip, hunting ? 0.55f : 0.28f);
        }
    }

    bool HeldAtLoadingGate()
    {
        return GameManager.Instance != null
            && GameManager.Instance.gateOpen
            && !GameManager.Instance.MonsterReleased
            && player.position.z > 73.2f
            && transform.position.z < 72.15f;
    }

    Vector3 ChooseTarget()
    {
        if (player == null)
            return transform.position;

        if (HeldAtLoadingGate())
            return new Vector3(0f, 0f, 70.2f);

        if (!hunting)
            return PatrolPoint();

        if (HasLineOfSightToPlayer() && DistanceToPlayer < 20f)
            return player.position;

        return NextPathPoint();
    }

    Vector3 PatrolPoint()
    {
        if (graph == null || patrolNodes == null || patrolNodes.Length == 0)
            return transform.position;
        if (patrolIndex >= patrolNodes.Length)
            patrolIndex = 0;
        Vector3 point = graph.NodePosition(patrolNodes[patrolIndex]);
        Vector3 flat = point - transform.position;
        flat.y = 0f;
        if (flat.magnitude < 0.55f)
        {
            patrolIndex = (patrolIndex + 1) % patrolNodes.Length;
            point = graph.NodePosition(patrolNodes[patrolIndex]);
        }
        return point;
    }

    Vector3 NextPathPoint()
    {
        if (graph == null)
            return player.position;

        replanTimer -= Time.deltaTime;
        if (path.Count == 0 || replanTimer <= 0f)
        {
            int from = graph.Nearest(transform.position);
            int to = graph.Nearest(player.position);
            path = graph.PathToward(from, to);
            pathIndex = 0;
            replanTimer = 0.4f;
        }

        while (pathIndex < path.Count)
        {
            Vector3 point = graph.NodePosition(path[pathIndex]);
            Vector3 flat = point - transform.position;
            flat.y = 0f;
            if (flat.magnitude < 0.5f)
                pathIndex++;
            else
                return point;
        }

        return player.position;
    }

    bool IsObserved()
    {
        if (playerCamera == null)
            return false;

        Vector3[] points =
        {
            transform.position + Vector3.up * 1.72f,
            transform.position + Vector3.up * 1.25f,
            transform.position + Vector3.up * 0.7f
        };

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 viewport = playerCamera.WorldToViewportPoint(points[i]);
            if (viewport.z < 0.25f)
                continue;
            if (viewport.x < 0.025f || viewport.x > 0.975f || viewport.y < 0.025f || viewport.y > 0.975f)
                continue;
            if (ClearSight(playerCamera.transform.position, points[i]))
                return true;
        }
        return false;
    }

    bool HasLineOfSightToPlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.35f;
        Vector3 target = player.position + Vector3.up * 1.2f;
        return ClearSight(origin, target);
    }

    bool ClearSight(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        float distance = delta.magnitude;
        if (distance < 0.05f)
            return true;

        int count = Physics.RaycastNonAlloc(
            from,
            delta / distance,
            Hits,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            int nearest = i;
            for (int j = i + 1; j < count; j++)
            {
                if (Hits[j].distance < Hits[nearest].distance)
                    nearest = j;
            }
            RaycastHit swap = Hits[i];
            Hits[i] = Hits[nearest];
            Hits[nearest] = swap;
        }

        for (int i = 0; i < count; i++)
        {
            Collider collider = Hits[i].collider;
            if (collider == null)
                continue;
            if (collider.GetComponentInParent<BasicFirstPersonController>() != null)
                continue;
            if (collider.GetComponentInParent<MonsterWatcherAI>() != null)
                return true;
            return false;
        }
        return true;
    }

    Vector3 Steer(Vector3 direction)
    {
        Vector3 origin = transform.position + Vector3.up * 0.7f;
        if (!Blocked(origin, direction, 0.95f))
            return direction;

        Vector3 side = Vector3.Cross(Vector3.up, direction);
        if (!Blocked(origin, side, 0.8f))
            return side.normalized;
        if (!Blocked(origin, -side, 0.8f))
            return (-side).normalized;
        return direction;
    }

    bool Blocked(Vector3 origin, Vector3 direction, float distance)
    {
        RaycastHit hit;
        if (!Physics.Raycast(origin, direction, out hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return false;
        if (hit.collider.GetComponentInParent<BasicFirstPersonController>() != null)
            return false;
        if (hit.collider.GetComponentInParent<MonsterWatcherAI>() != null)
            return false;
        return true;
    }

    void Move(Vector3 direction, float speed, float dt)
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += Physics.gravity.y * dt;
        Vector3 motion = direction * speed;
        motion.y = verticalVelocity;
        if (GameManager.Instance != null && GameManager.Instance.gateOpen && !GameManager.Instance.MonsterReleased
            && transform.position.z > 71.7f && player.position.z > 73f)
            motion = Vector3.zero;
        controller.Move(motion * dt);
    }

    void Settle(float dt)
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += Physics.gravity.y * dt;
        controller.Move(new Vector3(0f, verticalVelocity * dt, 0f));
    }

    void Face(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;
        Quaternion look = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, 6f * Time.deltaTime);
    }

    void Animate(bool moving)
    {
        float swing = moving ? Mathf.Sin(Time.time * (hunting ? 8f : 3.2f)) * (hunting ? 32f : 16f) : 0f;
        if (armL != null)
            armL.localRotation = Quaternion.Euler(swing, 0f, 6f);
        if (armR != null)
            armR.localRotation = Quaternion.Euler(-swing, 0f, -6f);
        if (head != null)
        {
            float tilt = frozen ? Mathf.Sin(Time.time * 1.4f) * 4f : Mathf.Sin(Time.time * 6f) * 3f;
            head.localRotation = Quaternion.Euler(8f + tilt, 0f, 0f);
        }
    }

    void UpdateVoice(bool moving)
    {
        if (voice == null)
            return;
        float target = (!frozen && moving) ? (hunting ? 0.45f : 0.18f) : 0f;
        voice.volume = Mathf.MoveTowards(voice.volume, target, Time.deltaTime * 2f);
    }

    void UpdateEyes()
    {
        if (eyeMaterial == null)
            return;
        float pulse = 1.4f + Mathf.Sin(Time.time * (frozen ? 2f : 7f)) * (frozen ? 0.2f : 0.7f);
        if (!frozen && hunting)
            pulse += 1.2f;
        eyeMaterial.SetColor("_EmissionColor", new Color(0.85f, 0.04f, 0.02f) * pulse);
    }

    float HorizontalDistance(Vector3 world)
    {
        Vector3 delta = world - transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }
}
