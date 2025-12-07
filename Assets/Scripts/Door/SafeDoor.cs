using System.Collections;
using UnityEngine;
using TMPro;

public class SafeDoor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 2.0f;
    public GameObject actionDisplay;
    public GameObject actionText;
    public GameObject lockedText;
    public GameObject nameObject;
    public GameObject extraCross;
    public GameObject safeDoorObject;
    public string openAnimationName = "SafeDoorOpen";
    public AudioSource openSound;

    [Header("Requirements")]
    public bool requiresKeycard = true;
    public ItemType requiredItem = ItemType.SafeCard;

    [Header("Keypad UI")]
    public GameObject keypadPanel;
    public TMP_InputField keypadInput;
    public string correctCode = "0308";

    [Header("Player Control While Keypad Open")]
    public MonoBehaviour playerMovementScript;
    public GameObject crosshair;

    [Header("Monologue Lines")]
    [TextArea(2, 5)] public string firstSeenLine = "Một cái két sắt. Nặng trịch. Cần cả thẻ từ... và một dãy số. Không thể mở bằng tay không được rồi.";
    [TextArea(2, 5)] public string hintLineAfterSequence = "Đợi đã... 0308... 'một dãy số'? Có khi nào... là mật khẩu két sắt";
    public float monologueDuration = 4f;

    [Header("Sequence Dependency")]
    public string requiredSequenceKey = "Birthday0308";

    private bool isOpen;
    private bool hasPlayedFirstLine;
    private bool hasPlayedHintLine;
    private bool isFocused;
    private bool isKeypadActive;

    void Awake()
    {
        if (keypadInput != null)
        {
            keypadInput.onValidateInput += AllowOnlyDigits;
        }
    }

    void Update()
    {
        if (isKeypadActive)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SubmitKeypadCode();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideKeypad();
            }
        }
    }

    void OnMouseOver()
    {
        float distance = PlayerCasting.DistanceFromTarget;
        bool inRange = distance <= interactionRange;

        if (inRange && !isFocused)
        {
            isFocused = true;
            OnFocus();
        }
        else if (!inRange && isFocused)
        {
            isFocused = false;
            OnLoseFocus();
        }

        if (Input.GetButtonDown("Action"))
        {
            if (isKeypadActive)
            {
                HideKeypad();
                return;
            }

            if (inRange)
            {
                TryInteract();
            }
        }
    }

    void OnMouseExit()
    {
        if (isFocused)
        {
            isFocused = false;
            OnLoseFocus();
        }
    }

    void OnFocus()
    {
        if (isOpen)
        {
            HideAllInteractionUI();
            return;
        }

        if (nameObject != null) nameObject.SetActive(true);
        if (extraCross != null) extraCross.SetActive(true);

        if (CanOpen())
        {
            if (actionDisplay != null) actionDisplay.SetActive(true);
            if (actionText != null) actionText.SetActive(true);
        }
        else
        {
            if (lockedText != null) lockedText.SetActive(true);
        }
    }

    void OnLoseFocus()
    {
        HideAllInteractionUI();
    }

    void HideAllInteractionUI()
    {
        if (nameObject != null) nameObject.SetActive(false);
        if (extraCross != null) extraCross.SetActive(false);
        if (actionDisplay != null) actionDisplay.SetActive(false);
        if (actionText != null) actionText.SetActive(false);
        if (lockedText != null) lockedText.SetActive(false);
    }

    void TryInteract()
    {
        if (!hasPlayedFirstLine && !string.IsNullOrWhiteSpace(firstSeenLine))
        {
            MonologueManager.PlayMonologue(firstSeenLine, monologueDuration, true, true);
            hasPlayedFirstLine = true;
        }

        if (!CanOpen())
        {
            if (!hasPlayedHintLine && HasSequenceProgress())
            {
                MonologueManager.PlayMonologue(hintLineAfterSequence, monologueDuration, true, true);
                hasPlayedHintLine = true;
            }

            if (lockedText != null) lockedText.SetActive(true);
            return;
        }

        if (keypadPanel != null && keypadInput != null)
        {
            ShowKeypad();
        }
        else
        {
            ValidateCodeAndOpen(correctCode);
        }
    }

    public void SubmitKeypadCode()
    {
        if (keypadInput == null) return;
        ValidateCodeAndOpen(keypadInput.text);
    }

    void LateUpdate()
    {
        if (isKeypadActive && keypadInput != null && !keypadInput.isFocused)
        {
            keypadInput.ActivateInputField();
            keypadInput.selectionAnchorPosition = keypadInput.text.Length;
            keypadInput.selectionFocusPosition = keypadInput.text.Length;
        }
    }

    void ValidateCodeAndOpen(string inputCode)
    {
        if (inputCode == correctCode)
        {
            HideKeypad();
            OpenSafe();
        }
        else
        {
            if (keypadInput != null) keypadInput.text = string.Empty;
        }
    }

    void ShowKeypad()
    {
        if (keypadPanel == null) return;

        keypadPanel.SetActive(true);
        keypadInput.text = string.Empty;
        isKeypadActive = true;

        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (crosshair != null) crosshair.SetActive(false);

        keypadInput.ActivateInputField();
    }

    char AllowOnlyDigits(string text, int charIndex, char addedChar)
    {
        return char.IsDigit(addedChar) ? addedChar : '\0';
    }

    void HideKeypad()
    {
        if (!isKeypadActive) return;

        if (keypadPanel != null)
        {
            keypadPanel.SetActive(false);
        }

        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (crosshair != null) crosshair.SetActive(true);

        isKeypadActive = false;
    }

    bool CanOpen()
    {
        if (isOpen) return false;
        if (!requiresKeycard) return true;
        if (requiredItem == ItemType.GuardKey)
        {
            return GlobalInventory.hasGuardKey;
        }
        return GlobalInventory.HasSpecificItem(requiredItem);
    }

    bool HasSequenceProgress()
    {
        return ReadNotebook.HasSequenceCompleted(requiredSequenceKey);
    }

    void OpenSafe()
    {
        HideAllInteractionUI();

        HideKeypad();

        if (safeDoorObject != null)
        {
            var anim = safeDoorObject.GetComponent<Animation>();
            if (anim != null)
            {
                anim.Play(openAnimationName);
            }
        }

        if (openSound != null) openSound.Play();

        isOpen = true;
    }
}
