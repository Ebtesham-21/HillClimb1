using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    // --- THIS IS THE FIX ---
    // Add [SerializeField] to make these private variables show up in the Inspector.
    [SerializeField] private AudioSource bgmSource;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundMusic;

    public static event Action<bool> OnMuteStateChanged;

    private bool isMuted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // It's better to call setup after the singleton is established.
        SetupBGM();
        LoadMuteState();
        UpdateMuteState(); // Apply the loaded state immediately
    }

    private void SetupBGM()
    {
        // Safety check to prevent errors if you forget to assign the source
        if (bgmSource == null)
        {
            Debug.LogError("BGM AudioSource is not assigned on the AudioManager!");
            return;
        }

        bgmSource.clip = backgroundMusic;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void ToggleSound()
    {
        isMuted = !isMuted;
        SaveMuteState();
        UpdateMuteState();
    }

    private void UpdateMuteState()
    {
        // Safety check
        if (bgmSource != null)
        {
            bgmSource.mute = isMuted;
        }
        
        OnMuteStateChanged?.Invoke(isMuted);
    }

    private void SaveMuteState()
    {
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadMuteState()
    {
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
    }
}