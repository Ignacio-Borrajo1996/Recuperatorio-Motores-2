using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menuinicial : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameplaySceneName = "Nivel1";
    public string secretSceneName = "NivelSecreto";

    [Header("Audio Settings")]
    public AudioSource clickAudioSource;

    [Header("UI Panels (Optional)")]
    public GameObject mainMenuPanel;
    public GameObject levelsPanel;
    public GameObject controlsPanel;

    [Header("Secret Level UI")]
    public GameObject secretLevelButton;

    private void Start()
    {
        int unlockedState = PlayerPrefs.GetInt("SecretLevelUnlocked", 0);
        Debug.Log("SecretLevelUnlocked value: " + unlockedState);

        if (secretLevelButton != null)
        {
            secretLevelButton.SetActive(unlockedState == 1);
        }
    }

    public void PlayGame()
    {
        if (clickAudioSource != null)
        {
            clickAudioSource.Play();
            Invoke("LoadGameplayScene", 0.30f);
        }
        else
        {
            LoadGameplayScene();
        }
    }

    private void LoadGameplayScene()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void PlaySecretLevel()
    {
        if (clickAudioSource != null)
        {
            clickAudioSource.Play();
            Invoke("LoadSecretScene", 0.30f);
        }
        else
        {
            LoadSecretScene();
        }
    }

    private void LoadSecretScene()
    {
        SceneManager.LoadScene(secretSceneName);
    }

    public void ExitGame()
    {
        if (clickAudioSource != null)
        {
            clickAudioSource.Play();
            Invoke("QuitApplication", 0.15f);
        }
        else
        {
            QuitApplication();
        }
    }

    private void QuitApplication()
    {
        Application.Quit();
        Debug.Log("Player exited the game.");
    }
}
