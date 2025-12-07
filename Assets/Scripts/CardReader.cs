using UnityEngine;

public class CardReader : MonoBehaviour
{
    [Header("UI References")]
    public float TheDistance;
    public GameObject ActionDisplay;
    public GameObject ActionText;
    public GameObject NameObject;
    public GameObject NoCardText;
    public GameObject ExtraCross;

    [Header("Reader Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private bool consumeCardOnUse = false;
    [SerializeField] private bool openDoorImmediately = true;
    [SerializeField] private string successMonologue;
    [SerializeField] private float successMonologueDuration = 3f;
    [SerializeField] private bool successAddsToLog = false;

    [Header("Dependencies")]
    public DoorKey doorToUnlock;

    private bool readerUnlocked;
    private bool hasShownUnlockedPrompt;

    private void Awake()
    {
        readerUnlocked = doorToUnlock != null && doorToUnlock.IsUnlocked;
    }

    private void Update()
    {
        TheDistance = PlayerCasting.DistanceFromTarget;
    }

    private void OnMouseOver()
    {
        bool inRange = TheDistance <= interactionRange;

        if (!inRange)
        {
            HideUI();
            return;
        }

        NameObject?.SetActive(true);

        readerUnlocked = readerUnlocked || (doorToUnlock != null && doorToUnlock.IsUnlocked);

        if (readerUnlocked)
        {
            if (!hasShownUnlockedPrompt)
            {
                HideUI();
                hasShownUnlockedPrompt = true;
            }
            return;
        }

        bool hasCard = GlobalInventory.hasMachineRoomCard;

        if (hasCard)
        {
            ActionDisplay?.SetActive(true);
            ActionText?.SetActive(true);
            ExtraCross?.SetActive(true);
            NoCardText?.SetActive(false);
        }
        else
        {
            NoCardText?.SetActive(true);
            ActionDisplay?.SetActive(false);
            ActionText?.SetActive(false);
            ExtraCross?.SetActive(false);
        }

        if (hasCard && Input.GetButtonDown("Action"))
        {
            StartReaderSequence();
        }
    }

    private void OnMouseExit()
    {
        HideUI();
    }

    private void StartReaderSequence()
    {
        HideUI();

        readerUnlocked = true;

        if (consumeCardOnUse)
        {
            GlobalInventory.hasMachineRoomCard = false;
            if (GlobalInventory.currentRegularItem == ItemType.MachineRoomCard)
            {
                GlobalInventory.ClearCurrentItem(ItemType.MachineRoomCard);
            }
        }

        if (!string.IsNullOrWhiteSpace(successMonologue))
        {
            float duration = successMonologueDuration > 0 ? successMonologueDuration : 3f;
            MonologueManager.PlayMonologue(successMonologue, duration, successAddsToLog, true);
        }

        if (doorToUnlock != null)
        {
            doorToUnlock.UnlockDoorExternally(openDoorImmediately);
        }
    }

    private void HideUI()
    {
        NameObject?.SetActive(false);
        ExtraCross?.SetActive(false);
        ActionDisplay?.SetActive(false);
        ActionText?.SetActive(false);
        NoCardText?.SetActive(false);
    }
}
