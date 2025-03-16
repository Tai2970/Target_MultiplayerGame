using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip shootSound;
    public AudioClip targetHitSound;
    public AudioClip explosionSound;
    public AudioClip tankDestroyedSound;
    public AudioClip gameStartSound;
    public AudioClip gameEndSound;
    public AudioClip timerTickSound;
    public AudioClip tankMoveSound;
    public AudioClip gainPointSound; 

    private AudioSource audioSource;
    private bool isMoving = false; // Track tank movement status

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep AudioManager alive across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    // Function to handle tank movement sound
    public void PlayTankMoveSound(bool moving)
    {
        if (moving && !isMoving)
        {
            isMoving = true;
            audioSource.loop = true;
            audioSource.clip = tankMoveSound;
            audioSource.Play();
        }
        else if (!moving && isMoving)
        {
            isMoving = false;
            audioSource.loop = false;
            audioSource.Stop();
        }
    }
}
