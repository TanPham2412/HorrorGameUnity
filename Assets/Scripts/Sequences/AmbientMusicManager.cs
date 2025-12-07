using UnityEngine;

public class AmbientMusicManager : MonoBehaviour
{
    public static AmbientMusicManager Instance { get; private set; }

    [Header("Ambient Music")]
    public AudioSource ambientSource;
    public AudioClip ambientClip;
    [Range(0f, 1f)] public float ambientVolume = 0.7f;

    [Header("Restroom Music")]
    public AudioSource restroomSource;
    public AudioClip restroomClip;
    [Range(0f, 1f)] public float restroomVolume = 0.7f;
    public bool restroomLoop = true;

    private bool restroomMusicDisabled = false;
    private bool restroomMusicStarted = false;
    private bool restroomMusicPaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ConfigureAmbientSource();
        ConfigureRestroomSource();
    }

    private void ConfigureAmbientSource()
    {
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
        }

        ambientSource.clip = ambientClip;
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        ambientSource.spatialBlend = 0f;
        ambientSource.playOnAwake = false;

        if (ambientClip != null)
        {
            ambientSource.Play();
        }
    }

    private void ConfigureRestroomSource()
    {
        if (restroomSource == null)
        {
            restroomSource = gameObject.AddComponent<AudioSource>();
        }

        restroomSource.clip = restroomClip;
        restroomSource.loop = restroomLoop;
        restroomSource.volume = restroomVolume;
        restroomSource.spatialBlend = 0f;
        restroomSource.playOnAwake = false;
    }

    public void EnterRestroomZone()
    {
        if (restroomMusicDisabled || restroomSource == null || restroomClip == null)
        {
            return;
        }

        if (!restroomMusicStarted)
        {
            restroomSource.Play();
            restroomMusicStarted = true;
            restroomMusicPaused = false;
            return;
        }

        if (restroomMusicPaused)
        {
            restroomSource.UnPause();
            restroomMusicPaused = false;
            return;
        }

        if (!restroomSource.isPlaying)
        {
            restroomSource.Play();
        }
    }

    public void ExitRestroomZone()
    {
        if (!restroomMusicStarted || restroomSource == null)
        {
            return;
        }

        if (restroomSource.isPlaying)
        {
            restroomSource.Pause();
            restroomMusicPaused = true;
        }
    }

    public void DisableRestroomMusic()
    {
        if (restroomSource != null)
        {
            restroomSource.Stop();
        }

        restroomMusicDisabled = true;
        restroomMusicStarted = false;
        restroomMusicPaused = false;
    }
}
