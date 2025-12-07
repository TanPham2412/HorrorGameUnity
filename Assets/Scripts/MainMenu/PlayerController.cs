using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Tốc độ")]
    public float moveSpeed = 5.0f;
    // NOTE: rotateSpeed không còn dùng cho xoay thân nữa, Mouse Look sẽ điều khiển.

    [Header("Góc nhìn (FPS)")] // THIẾT LẬP MỚI
    public float mouseSensitivity = 300f; // Tốc độ xoay chuột

    [Header("Tài sản")]
    public Light flashlight;

    private CharacterController controller;
    private Animator animator;
    private Transform cameraMainTransform;
    private float xRotation = 0f; // Biến để lưu trữ góc xoay dọc của camera

    // Vật lý
    private float gravity = -9.81f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        // Tìm camera con (Giả sử Main Camera là con trực tiếp của Player)
        cameraMainTransform = GetComponentInChildren<Camera>().transform;

        // KHÓA CON TRỎ CHUỘT
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- 1. XỬ LÝ TRỌNG LỰC ---
        HandleGravity();

        // --- 2. XỬ LÝ DI CHUYỂN ---
        HandleMovement();

        // --- 3. XỬ LÝ GÓC NHÌN CHUỘT (MỚI) ---
        HandleMouseLook();

        // --- 4. XỬ LÝ ĐÈN PIN (PHÍM F) ---
        HandleFlashlight();
    }

    // [Các hàm phụ trợ]

    void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Đã đổi tên và thay đổi logic di chuyển/xoay
    void HandleMovement()
    {
        // Lấy input
        float moveZ_Input = Input.GetAxis("Vertical");
        float moveX_Input = Input.GetAxis("Horizontal");

        // Tính toán di chuyển dựa trên hướng của BODY (transform.forward/right)
        // Body (Player) luôn quay theo chuột, nên đây là hướng di chuyển đúng.
        Vector3 forwardMovement = transform.forward * moveZ_Input;
        Vector3 rightMovement = transform.right * moveX_Input;

        // Tổng hợp hướng di chuyển (chuẩn FPS)
        Vector3 finalMoveDirection = (forwardMovement + rightMovement).normalized;

        // Áp dụng di chuyển
        controller.Move(finalMoveDirection * moveSpeed * Time.deltaTime);

        // --- XỬ LÝ ANIMATION ---
        // (Sử dụng input gốc để biết người chơi có muốn di chuyển không)
        float speed = new Vector2(moveX_Input, moveZ_Input).magnitude;
        animator.SetFloat("Speed", speed);
    }

    // === HÀM XỬ LÝ GÓC NHÌN MỚI (FPS MOUSE LOOK) ===
    void HandleMouseLook()
    {
        // Nhận input chuột (đã nhân với mouseSensitivity và Time.deltaTime)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 1. Xoay thân nhân vật (trục Y - ngang): Xoay đối tượng Player (Cha)
        transform.Rotate(Vector3.up * mouseX);

        // 2. Xoay camera (trục X - dọc/pitch): Xoay đối tượng Camera (Con)
        xRotation -= mouseY; // Trừ vì trục Y chuột ngược với trục X camera

        // Giới hạn góc nhìn lên/xuống (-90 độ là nhìn thẳng lên, 90 độ là nhìn thẳng xuống)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Áp dụng xoay dọc cho camera (chỉ xoay camera, không xoay thân)
        cameraMainTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }


    void HandleFlashlight()
    {
        if (flashlight != null && Input.GetKeyDown(KeyCode.F))
        {
            flashlight.enabled = !flashlight.enabled;
        }
    }
}