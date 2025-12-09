using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    public float volume = 0.7f;

    private AudioSource _audioSource;
    private static MusicManager _instance;

    private void Awake()
    {
        // Реализуем паттерн Singleton, чтобы музыка не повторялась при загрузке новой сцены
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject); // Музыка сохраняется при смене сцен
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _audioSource.clip = backgroundMusic;
        _audioSource.volume = volume;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;

        if (backgroundMusic != null)
        {
            _audioSource.Play();
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (_audioSource != null)
        {
            _audioSource.volume = volume;
        }
    }

    public void StopMusic()
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
        }
    }

    public void ResumeMusic()
    {
        if (_audioSource != null && !_audioSource.isPlaying)
        {
            _audioSource.Play();
        }
    }
}