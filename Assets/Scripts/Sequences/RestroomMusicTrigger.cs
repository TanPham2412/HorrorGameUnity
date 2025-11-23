using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RestroomMusicTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string playerTag = "Player";
    public bool disableAfterSafeCardSequence = true;
    public string safeCardFlagKey = "SafeCardSequenceCompleted";

    Collider triggerCollider;

    void Reset()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!CanPlayRestroomMusic()) return;
        AmbientMusicManager.Instance?.EnterRestroomZone();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        AmbientMusicManager.Instance?.ExitRestroomZone();
    }

    bool CanPlayRestroomMusic()
    {
        if (!disableAfterSafeCardSequence) return true;
        if (string.IsNullOrWhiteSpace(safeCardFlagKey)) return true;
        return !StoryFlagManager.HasFlag(safeCardFlagKey);
    }
}
