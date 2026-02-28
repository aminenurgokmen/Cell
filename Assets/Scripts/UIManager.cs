using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    void Awake()
    {
        Instance = this;
    }
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI _scoreText;
    public TMPro.TextMeshProUGUI highScoreText; // Inspector'dan bağla
    private int destroyedCount = 0;
    private int highScore = 0;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateScoreText();
        UpdateHighScoreText();
    }

    public void AddDestroyedCount(int count)
    {
        destroyedCount += count;
        UpdateScoreText();

        if (destroyedCount > highScore)
        {
            highScore = destroyedCount;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateHighScoreText();
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = destroyedCount.ToString();
        if (_scoreText != null)
            _scoreText.text = "Score: " + destroyedCount.ToString();
    }

    void UpdateHighScoreText()
    {
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore.ToString();
    }

    public GameObject failPanel; // Inspector'dan bağla

    public void OnGameFail()
    {
        // Hafif kamera sarsıntısı
        Camera.main.transform.DOShakePosition(0.4f, 0.15f, 12, 90f, false)
            .OnComplete(() =>
            {
                // 1 saniye sonra fail paneli aç
                DOVirtual.DelayedCall(1f, () =>
                {
                    if (failPanel != null)
                        failPanel.SetActive(true);
                    scoreText.gameObject.SetActive(false);
                });
            });
    }
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
