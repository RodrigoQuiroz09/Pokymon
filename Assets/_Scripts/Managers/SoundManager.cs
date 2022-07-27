using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource effectsSource, musicSource;
    public Vector2 pitchRange = Vector2.zero;
    public static SoundManager SharedInstance;
    [SerializeField] private AudioClip[] characterSounds;

    private void Awake()
    {
        if (SharedInstance != null) Destroy(gameObject);
        else SharedInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(AudioClip clip)
    {
        effectsSource.pitch = 1;
        effectsSource.Stop();
        effectsSource.clip = clip;
        effectsSource.Play();
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlayRandomCharacterSound()

    {
        RandomSoundEffect(characterSounds);
    }

    void RandomSoundEffect(params AudioClip[] clips)
    {
        int index = Random.Range(0, clips.Length);
        float pitch = Random.Range(pitchRange.x, pitchRange.y);

        effectsSource.pitch = pitch;
        PlaySound(clips[index]);
    }

}
