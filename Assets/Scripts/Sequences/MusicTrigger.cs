using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MusicTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool disableColliderAfterTrigger = true;

    [Header("Music Settings")]
    [SerializeField] private AudioSource machineRoomMusicSource;
    [SerializeField] private bool loopUntilSuccessVideo = true;

    [Header("Door Handling")]
    [SerializeField] private DoorKey doorToClose;

    private Collider triggerCollider;
    private bool hasTriggered;
    private static readonly System.Collections.Generic.HashSet<AudioSource> ActiveSources = new();

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
    }

    private void OnEnable()
    {
        MagicActive.SuccessVideoStarted += HandleSuccessVideoStarted;
    }

    private void OnDisable()
    {
        MagicActive.SuccessVideoStarted -= HandleSuccessVideoStarted;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        hasTriggered = true;

        PlayMachineRoomMusic();
        CloseAndDisableDoor();

        if (disableColliderAfterTrigger && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    private void PlayMachineRoomMusic()
    {
        if (machineRoomMusicSource == null)
        {
            return;
        }

        machineRoomMusicSource.loop = loopUntilSuccessVideo;
        machineRoomMusicSource.Play();
        ActiveSources.Add(machineRoomMusicSource);
    }

    private void CloseAndDisableDoor()
    {
        if (doorToClose == null)
        {
            return;
        }

        doorToClose.ForceCloseAndDisableInteraction();
    }

    private void HandleSuccessVideoStarted()
    {
        StopAudio(machineRoomMusicSource);
    }

    private static void StopAudio(AudioSource source)
    {
        if (source == null) return;
        source.loop = false;
        source.Stop();
        ActiveSources.Remove(source);
    }

    public static void StopAllMachineRoomAudio()
    {
        if (ActiveSources.Count == 0) return;

        foreach (var source in ActiveSources)
        {
            if (source == null) continue;
            source.loop = false;
            source.Stop();
        }

        ActiveSources.Clear();
    }
}
