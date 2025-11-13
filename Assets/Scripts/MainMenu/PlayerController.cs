using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Tốc độ")]
    public float moveSpeed = 5.0f;      // Tốc độ di chuyển
    public float rotateSpeed = 10.0f;   // Tốc độ xoay (cần nhanh hơn để xoay mượt)

    [Header("Tài sản")]
    public Light flashlight;

    private CharacterController controller;
    private Animator animator;
    private Transform cameraMainTransform; // Biến để lưu trữ camera

    // Vật lý
    private float gravity = -9.81f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        cameraMainTransform = Camera.main.transform; // Tự động tìm camera chính
    }

    void Update()
    {
        // --- 1. XỬ LÝ TRỌNG LỰC ---
        HandleGravity();

        // --- 2. XỬ LÝ DI CHUYỂN & XOAY (Đã tối ưu hóa) ---
        HandleMovementAndRotation();

        // --- 3. XỬ LÝ ĐÈN PIN (PHÍM F) ---
        HandleFlashlight();
    }

    void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMovementAndRotation()
    {
        // Lấy input
        float moveZ_Input = Input.GetAxis("Vertical");   // W/S
        float moveX_Input = Input.GetAxis("Horizontal"); // A/D

        // Lấy hướng của camera (và làm phẳng nó, không quan tâm camera nhìn lên hay xuống)
        Vector3 camForward = cameraMainTransform.forward;
        Vector3 camRight = cameraMainTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Tính toán hướng di chuyển dựa trên camera
        Vector3 moveDirection = (camForward * moveZ_Input) + (camRight * moveX_Input);
        moveDirection.Normalize(); // Chuẩn hóa vector để di chuyển chéo không nhanh hơn

        // Áp dụng di chuyển
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // --- XỬ LÝ XOAY (Tối ưu hóa) ---
        if (moveDirection != Vector3.zero) // Chỉ xoay khi có di chuyển
        {
            // Tính toán hướng xoay (luôn quay mặt về hướng di chuyển)
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // Xoay nhân vật một cách "mượt" (Slerp)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        // --- XỬ LÝ ANIMATION ---
        // Animator chỉ cần biết TỐC ĐỘ
        // (vì nhân vật luôn quay mặt về trước, chúng ta không cần "Direction" nữa)
        float speed = new Vector2(moveX_Input, moveZ_Input).magnitude;
        animator.SetFloat("Speed", speed);
    }

    void HandleFlashlight()
    {
        if (flashlight != null && Input.GetKeyDown(KeyCode.F))
        {
            flashlight.enabled = !flashlight.enabled;
        }
    }
}