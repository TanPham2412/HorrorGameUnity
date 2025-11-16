using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClueLogManager : MonoBehaviour
{
    public static ClueLogManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject logPanel;
    public Button cluesTabButton;
    public Button monologueTabButton;
    public GameObject cluesContainer;
    public GameObject monologueContainer;
    public ScrollRect cluesScrollRect;
    public ScrollRect monologueScrollRect;
    public Transform cluesContentRoot;
    public Transform monologueContentRoot;
    public GameObject entryPrefab;

    [Header("Player Control References")]
    public MonoBehaviour playerMovementScript;
    public GameObject crosshair;

    [Header("Input Settings")]
    public KeyCode toggleKey = KeyCode.I;

    private readonly List<string> clues = new();
    private readonly List<string> monologues = new();
    private bool isOpen;
    private bool showingClues = true;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (logPanel != null) logPanel.SetActive(false);
        cluesTabButton?.onClick.AddListener(() => ShowTab(true));
        monologueTabButton?.onClick.AddListener(() => ShowTab(false));
        ShowTab(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen) CloseLog();
            else OpenLog();
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseLog();
        }
    }

    public static void AddClue(string text)
    {
        if (Instance == null)
        {
            Debug.LogWarning("ClueLogManager not present in scene.");
            return;
        }

        Debug.Log("ClueLogManager.AddClue called with: " + text);
        Instance.InternalAddEntry(text, true);
    }

    public static void AddMonologue(string text)
    {
        if (Instance == null)
        {
            Debug.LogWarning("ClueLogManager not present in scene.");
            return;
        }

        Debug.Log("ClueLogManager.AddMonologue called with: " + text);
        Instance.InternalAddEntry(text, false);
    }

    void InternalAddEntry(string text, bool isClue)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning("ClueLogManager.InternalAddEntry: empty text, skip.");
            return;
        }
        if (entryPrefab == null)
        {
            Debug.LogError("ClueLogManager.InternalAddEntry: entryPrefab is NULL. Gán prefab dòng text vào Inspector.");
            return;
        }

        var targetList = isClue ? clues : monologues;
        var contentRoot = isClue ? cluesContentRoot : monologueContentRoot;

        if (contentRoot == null)
        {
            Debug.LogError("ClueLogManager.InternalAddEntry: contentRoot is NULL (" + (isClue ? "cluesContentRoot" : "monologueContentRoot") + ").");
            return;
        }

        if (targetList.Contains(text))
        {
            Debug.Log("ClueLogManager.InternalAddEntry: text already exists, skip duplicate.");
            return;     // tránh trùng
        }

        targetList.Add(text);

        Debug.Log("ClueLogManager.InternalAddEntry: creating UI entry in " + (isClue ? "Clues" : "Monologues"));
        var entry = Instantiate(entryPrefab, contentRoot);
        var entryText = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (entryText != null) entryText.text = text;
        else Debug.LogWarning("ClueLogManager.InternalAddEntry: entryPrefab không có TextMeshProUGUI con.");

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot as RectTransform);
        Canvas.ForceUpdateCanvases();

        if (isClue && cluesScrollRect != null)
            cluesScrollRect.verticalNormalizedPosition = 0f;  // cuộn xuống cuối
        else if (!isClue && monologueScrollRect != null)
            monologueScrollRect.verticalNormalizedPosition = 0f;
    }

    void ShowTab(bool showCluesTab)
    {
        showingClues = showCluesTab;
        if (cluesContainer != null) cluesContainer.SetActive(showingClues);
        if (monologueContainer != null) monologueContainer.SetActive(!showingClues);

        if (cluesTabButton != null) cluesTabButton.interactable = !showingClues;
        if (monologueTabButton != null) monologueTabButton.interactable = showingClues;
    }

    public void ShowCluesTab()
    {
        Debug.Log("ClueLogManager: CLICK Manh Moi tab");
        ShowTab(true);
    }

    public void ShowMonologueTab()
    {
        Debug.Log("ClueLogManager: CLICK Loi Thoai tab");
        ShowTab(false);
    }

    void OpenLog()
    {
        isOpen = true;
        if (logPanel != null) logPanel.SetActive(true);
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (crosshair != null) crosshair.SetActive(false);

        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseLog()
    {
        isOpen = false;
        if (logPanel != null) logPanel.SetActive(false);
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (crosshair != null) crosshair.SetActive(true);

        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
    }
}