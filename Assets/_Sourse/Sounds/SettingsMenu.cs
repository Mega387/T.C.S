using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider musicSlider;
    public Dropdown resolutionDropdown;
    public Toggle vSyncToggle;
    public GameObject pauseMenuPanel;

    private Resolution[] resolutions;
    private bool isPaused = false;

    private float lastEscapeTime = -999f;
    private bool waitingForDoublePress = false;
    private float doublePressDelay = 2f;

    void Start()
    {
        if (resolutionDropdown != null)
        {
            SetupResolutionDropdown();
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (vSyncToggle != null)
        {
            vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        }

        LoadAllSettings();

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
                waitingForDoublePress = false;
            }
            else
            {
                float currentTime = Time.unscaledTime;

                if (currentTime - lastEscapeTime <= doublePressDelay)
                {
                    PauseGame();
                    waitingForDoublePress = false;
                }
                else
                {
                    lastEscapeTime = currentTime;
                    waitingForDoublePress = true;
                    StartCoroutine(ResetDoublePress());
                }
            }
        }
    }

    private IEnumerator ResetDoublePress()
    {
        yield return new WaitForSecondsRealtime(doublePressDelay);
        waitingForDoublePress = false;
    }

    public void OpenMenu()
    {
        if (!isPaused)
        {
            PauseGame();
            waitingForDoublePress = false;
        }
    }

    void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnResolutionChanged(int index)
    {
        if (resolutions != null && index < resolutions.Length)
        {
            Screen.SetResolution(resolutions[index].width, resolutions[index].height, FullScreenMode.FullScreenWindow);
            PlayerPrefs.SetInt("ResolutionWidth", resolutions[index].width);
            PlayerPrefs.SetInt("ResolutionHeight", resolutions[index].height);
        }
    }

    public void OnVSyncChanged(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        PlayerPrefs.SetInt("VSync", isOn ? 1 : 0);
    }

    public void OnMusicVolumeChanged(float value)
    {
        MusicPlayer musicPlayer = FindAnyObjectByType<MusicPlayer>();
        if (musicPlayer != null)
        {
            musicPlayer.SetVolume(value);
        }
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    void LoadAllSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        if (musicSlider != null)
        {
            musicSlider.value = savedVolume;
            OnMusicVolumeChanged(savedVolume);
        }

        int savedVSync = PlayerPrefs.GetInt("VSync", 1);
        if (vSyncToggle != null)
        {
            vSyncToggle.isOn = savedVSync == 1;
            OnVSyncChanged(savedVSync == 1);
        }

        if (resolutionDropdown != null && resolutions != null && resolutions.Length > 0)
        {
            int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.width);
            int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.height);

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == savedWidth && resolutions[i].height == savedHeight)
                {
                    resolutionDropdown.value = i;
                    OnResolutionChanged(i);
                    break;
                }
            }
        }
    }
}