using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MonologueManager : MonoBehaviour
{
    public static MonologueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject textBox;
    [SerializeField] private TextMeshProUGUI textLabel;

    [Header("Timing Settings")]
    [SerializeField] private float defaultDisplayDuration = 4f;

    private readonly Queue<MonologueRequest> queue = new();
    private readonly HashSet<string> playedLines = new();
    private Coroutine playbackCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (textBox != null && textLabel == null)
        {
            textLabel = textBox.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (textBox != null)
        {
            textBox.SetActive(false);
        }
    }

    public static void PlayMonologue(string text, float duration = -1f, bool addToLog = false, bool preventDuplicate = true)
    {
        if (Instance == null)
        {
            Debug.LogWarning("MonologueManager is not present in the scene.");
            return;
        }

        Instance.EnqueueMonologue(text, duration, addToLog, preventDuplicate);
    }

    private void EnqueueMonologue(string text, float duration, bool addToLog, bool preventDuplicate)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        if (preventDuplicate && playedLines.Contains(text)) return;

        if (preventDuplicate)
        {
            playedLines.Add(text);
        }

        queue.Enqueue(new MonologueRequest
        {
            text = text,
            duration = duration > 0f ? duration : defaultDisplayDuration,
            addToLog = addToLog
        });

        if (playbackCoroutine == null)
        {
            playbackCoroutine = StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        while (queue.Count > 0)
        {
            var request = queue.Dequeue();

            if (textBox != null) textBox.SetActive(true);
            if (textLabel != null) textLabel.text = request.text;

            if (request.addToLog)
            {
                ClueLogManager.AddMonologue(request.text);
            }

            yield return new WaitForSeconds(request.duration);
        }

        if (textLabel != null) textLabel.text = string.Empty;
        if (textBox != null) textBox.SetActive(false);
        playbackCoroutine = null;
    }

    private class MonologueRequest
    {
        public string text;
        public float duration;
        public bool addToLog;
    }
}
