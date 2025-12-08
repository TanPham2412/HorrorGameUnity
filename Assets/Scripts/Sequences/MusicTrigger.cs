using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MusicTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool disableColliderAfterTrigger = true;

    [Header("Music Settings")]
    [SerializeField] private AudioSource machineRoomMusicSource;
    [SerializeField] private float musicDelaySeconds = 3f;
    [SerializeField] private bool loopUntilSuccessVideo = true;

    [Header("Door Handling")]
    [SerializeField] private DoorKey doorToClose;
    [SerializeField] private float doorCloseDelaySeconds = 2f;
    [SerializeField] private float doorKnockDelaySeconds = 5f;
    [SerializeField] private AudioSource doorKnockAudioSource;

    private Collider triggerCollider;
    private bool hasTriggered;
    private static readonly System.Collections.Generic.HashSet<AudioSource> ActiveSources = new();
    private Coroutine pendingPlayRoutine;
    private Coroutine doorCloseRoutine;
    private Coroutine doorKnockRoutine;

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

        pendingPlayRoutine = StartCoroutine(PlayMachineRoomMusicAfterDelay());
        doorCloseRoutine = StartCoroutine(CloseDoorAfterDelay());

        if (disableColliderAfterTrigger && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    private System.Collections.IEnumerator PlayMachineRoomMusicAfterDelay()
    {
        if (musicDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(musicDelaySeconds);
        }

        PlayMachineRoomMusic();
        pendingPlayRoutine = null;
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

    private System.Collections.IEnumerator CloseDoorAfterDelay()
    {
        if (doorCloseDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(doorCloseDelaySeconds);
        }

        CloseAndDisableDoor();
        doorCloseRoutine = null;

        if (doorKnockAudioSource != null)
        {
            doorKnockRoutine = StartCoroutine(PlayDoorKnockAfterDelay());
        }
    }

    private System.Collections.IEnumerator PlayDoorKnockAfterDelay()
    {
        if (doorKnockDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(doorKnockDelaySeconds);
        }

        PlayDoorKnockAudio();
        doorKnockRoutine = null;
    }

    private void CloseAndDisableDoor()
    {
        if (doorToClose == null)
        {
            return;
        }

        doorToClose.ForceCloseAndDisableInteraction();
    }

    private void PlayDoorKnockAudio()
    {
        if (doorKnockAudioSource == null)
        {
            return;
        }

        doorKnockAudioSource.Stop();
        doorKnockAudioSource.Play();
    }

    private void HandleSuccessVideoStarted()
    {
        if (pendingPlayRoutine != null)
        {
            StopCoroutine(pendingPlayRoutine);
            pendingPlayRoutine = null;
        }
        if (doorCloseRoutine != null)
        {
            StopCoroutine(doorCloseRoutine);
            doorCloseRoutine = null;
        }
        if (doorKnockRoutine != null)
        {
            StopCoroutine(doorKnockRoutine);
            doorKnockRoutine = null;
        }
        StopAudio(machineRoomMusicSource);
    }

    private void OnDestroy()
    {
        if (pendingPlayRoutine != null)
        {
            StopCoroutine(pendingPlayRoutine);
            pendingPlayRoutine = null;
        }
        if (doorCloseRoutine != null)
        {
            StopCoroutine(doorCloseRoutine);
            doorCloseRoutine = null;
        }
        if (doorKnockRoutine != null)
        {
            StopCoroutine(doorKnockRoutine);
            doorKnockRoutine = null;
        }
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
