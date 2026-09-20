using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip bossMusic;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)]
    private float musicVolume = 0.5f;

    private AudioSource audioSource;

    private AudioClip currentMusic;

    private void Awake()
    {
        // Prevent duplicate MusicManagers
        // when changing scenes.
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep this MusicManager alive
        // when changing scenes.
        DontDestroyOnLoad(gameObject);

        audioSource =
            GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = musicVolume;
    }

    private void Start()
    {
        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    public void PlayBossMusic()
    {
        PlayMusic(bossMusic);
    }

    private void PlayMusic(AudioClip newMusic)
    {
        if (newMusic == null)
        {
            Debug.LogWarning(
                "[MusicManager] Music clip is missing."
            );

            return;
        }

        // Don't restart the same music
        // when changing between scenes.
        if (
            currentMusic == newMusic &&
            audioSource.isPlaying
        )
        {
            return;
        }

        currentMusic = newMusic;

        audioSource.clip =
            newMusic;

        audioSource.volume =
            musicVolume;

        audioSource.Play();
    }

    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        currentMusic = null;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume =
            Mathf.Clamp01(volume);

        if (audioSource != null)
        {
            audioSource.volume =
                musicVolume;
        }
    }
}