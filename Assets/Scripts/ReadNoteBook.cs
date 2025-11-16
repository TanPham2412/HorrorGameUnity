using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReadNotebook : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float TheDistance;
    public GameObject ActionDisplay;   
    public GameObject ActionText;      // "Press E"
    public GameObject NameObject;      // "Notebook"
    public GameObject ExtraCross;      // crosshair phụ (nếu có)

    [Header("Note UI")]
    public GameObject notePanel;       // Panel UI full màn hình
    public Image noteImage;            // Image hiển thị tờ giấy
    public Sprite noteSprite;          // notebookpaper.jpg
    public TextMeshProUGUI noteText;   // Text nằm trên tờ giấy
    [TextArea(3, 8)]
    public string noteContent;         // Nội dung bạn muốn viết

    [Header("Player")]
    public MonoBehaviour playerMovementScript; // script điều khiển nhân vật
    public GameObject crosshair;              // crosshair chính

    private bool isOpen = false;

    void Start()
    {
        // Đảm bảo panel tắt lúc bắt đầu game
        if (notePanel != null)
        {
            notePanel.SetActive(false);
        }
    }

    void Update()
    {
        // Cập nhật khoảng cách giống các script khác
        TheDistance = PlayerCasting.DistanceFromTarget;

        // Nếu đang mở note, cho phép đóng lại
        if (isOpen)
        {
            // CHỈ dùng phím Escape để đóng, tránh bị mở xong đóng ngay khi bấm E
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseNote();
            }
        }
    }

    void OnMouseOver()
    {
        if (TheDistance <= 3f && !isOpen)
        {
            NameObject.SetActive(true);
            ActionDisplay.SetActive(true);
            ActionText.SetActive(true);
            ExtraCross.SetActive(true);

            // Dùng Input.GetButtonDown("Action") giống PickUpItem
            if (Input.GetButtonDown("Action"))
            {
                OpenNote();
            }
        }
        else if (!isOpen)
        {
            ActionDisplay.SetActive(false);
            ActionText.SetActive(false);
            ExtraCross.SetActive(false);
            NameObject.SetActive(false);
        }
    }

    void OnMouseExit()
    {
        if (!isOpen)
        {
            ActionDisplay.SetActive(false);
            ActionText.SetActive(false);
            ExtraCross.SetActive(false);
            NameObject.SetActive(false);
        }
    }

    private void OpenNote()
    {
        isOpen = true;

        Debug.Log("OpenNote: mở notebook");

        // Tắt điều khiển nhân vật & crosshair nếu cần
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (crosshair != null) crosshair.SetActive(false);

        // Bật panel note
        if (notePanel != null) notePanel.SetActive(true);
        if (noteImage != null && noteSprite != null) noteImage.sprite = noteSprite;
        if (noteText != null) noteText.text = noteContent;
    }

    private void CloseNote()
    {
        isOpen = false;

        Debug.Log("CloseNote: đóng notebook");

        if (notePanel != null) notePanel.SetActive(false);
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (crosshair != null) crosshair.SetActive(true);

        ActionDisplay.SetActive(false);
        ActionText.SetActive(false);
        ExtraCross.SetActive(false);
        NameObject.SetActive(false);
    }
}