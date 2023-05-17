using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip collision, win;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }
    public void PlayOneShotSFX(string sfxName)
    {
        switch (sfxName)
        {
            case "Collision":
                audioSource.PlayOneShot(collision);
                break;
            case "Win":
                audioSource.PlayOneShot(win);
                break;
        }
    }
}
