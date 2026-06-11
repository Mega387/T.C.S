using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [Header("Music List")]
    public List<AudioClip> playlist;

    [Header("Settings")]
    public bool shuffleMode = false;
    public float fadeInDuration = 10f;
    public float defaultVolume = 0.75f;

    private AudioSource audioSource;
    private int currentTrackIndex = 0;
    private bool isPlaying = false;
    private Coroutine currentFadeRoutine = null;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (playlist != null && playlist.Count > 0)
        {
            if (shuffleMode)
            {
                ShufflePlaylist();
            }
            StartMusic();
        }
    }

    void Update()
    {
        if (isPlaying && !audioSource.isPlaying && playlist != null && playlist.Count > 0)
        {
            NextTrack();
        }
    }

    public void StartMusic()
    {
        if (playlist != null && playlist.Count > 0 && !isPlaying)
        {
            isPlaying = true;
            PlayTrackWithFade(playlist[currentTrackIndex]);
        }
    }

    public void StopMusic()
    {
        if (isPlaying)
        {
            isPlaying = false;
            if (currentFadeRoutine != null)
            {
                StopCoroutine(currentFadeRoutine);
                currentFadeRoutine = null;
            }
            audioSource.Stop();
        }
    }

    public void NextTrack()
    {
        if (!isPlaying || playlist == null || playlist.Count == 0) return;

        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = null;
        }

        currentTrackIndex++;
        if (currentTrackIndex >= playlist.Count)
        {
            if (shuffleMode)
            {
                ShufflePlaylist();
                currentTrackIndex = 0;
            }
            else
            {
                currentTrackIndex = 0;
            }
        }

        PlayTrackWithFade(playlist[currentTrackIndex]);
    }

    public void PreviousTrack()
    {
        if (!isPlaying || playlist == null || playlist.Count == 0) return;

        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = null;
        }

        currentTrackIndex--;
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = playlist.Count - 1;
        }

        PlayTrackWithFade(playlist[currentTrackIndex]);
    }

    private void PlayTrackWithFade(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.volume = 0f;
        audioSource.clip = clip;
        audioSource.Play();

        currentFadeRoutine = StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        float time = 0f;
        while (time < fadeInDuration)
        {
            time += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, defaultVolume, time / fadeInDuration);
            yield return null;
        }

        audioSource.volume = defaultVolume;
        currentFadeRoutine = null;
    }

    public void SetVolume(float volume)
    {
        defaultVolume = volume;
        if (audioSource.isPlaying && currentFadeRoutine == null)
        {
            audioSource.volume = volume;
        }
    }

    private void ShufflePlaylist()
    {
        for (int i = 0; i < playlist.Count; i++)
        {
            AudioClip temp = playlist[i];
            int randomIndex = Random.Range(i, playlist.Count);
            playlist[i] = playlist[randomIndex];
            playlist[randomIndex] = temp;
        }
    }
}