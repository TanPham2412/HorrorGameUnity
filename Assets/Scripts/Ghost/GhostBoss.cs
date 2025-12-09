using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class GhostBoss : MonoBehaviour
{
    [Header("Activation")]
    public bool isAIActive = false;

    [Header("Animation")]
    public Animation legacyAnimation;
    public string walkClipName = "GltfAnimation 0";

    [Header("Patrol Settings")]
    public string waypointPathName = "WaypointPath";
    public List<Transform> waypoints = new List<Transform>();
    public float patrolSpeed = 2f;
    public float waitTimeAtPoint = 2f;

    [Header("Detection (Cảm biến)")]
    public float visionRange = 15f;
    [Range(0, 360)] public float visionAngle = 140f;
    public float chaseSpeed = 5f;
    public float eyeOffset = 0.0f;
    public LayerMask detectionLayer;
    [Header("Cry Range")]
    public float cryRange = 20f;

    [Header("Attack Settings")]
    public float attackRange = 2.0f;

    [Header("Audio")]
    [SerializeField] private AudioSource ghostCrySource;
    [SerializeField] private AudioSource ghostScreamSource;

    // Các biến nội bộ
    private NavMeshAgent agent;
    private Transform player;
    private PlayerStatus playerStatus;
    private float patrolTimer = 0f;
    private int currentWaypointIndex = 0;

    // === ZME UPDATE: Biến lưu vị trí cuối cùng nhìn thấy ===
    private Vector3 lastKnownPosition;
    private bool hasLastKnownPosition = false;

    // Debug Raycast
    private Vector3 debugRayStart;
    private Vector3 debugRayEnd;
    private Color debugRayColor = Color.white;

    private enum State { Patrolling, Chasing, Attacking }
    [SerializeField] private State currentState;
    private bool cryPlaying;
    private bool screamPlaying;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        legacyAnimation = GetComponentInChildren<Animation>();

        if (legacyAnimation != null && !string.IsNullOrEmpty(walkClipName))
            legacyAnimation.wrapMode = WrapMode.Loop;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerStatus = playerObj.GetComponent<PlayerStatus>();
            if (playerStatus == null) playerStatus = playerObj.GetComponentInChildren<PlayerStatus>();
        }

        if (waypoints.Count == 0)
        {
            GameObject path = GameObject.Find(waypointPathName);
            if (path != null) foreach (Transform child in path.transform) waypoints.Add(child);
        }

        currentState = State.Patrolling;

        if (!isAIActive) agent.isStopped = true;
        else MoveToNextWaypoint();
    }

    void Update()
    {
        if (!isAIActive) { if (agent.isOnNavMesh) agent.isStopped = true; return; }

        // Xử lý Animation
        if (agent.isOnNavMesh && !agent.isStopped && currentState != State.Attacking)
        {
            if (legacyAnimation != null && !legacyAnimation.isPlaying) legacyAnimation.Play(walkClipName);
        }

        if (player == null) return;

        bool canSee = CheckFieldOfView();

        HandleVisionAudio(canSee);

        switch (currentState)
        {
            case State.Patrolling:
                if (canSee)
                {
                    // Phát hiện Player -> Chuyển sang đuổi bắt ngay
                    currentState = State.Chasing;
                    hasLastKnownPosition = true;
                    lastKnownPosition = player.position;
                    StartScreamAudio();
                    StopCryAudio();
                }
                else
                {
                    PatrolLogic();
                }
                break;

            case State.Chasing:
                ChaseLogic(canSee);
                break;

            case State.Attacking:
                break;
        }
    }

    // === ZME LOGIC: ĐUỔI BẮT CẢI TIẾN ===
    void ChaseLogic(bool canSee)
    {
        agent.speed = chaseSpeed;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. Nếu nhìn thấy Player
        if (canSee)
        {
            StopCryAudio();
            StartScreamAudio();
            // Cập nhật vị trí hiện tại làm vị trí cuối cùng
            lastKnownPosition = player.position;

            hasLastKnownPosition = true;

            // Đuổi theo vị trí thực
            if (agent.isOnNavMesh) agent.SetDestination(player.position);

            // Nếu bắt được -> Tấn công
            if (distToPlayer <= attackRange)
            {
                StartAttack();
            }
        }
        // 2. Nếu mất dấu (Khuất tường hoặc quá xa)
        else
        {
            if (hasLastKnownPosition)
            {
                // Tiếp tục chạy đến chỗ cuối cùng nhìn thấy (chứ không dừng lại ngay)
                if (agent.isOnNavMesh) agent.SetDestination(lastKnownPosition);

                // Kiểm tra xem đã đến nơi chưa
                float distToLastPos = Vector3.Distance(transform.position, lastKnownPosition);

                // Nếu đã đến "Vị trí cuối cùng" mà vẫn không thấy (hoặc Player đã chạy quá xa tầm nhìn)
                if (distToLastPos < 2.0f || distToPlayer > visionRange * 1.5f)
                {
                    // === ĐÂY LÀ PHẦN BẠN YÊU CẦU ===
                    // Player đã thoát -> Quay lại đi tuần ở Waypoint tiếp theo
                    hasLastKnownPosition = false;
                    currentState = State.Patrolling;
                    MoveToNextWaypoint();
                    StopScreamAudio();
                    if (IsPlayerWithinCryRange())
                    {
                        StartCryAudio();
                    }
                    Debug.Log("ZME: Player escaped via hiding. Returning to Patrol.");
                }
            }
            else
            {
                // Fallback nếu lỗi
                currentState = State.Patrolling;
                StopScreamAudio();
                if (IsPlayerWithinCryRange())
                {
                    StartCryAudio();
                }
            }
        }
    }

    private void HandleVisionAudio(bool canSee)
    {
        if (currentState == State.Attacking) return;

        bool inCryRange = IsPlayerWithinCryRange();

        if (!inCryRange)
        {
            StopCryAudio();
            return;
        }

        if (currentState == State.Patrolling && !canSee)
        {
            StartCryAudio();
            StopScreamAudio();
        }
    }

    private bool IsPlayerWithinVisionRangeOnly()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= visionRange;
    }

    private bool IsPlayerWithinCryRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= cryRange;
    }

    private void StartCryAudio()
    {
        if (ghostCrySource == null || cryPlaying) return;
        ghostCrySource.loop = true;
        ghostCrySource.Play();
        cryPlaying = true;
    }

    private void StopCryAudio()
    {
        if (ghostCrySource == null || !cryPlaying) return;
        ghostCrySource.Stop();
        cryPlaying = false;
    }

    private void StartScreamAudio()
    {
        if (ghostScreamSource == null || screamPlaying) return;
        ghostScreamSource.loop = true;
        ghostScreamSource.Play();
        screamPlaying = true;
    }

    private void StopScreamAudio()
    {
        if (ghostScreamSource == null || !screamPlaying) return;
        ghostScreamSource.Stop();
        screamPlaying = false;
    }

    // ... (Giữ nguyên các hàm Attack, ActivateGhost, PatrolLogic cũ) ...

    public void ActivateGhost()
    {
        if (isAIActive) return;
        isAIActive = true;
        agent.isStopped = false;
        MoveToNextWaypoint();
    }

    bool CheckFieldOfView()
    {
        if (player == null) return false;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > visionRange) return false;

        Vector3 targetCenter = player.position + Vector3.up * 1.2f;
        Vector3 ghostEyes = transform.position + Vector3.up * eyeOffset;
        Vector3 dirToPlayer = (targetCenter - ghostEyes).normalized;
        debugRayStart = ghostEyes;

        if (Vector3.Angle(transform.forward, dirToPlayer) < visionAngle / 2)
        {
            RaycastHit hit;
            if (Physics.Raycast(ghostEyes, dirToPlayer, out hit, distanceToPlayer, detectionLayer))
            {
                debugRayEnd = hit.point;
                if (hit.collider.CompareTag("Player")) { debugRayColor = Color.green; return true; }
                else { debugRayColor = Color.red; return false; }
            }
            else { debugRayEnd = ghostEyes + dirToPlayer * distanceToPlayer; debugRayColor = Color.green; return true; }
        }
        else { debugRayEnd = ghostEyes + transform.forward * 2f; debugRayColor = Color.yellow; }
        return false;
    }

    void PatrolLogic()
    {
        agent.speed = patrolSpeed;
        if (waypoints.Count == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= waitTimeAtPoint)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                MoveToNextWaypoint();
                patrolTimer = 0f;
            }
        }
    }

    void StartAttack()
    {
        if (currentState == State.Attacking) return;
        currentState = State.Attacking;
        if (agent.isOnNavMesh) agent.isStopped = true;
        if (legacyAnimation != null) legacyAnimation.Stop();
        StopScreamAudio();
        StopCryAudio();
        if (JumpscareManager.instance != null) JumpscareManager.instance.TriggerJumpscare();
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Count > 0 && agent.isOnNavMesh)
            agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = debugRayColor;
        if (debugRayStart != Vector3.zero) Gizmos.DrawLine(debugRayStart, debugRayEnd);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, visionRange);

        // Vẽ điểm cuối cùng nhìn thấy để debug
        if (currentState == State.Chasing && hasLastKnownPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastKnownPosition, 0.5f);
            Gizmos.DrawLine(transform.position, lastKnownPosition);
        }
    }
}