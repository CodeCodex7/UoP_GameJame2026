using UnityEngine;

public class SettingsUtility : MonoService<SettingsUtility>
{
    [SerializeField] private bool applyWindowedDefaultOnAwake = true;
    [SerializeField] private int defaultWindowWidth = 1024;
    [SerializeField] private int defaultWindowHeight = 768;

    private void Awake()
    {
        RegisterService();

        if (applyWindowedDefaultOnAwake)
        {
            ApplyWindowedDefault();
        }
    }

    private void OnDestroy()
    {
        UnregisterService();
    }

    public bool IsFullscreen()
    {
        return Screen.fullScreen;
    }

    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    public void EnableFullscreen()
    {
        SetFullscreen(true);
    }

    public void DisableFullscreen()
    {
        SetFullscreen(false);
    }

    public void ToggleFullscreen()
    {
        SetFullscreen(!Screen.fullScreen);
    }

    public void SetFullscreenMode(int modeIndex)
    {
        if (!System.Enum.IsDefined(typeof(FullScreenMode), modeIndex))
        {
            Debug.LogWarning($"Cannot set fullscreen mode. {modeIndex} is not a valid FullScreenMode value.");
            return;
        }

        Screen.fullScreenMode = (FullScreenMode)modeIndex;
    }

    public void SetExclusiveFullscreen()
    {
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        Screen.fullScreen = true;
    }

    public void SetBorderlessFullscreen()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.fullScreen = true;
    }

    public void SetWindowed()
    {
        Screen.fullScreenMode = FullScreenMode.Windowed;
        Screen.fullScreen = false;
    }

    public void ApplyWindowedDefault()
    {
        if (defaultWindowWidth <= 0 || defaultWindowHeight <= 0)
        {
            SetWindowed();
            return;
        }

        Screen.SetResolution(defaultWindowWidth, defaultWindowHeight, FullScreenMode.Windowed);
    }

    public void SetResolution(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            Debug.LogWarning($"Cannot set resolution to {width}x{height}.");
            return;
        }

        Screen.SetResolution(width, height, Screen.fullScreenMode);
    }
}
