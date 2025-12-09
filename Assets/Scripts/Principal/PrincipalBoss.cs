using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class PrincipalBoss : MonoBehaviour
{
    [Header("Activation")]
    public bool isAIActive = false;

    [Header("Animation (Animator)")]
    public Animator animator;
    public string walkStateName = "Walk";
    public string idleStateName = "Idle";

    [Header("Patrol Settings")]
    public string waypointPathName = "BasementPatrolPath";
    public List<Transform> waypoints = new List<Transform>();
    public float patrolSpeed = 2.5f;
    public float waitTimeAtPoint = 3f;

    [Header("Detection")]
    public float visionRange = 12f;
    [Range(0, 360)] public float visionAngle = 120f;
    public float chaseSpeed = 5f;
    public float eyeOffset = 1.6f;
    public LayerMask detectionLayer;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;

    [Header("Boss Specific")]
    public PrincipalJumpscare principalJumpscare;

    [Header("Respawn After Jumpscare")]
    public Transform respawnPoint;

    [Header("Search Settings")]
    public float searchDuration = 4.0f;
    public float searchTurnSpeed = 120f;

    [Header("Audio")]
    [SerializeField] private AudioSource chaseAudioSource;
    [SerializeField] private bool loopChaseAudio = true;

    // Private vars
    private NavMeshAgent agent;
    private Transform player;
    private float patrolTimer = 0f;
    private float searchTimer = 0f;
    private int currentWaypointIndex = 0;
    private Vector3 lastKnownPosition;
    private bool hasLastKnownPosition = false;
    private string currentAnimState = "";
    private bool chaseAudioPlaying;

    private enum State { Patrolling, Chasing, Attacking, Searching }
    [SerializeField] private State currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (principalJumpscare != null)
            principalJumpscare.bossController = this;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;

        LoadWaypoints();

        if (respawnPoint == null && waypoints.Count > 0)
            respawnPoint = waypoints[0];

        currentState = State.Patrolling;

        if (!isAIActive)
            agent.isStopped = true;
        else
            MoveToNextWaypoint();
    }

    void Update()
    {
        if (!isAIActive) { agent.isStopped = true; return; }
        if (player == null) return;

        bool canSee = CheckFieldOfView();
        UpdateAnimation();

        switch (currentState)
        {
            case State.Patrolling:
                if (canSee)
                {
                    currentState = State.Chasing;
                    hasLastKnownPosition = true;
                    lastKnownPosition = player.position;
                    agent.SetDestination(player.position);
                    StartChaseAudio();
                }
                else PatrolLogic();
                break;

            case State.Chasing:
                ChaseLogic(canSee);
                break;

            case State.Searching:
                SearchLogic(canSee);
                break;

            case State.Attacking:
                break;
        }
    }

    void LoadWaypoints()
    {
        if (waypoints.Count > 0) return;

        GameObject path = GameObject.Find(waypointPathName);
        if (path != null)
        {
            foreach (Transform c in path.transform)
                waypoints.Add(c);
        }
    }

    // ================================================================
    //                 FIX LỖI RESET SAU JUMPSCARE
    // ================================================================
    public void OnJumpscareEnded()
    {
        Debug.Log("ZME: Jumpscare kết thúc. Chuyển về Main Menu...");

        // 1. Mở khóa con trỏ chuột (Rất quan trọng, nếu không về menu không bấm được)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Chuyển cảnh về Main Menu
        // Hãy đảm bảo tên Scene trùng khớp với tên file trong Project của bạn
        // Ví dụ: "MainMenu_Scene" hoặc "MainMenu"
        SceneManager.LoadScene("MainMenu_Scene");
    }

    // ================================================================

    void UpdateAnimation()
    {
        if (animator == null) return;

        if (currentState == State.Attacking)
        {
            animator.speed = 0;
            return;
        }
        else animator.speed = 1;

        string targetState =
          (agent.velocity.sqrMagnitude > 0.1f && !agent.isStopped)
          ? walkStateName
          : idleStateName;

        if (currentAnimState != targetState)
        {
            animator.CrossFade(targetState, 0.2f);
            currentAnimState = targetState;
        }
    }

    void PatrolLogic()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            patrolTimer += Time.deltaTime;
            agent.isStopped = true;

            if (patrolTimer >= waitTimeAtPoint)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                patrolTimer = 0f;
                agent.isStopped = false;
                MoveToNextWaypoint();
            }
        }
    }

    void ChaseLogic(bool canSee)
    {
        agent.speed = chaseSpeed;

        float dist = Vector3.Distance(transform.position, player.position);

        if (canSee)
        {
            lastKnownPosition = player.position;
            hasLastKnownPosition = true;
            agent.SetDestination(player.position);

            if (dist <= attackRange)
                StartAttack();
        }
        else
        {
            if (hasLastKnownPosition)
            {
                agent.SetDestination(lastKnownPosition);

                if (Vector3.Distance(transform.position, lastKnownPosition) < 1f)
                {
                    currentState = State.Searching;
                    searchTimer = 0f;
                    agent.isStopped = true;
                    StopChaseAudio();
                }
            }
            else
            {
                currentState = State.Patrolling;
                StopChaseAudio();
                MoveToNextWaypoint();
            }
        }
    }

    void SearchLogic(bool canSee)
    {
        if (canSee)
        {
            currentState = State.Chasing;
            agent.isStopped = false;
            StartChaseAudio();
            return;
        }

        searchTimer += Time.deltaTime;

        if (searchTimer >= searchDuration)
        {
            currentState = State.Patrolling;
            agent.isStopped = false;
            StopChaseAudio();
            MoveToNextWaypoint();
        }
    }

    void StartAttack()
    {
        if (currentState == State.Attacking) return;

        currentState = State.Attacking;
        agent.isStopped = true;
        StopChaseAudio();

        if (principalJumpscare != null)
            principalJumpscare.TriggerPrincipalJumpscare();
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Count == 0) return;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    bool CheckFieldOfView()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > visionRange) return false;

        Vector3 eyes = transform.position + Vector3.up * eyeOffset;
        Vector3 target = player.position + Vector3.up * 1.2f;
        Vector3 dir = (target - eyes).normalized;

        if (Vector3.Angle(transform.forward, dir) < visionAngle / 2)
        {
            if (Physics.Raycast(eyes, dir, out RaycastHit hit, dist, detectionLayer))
            {
                return hit.collider.CompareTag("Player");
            }
            return true;
        }

        return false;
    }

    private void StartChaseAudio()
    {
        if (chaseAudioSource == null || chaseAudioPlaying) return;
        chaseAudioSource.loop = loopChaseAudio;
        chaseAudioSource.Play();
        chaseAudioPlaying = true;
    }

    private void StopChaseAudio()
    {
        if (chaseAudioSource == null || !chaseAudioPlaying) return;
        chaseAudioSource.Stop();
        chaseAudioPlaying = false;
    }

    public void ActivatePrincipal()
    {
        // Nếu đã kích hoạt rồi thì không làm gì cả (tránh lỗi gọi nhiều lần)
        if (isAIActive) return;

        Debug.Log("ZME: THẦY HIỆU TRƯỞNG ĐÃ XUẤT HIỆN!");

        // 1. Bật cờ hoạt động
        isAIActive = true;

        // 2. Mở khóa di chuyển
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed; // Đảm bảo tốc độ đúng
        }

        // 3. Reset Animation (đảm bảo không bị đơ)
        if (animator != null)
        {
            animator.speed = 1;
            animator.Play(walkStateName); // Chuyển sang đi bộ ngay
        }

        // 4. Bắt đầu đi tuần ngay lập tức
        currentState = State.Patrolling;
        MoveToNextWaypoint();
    }
}
