using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class GhostAI_Hybrid : MonoBehaviour
{
    // === THAY ĐỔI: Dùng Animation cũ thay vì Animator ===
    private Animation legacyAnimation;

    [Header("Legacy Animation Settings")]
    public string walkClipName = "GltfAnimation 0"; // Tên clip đi bộ
    public string jumpscareClipName = "";           // Tên clip hù (để trống nếu không có)

    // === CÀI ĐẶT CHUNG ===
    private NavMeshAgent agent;
    private Transform player;
    private PlayerStatus playerStatus;

    // === CÁC TRẠNG THÁI AI ===
    private enum State { Patrolling, Searching, Chasing, Attacking }
    private State currentState;

    // === CÀI ĐẶT PATROL ===
    [Header("Patrol")]
    public List<Transform> waypoints;
    public float patrolSpeed = 2f;
    public float waitTimeAtPoint = 2f;
    private int currentWaypointIndex = 0;
    private float patrolTimer = 0f;

    // === CÀI ĐẶT PHÁT HIỆN ===
    [Header("Detection")]
    public float visionRange = 10f;
    public float visionAngle = 90f;
    public float hearingRange = 20f;
    public float chaseSpeed = 5f;
    private Vector3 lastHeardPosition;

    // === CÀI ĐẶT TẤN CÔNG ===
    [Header("Attack")]
    public float attackRange = 1.5f;
    public LayerMask obstacleMask;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // === THAY ĐỔI: Tìm component Animation ===
        legacyAnimation = GetComponentInChildren<Animation>();

        // Tự động chạy animation đi bộ ngay khi bắt đầu
        if (legacyAnimation != null && !string.IsNullOrEmpty(walkClipName))
        {
            legacyAnimation.Play(walkClipName);
        }
        // =========================================

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStatus = playerObject.GetComponent<PlayerStatus>();
        }

        currentState = State.Patrolling;

        if (!EnsureAgentOnNavMesh()) return;

        if (waypoints.Count > 0)
        {
            agent.speed = patrolSpeed;
            TrySetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        if (!GameManager.ghostIsActive)
        {
            agent.isStopped = true;
            return;
        }
        if (agent.isStopped && currentState != State.Attacking)
        {
            agent.isStopped = false;
            // Đảm bảo animation đi bộ đang chạy
            if (legacyAnimation != null && !legacyAnimation.isPlaying)
                legacyAnimation.Play(walkClipName);
        }

        if (player == null) return;

        bool canSeePlayer = CanSeePlayer();

        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrolling(canSeePlayer);
                break;
            case State.Searching:
                HandleSearching(canSeePlayer);
                break;
            case State.Chasing:
                HandleChasing(canSeePlayer);
                break;
            case State.Attacking:
                HandleAttacking();
                break;
        }
    }

    // ... (Các hàm HandlePatrolling, Searching, Chasing giữ nguyên như cũ) ...
    // Để ngắn gọn, tôi chỉ viết lại hàm HandleAttacking có thay đổi

    void HandlePatrolling(bool canSeePlayer)
    {
        agent.speed = patrolSpeed;
        if (canSeePlayer) { patrolTimer = 0f; ChangeState(State.Chasing); return; }
        if (waypoints.Count == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= waitTimeAtPoint)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                TrySetDestination(waypoints[currentWaypointIndex].position);
                patrolTimer = 0f;
            }
        }
        else patrolTimer = 0f;
    }

    void HandleSearching(bool canSeePlayer)
    {
        agent.speed = chaseSpeed * 0.75f;
        TrySetDestination(lastHeardPosition);

        if (canSeePlayer) { ChangeState(State.Chasing); return; }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            ChangeState(State.Patrolling);
    }

    void HandleChasing(bool canSeePlayer)
    {
        agent.speed = chaseSpeed;
        TrySetDestination(player.position);

        if (!canSeePlayer)
        {
            Debug.Log("Mất dấu"); ChangeState(State.Patrolling); TrySetDestination(waypoints[currentWaypointIndex].position); return;

        }
        if (Vector3.Distance(transform.position, player.position) < attackRange)
            ChangeState(State.Attacking);
    }

    void HandleAttacking()
    {
        agent.isStopped = true; // Dừng ma lại

        // --- KÍCH HOẠT VIDEO JUMPSCARE ---
        if (JumpscareManager.instance != null)
        {
            JumpscareManager.instance.TriggerJumpscare();
        }
        else
        {
            // Dự phòng nếu quên tạo JumpscareManager thì vẫn reset game
            if (playerStatus != null) playerStatus.Die();
        }
        // ---------------------------------

        // Không cần gọi playerStatus.Die() ở đây nữa
        // Vì JumpscareManager sẽ lo việc đó sau khi video hết.

        ChangeState(State.Patrolling);
    }

    // ... (Các hàm tiện ích giữ nguyên) ...
    bool CanSeePlayer()
    {
        if (player == null) return false;
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > visionRange) return false;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > visionAngle / 2) return false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, distance, obstacleMask)) return false;
        return true;
    }

    public void HeardLoudNoise(Vector3 noisePosition)
    {
        if (currentState == State.Patrolling && Vector3.Distance(transform.position, noisePosition) < hearingRange)
        {
            lastHeardPosition = noisePosition;
            ChangeState(State.Searching);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        DoorInteraction door = collision.gameObject.GetComponentInParent<DoorInteraction>();
        if (door != null) door.OpenDoorForAI();
    }

    void ChangeState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        agent.isStopped = false;
    }

    bool EnsureAgentOnNavMesh()
    {
        if (agent.isOnNavMesh) return true;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return true;
        }

        Debug.LogError($"{nameof(GhostAI_Hybrid)}: NavMeshAgent is not on a NavMesh. Please bake a NavMesh or move the ghost onto it.");
        enabled = false;
        return false;
    }

    bool TrySetDestination(Vector3 targetPosition)
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning($"{nameof(GhostAI_Hybrid)}: Cannot set destination because the agent is not on a NavMesh.");
            return false;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
        {
            return agent.SetDestination(hit.position);
        }

        Debug.LogWarning($"{nameof(GhostAI_Hybrid)}: No NavMesh point found near target position {targetPosition}.");
        return false;
    }
}