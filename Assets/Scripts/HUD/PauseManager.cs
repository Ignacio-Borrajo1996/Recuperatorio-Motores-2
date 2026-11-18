using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;

    [Header("Audio Mixer Settings")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string volumeParam = "PausedVolume";
    [SerializeField] private string lowpassParam = "PausedLowpass";

    [Header("Wall Effect Values")]
    [SerializeField] private float normalVolume = 0f;
    [SerializeField] private float pausedVolume = -12f;
    [SerializeField] private float normalCutoff = 22000f;
    [SerializeField] private float pausedCutoff = 700f;
    [SerializeField] private float transitionSpeed = 8f;

    private bool isPaused = false;
    private float targetVolume;
    private float targetCutoff;
    private float currentVolume;
    private float currentCutoff;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        currentVolume = normalVolume;
        targetVolume = normalVolume;
        currentCutoff = normalCutoff;
        targetCutoff = normalCutoff;

        ApplyAudioValues();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.Instance != null && (UIManager.Instance.IsWin || UIManager.Instance.GameOver))
            {
                return;
            }

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.unscaledDeltaTime * transitionSpeed);
        currentCutoff = Mathf.Lerp(currentCutoff, targetCutoff, Time.unscaledDeltaTime * transitionSpeed);
        ApplyAudioValues();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (Movement.Instance != null)
        {
            Movement.Instance.canMove = false;
            Movement.Instance.StopInteractions = true;
        }

        targetVolume = pausedVolume;
        targetCutoff = pausedCutoff;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Movement.Instance != null)
        {
            Movement.Instance.canMove = true;
            Movement.Instance.StopInteractions = false;
        }

        targetVolume = normalVolume;
        targetCutoff = normalCutoff;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        ApplyInstantNormalAudio();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        ApplyInstantNormalAudio();
        SceneManager.LoadScene("Main Menu");
    }

    private void ApplyAudioValues()
    {
        if (mainMixer == null) return;
        mainMixer.SetFloat(volumeParam, currentVolume);
        mainMixer.SetFloat(lowpassParam, currentCutoff);
    }

    private void ApplyInstantNormalAudio()
    {
        if (mainMixer == null) return;
        mainMixer.SetFloat(volumeParam, normalVolume);
        mainMixer.SetFloat(lowpassParam, normalCutoff);
    }
}
