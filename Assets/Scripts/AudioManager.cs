using UnityEngine;

public class AudioManager : MonoBehaviour
{   
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private AudioSource EffectAudioSource;
    [SerializeField] private AudioClip backgroundClip;
    [SerializeField] private AudioClip JumpClip;
    [SerializeField] private AudioClip CoinClip;
    [SerializeField] private AudioClip GameOverClip;
    [SerializeField] private AudioClip GameWinClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayBackgroundAudio();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayBackgroundAudio()
    {
        backgroundAudioSource.clip = backgroundClip;
        backgroundAudioSource.Play();
    }
    public void playcoinsound()
    {
        EffectAudioSource.PlayOneShot(CoinClip);
        }
        public void playjumpsound()
    {
        EffectAudioSource.PlayOneShot(JumpClip);
        }
    public void playgameoversound()
    {
        EffectAudioSource.PlayOneShot(GameOverClip);
        }
    public void playgamewinsound()
    {
        EffectAudioSource.PlayOneShot(GameWinClip);
        }
        
}


