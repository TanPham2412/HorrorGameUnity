using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RestroomMusicTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool disableAfterSafeCardSequence = true;
    [SerializeField] private string safeCardFlagKey = "SafeCardSequenceCompleted";

    private Collider triggerCollider;
    private bool triggerDisabled;

    private void Reset()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        if (disableAfterSafeCardSequence && !string.IsNullOrWhiteSpace(safeCardFlagKey) &&
            StoryFlagManager.HasFlag(safeCardFlagKey))
        {
            DisableTrigger();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerDisabled) return;
        if (!other.CompareTag(playerTag)) return;

        AmbientMusicManager.Instance?.EnterRestroomZone();
    }

    private void OnTriggerExit(Collider other)
    {
        if (triggerDisabled) return;
        if (!other.CompareTag(playerTag)) return;

        AmbientMusicManager.Instance?.ExitRestroomZone();
    }

    public void DisableTrigger()
    {
        triggerDisabled = true;
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
        AmbientMusicManager.Instance?.ExitRestroomZone();
    }
}
