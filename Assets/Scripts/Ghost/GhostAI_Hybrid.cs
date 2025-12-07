using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class GhostAI_Hybrid : MonoBehaviour
{
    [Header("Activation")]
    public bool isAIActive = false;

    [Header("Animation")]
    public Animation legacyAnimation;
    public string walkClipName = "GltfAnimation 0";

    [Header("Patrol")]
    public string waypointPathName = "WaypointPath";
    public List<Transform> waypoints = new List<Transform>();
    public float patrolSpeed = 2f;
    public float waitTimeAtPoint = 2f;

    [Header("Detection (Quét Vùng)")]
    public float visionRange = 15f;
    [Range(0, 360)] public float visionAngle = 140f;
    public float chaseSpeed = 5f;

    [Header("Eyes / Target Offsets")]
    [Tooltip("Nếu gán, tia nhìn sẽ lấy vị trí mắt từ Transform này.")]
    public Transform eyePoint;
    [Tooltip("Điều chỉnh thêm độ cao so với mắt mặc định (1.4m) nếu không có eyePoint.")]
    public float eyeOffset = 0.2f;
    [Tooltip("Độ cao (tính từ pivot của player) để raycast nhắm tới.")]
    public float playerTargetHeight = 1.2f;

    // CHỈ CHỌN LỚP: Default, Wall... (KHÔNG CHỌN PLAYER)
    public LayerMask detectionLayer;

    [Header("Attack")]
    public float attackRange = 2.0f;

    private NavMeshAgent agent;
    private Transform player;
    private PlayerStatus playerStatus;
    private float patrolTimer = 0f;
    private int currentWaypointIndex = 0;

    // Debug Raycast
    private Vector3 debugRayStart;
    private Vector3 debugRayEnd;
    private Color debugRayColor = Color.white;

    private enum State { Patrolling, Chasing, Attacking }
    [SerializeField] private State currentState;

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

        if (agent.isOnNavMesh && agent.isStopped && currentState != State.Attacking)
        {
            agent.isStopped = false;
            if (legacyAnimation != null && !legacyAnimation.isPlaying) legacyAnimation.Play(walkClipName);
        }

        if (player == null) return;

        bool canSee = CheckFieldOfView();
        if (canSee)
        {
            Debug.Log("GhostAI_Hybrid: Player detected, switching to chase if needed.");
        }

        switch (currentState)
        {
            case State.Patrolling:
                if (canSee) currentState = State.Chasing;
                else PatrolLogic();
                break;
            case State.Chasing:
                ChaseLogic(canSee);
                break;
            case State.Attacking: break;
        }
    }

    public void ActivateGhost()
    {
        if (isAIActive) return;
        isAIActive = true;
        agent.isStopped = false;
        MoveToNextWaypoint();
    }

    // === ZME FIX: LOGIC NHÌN LINH HOẠT ===
    bool CheckFieldOfView()
    {
        if (player == null) return false;

        Vector3 ghostEyes = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * (1.4f + eyeOffset);
        Vector3 targetCenter = player.position + Vector3.up * playerTargetHeight; // Ngực Player

        float distanceToPlayer = Vector3.Distance(ghostEyes, targetCenter);
        if (distanceToPlayer > visionRange) return false;

        Vector3 facingDir = eyePoint != null ? eyePoint.forward : transform.forward;
        Vector3 dirToPlayer = (targetCenter - ghostEyes).normalized;

        // Vẽ tia debug liên tục để bạn dễ chỉnh
        debugRayStart = ghostEyes;

        if (Vector3.Angle(facingDir, dirToPlayer) < visionAngle / 2)
        {
            int rayMask = detectionLayer;
            // Luôn cho phép nhìn thấy Player cho dù dropdown không tick
            rayMask |= 1 << player.gameObject.layer;
            // Bỏ layer của chính con ma để tia không bị collider bản thân chặn
            rayMask &= ~(1 << gameObject.layer);

            RaycastHit[] hits = Physics.RaycastAll(ghostEyes, dirToPlayer, distanceToPlayer, rayMask, QueryTriggerInteraction.Ignore);
            if (hits.Length > 1)
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider.transform.root == transform) continue; // bỏ qua collider của ma

                debugRayEnd = hit.point;
                if (hit.collider.CompareTag("Player"))
                {
                    debugRayColor = Color.green; // THẤY!
                    return true;
                }

                debugRayColor = Color.red; // BỊ CHẶN
                return false;
            }

            // Không trúng gì hoặc chỉ trúng chính mình -> coi như thấy Player
            debugRayEnd = ghostEyes + dirToPlayer * distanceToPlayer;
            debugRayColor = Color.green;
            return true;
        }
        else
        {
            debugRayEnd = ghostEyes + facingDir * 2f;
            debugRayColor = Color.yellow; // Ngoài góc nhìn
        }
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

    void ChaseLogic(bool canSee)
    {
        agent.speed = chaseSpeed;
        if (agent.isOnNavMesh) agent.SetDestination(player.position);
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange) { StartAttack(); return; }
        if (!canSee && dist > visionRange * 1.2f) { currentState = State.Patrolling; MoveToNextWaypoint(); }
    }

    void StartAttack()
    {
        currentState = State.Attacking;
        if (agent.isOnNavMesh) agent.isStopped = true;
        if (playerStatus != null) playerStatus.Die();
        else UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Count > 0 && agent.isOnNavMesh) agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void OnDrawGizmos()
    {
        // Vẽ tia nhìn thời gian thực
        Gizmos.color = debugRayColor;
        if (debugRayStart != Vector3.zero)
        {
            Gizmos.DrawLine(debugRayStart, debugRayEnd);
            Gizmos.DrawWireSphere(debugRayEnd, 0.1f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        if (player != null)
        {
            Vector3 targetCenter = player.position + Vector3.up * playerTargetHeight;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(targetCenter, 0.05f);
        }
    }
}