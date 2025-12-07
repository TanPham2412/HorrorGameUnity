using UnityEngine;
using UnityEngine.AI; // Cần để dùng NavMeshAgent

// Yêu cầu GameObject phải có NavMeshAgent
[RequireComponent(typeof(NavMeshAgent))]
public class GhostController : MonoBehaviour
{
    // === Cài đặt trong Inspector ===
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f; // Tốc độ khi đi tuần
    [SerializeField] private float chaseSpeed = 5f;  // Tốc độ khi đuổi theo Player
    [SerializeField] private float patrolRadius = 10f; // Bán kính tuần tra
    [SerializeField] private float patrolWaitTime = 2f; // Thời gian chờ khi đến điểm tuần

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 10f; // Bán kính phát hiện Player
    [SerializeField] private float detectionAngle = 90f; // Góc nhìn
    [SerializeField] private LayerMask playerLayer; // Layer của Player
    [SerializeField] private LayerMask obstacleLayer; // Layer của tường/vật cản

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f; // Khoảng cách để tấn công Player

    // === Biến nội bộ (Phần bạn bị thiếu) ===
    private NavMeshAgent agent;
    private Transform player;
    private PlayerStatus playerStatus; // <-- BIẾN BỊ THIẾU
    private Vector3 patrolTarget;
    private float patrolTimer; // <-- BIẾN BỊ THIẾU

    // === Các trạng thái của Ma ===
    private enum GhostState { Patrol, Chase, Attack }
    private GhostState currentState = GhostState.Patrol;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Tìm Player và script PlayerStatus
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerStatus = playerObj.GetComponent<PlayerStatus>(); // Gán biến ở đây
        }

        if (player == null)
        {
            Debug.LogError("GhostController: Không tìm thấy GameObject có tag 'Player'!");
        }
        if (playerStatus == null)
        {
            Debug.LogError("GhostController: Player không có script 'PlayerStatus'!");
        }
    }

    void Start()
    {
        // Bắt đầu trạng thái Patrol
        GoToNewPatrolPoint();
    }

    void Update()
    {
        if (player == null) return; // Nếu không có Player, không làm gì cả

        // Luôn kiểm tra Player trước
        DetectPlayer();

        switch (currentState)
        {
            case GhostState.Patrol:
                PatrolLogic();
                break;
            case GhostState.Chase:
                ChaseLogic();
                break;
            case GhostState.Attack:
                AttackLogic();
                break;
        }
    }

    // --- LOGIC PATROL (Tuần tra) ---
    void PatrolLogic()
    {
        agent.speed = patrolSpeed;

        // Nếu Ma đã đến gần điểm tuần tra
        if (agent.remainingDistance < 1f && !agent.pathPending)
        {
            patrolTimer += Time.deltaTime; // Bắt đầu đếm thời gian chờ
            if (patrolTimer >= patrolWaitTime)
            {
                GoToNewPatrolPoint(); // Điểm tuần mới
                patrolTimer = 0f;
            }
        }
    }

    void GoToNewPatrolPoint()
    {
        Vector3 randomPoint = RandomNavSphere(transform.position, patrolRadius, NavMesh.AllAreas);
        agent.SetDestination(randomPoint);
    }

    // Hàm tìm điểm ngẫu nhiên trên NavMesh
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * dist;
        randomDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    // --- LOGIC CHASE (Truy đuổi) ---
    void ChaseLogic()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        // Nếu Player ra quá xa, quay về Patrol
        if (Vector3.Distance(transform.position, player.position) > detectionRadius * 1.5f)
        {
            currentState = GhostState.Patrol;
            GoToNewPatrolPoint();
        }

        // Nếu Player đủ gần để tấn công
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            currentState = GhostState.Attack;
        }
    }

    // --- LOGIC ATTACK (Tấn công) ---
    void AttackLogic()
    {
        agent.isStopped = true; // Dừng lại khi tấn công

        if (playerStatus != null)
        {
            playerStatus.Die(); // Gọi hàm Die()
        }

        // Sau khi tấn công, quay lại Patrol
        currentState = GhostState.Patrol;
        agent.isStopped = false;
        GoToNewPatrolPoint();
    }


    // --- LOGIC DETECTION (Phát hiện Player) ---
    void DetectPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;

            // Kiểm tra góc nhìn
            if (Vector3.Angle(transform.forward, directionToPlayer) < detectionAngle / 2)
            {
                // Kiểm tra vật cản
                RaycastHit hit;
                if (!Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer, obstacleLayer))
                {
                    // Nếu không có vật cản -> Nhìn thấy Player
                    if (currentState != GhostState.Chase && currentState != GhostState.Attack)
                    {
                        currentState = GhostState.Chase;
                        Debug.Log("Ma đã phát hiện Player!");
                    }
                }
            }
        }
    }

    // Để dễ nhìn bán kính phát hiện và tấn công
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}