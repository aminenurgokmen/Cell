using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    void Awake()
    {
        Instance = this;
    }
    public TMPro.TextMeshProUGUI scoreText;
    private int destroyedCount = 0;

    void Start()
    {
        UpdateScoreText();
    }

    public void AddDestroyedCount(int count)
    {
        destroyedCount += count;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = destroyedCount.ToString();
    }

    public GameObject failPanel; // Inspector'dan bağla

    public void OnGameFail()
    {
        if (failPanel != null)
            failPanel.SetActive(true);
    }
}
