using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public bool IsWin = false;
    public bool GameOver = false;

    [Header("Screens")]
    [SerializeField] public GameObject HUD;
    [SerializeField] public GameObject gameOverScreen;
    [SerializeField] public GameObject winScreen;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI livesValueText;
    [SerializeField] private TextMeshProUGUI timeValueText;
    [SerializeField] private Image[] collectibleIcons;

    [Header("Collectible Colors")]
    [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
    [SerializeField] private Color filledColor = Color.white;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (HUD != null) HUD.SetActive(true);
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);
        UpdateCollectiblesUI(0);
    }

    public void UpdateLivesUI(int currentLives)
    {
        if (livesValueText != null)
        {
            livesValueText.text = currentLives.ToString();
        }
    }

    public void UpdateTimeUI(float timeRemaining)
    {
        if (timeValueText == null) return;
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timeValueText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateCollectiblesUI(int collectedAmount)
    {
        for (int i = 0; i < collectibleIcons.Length; i++)
        {
            if (collectibleIcons[i] != null)
            {
                collectibleIcons[i].color = (i < collectedAmount) ? filledColor : emptyColor;
            }
        }
    }

    public void TriggerLose()
    {
        if (GameOver) return;
        GameOver = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (HUD != null) HUD.SetActive(false);
        if (gameOverScreen != null) gameOverScreen.SetActive(true);

        if (Movement.Instance != null)
        {
            Movement.Instance.canMove = false;
            Movement.Instance.StopInteractions = true;
        }
    }
}
