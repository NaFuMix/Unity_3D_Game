
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int targetGems = 20;
    public float gameTime = 120f;

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text gemCountText;
    public TMP_Text resultText;

    private int collectedGems = 0;
    private float remainingTime;
    private GameAudio gameAudio;

    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        remainingTime = gameTime;
        IsGameOver = false;
        gameAudio = GetComponent<GameAudio>();
        if (gameAudio == null)
            gameAudio = gameObject.AddComponent<GameAudio>();

        resultText.text = "";

        UpdateUI();
        if (GetComponent<HeistWorld>() == null)
            gameObject.AddComponent<HeistWorld>();
    }

    void Update()
    {
        if (IsGameOver) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0)
        {
            remainingTime = 0;

            if (collectedGems >= targetGems)
                WinGame();
            else
                LoseGame();
        }

        if (!IsGameOver)
            gameAudio.SetTimeRemaining(remainingTime);

        UpdateUI();
    }

    public void CollectGem()
    {
        if (IsGameOver) return;

        collectedGems++;

        UpdateUI();

        if (collectedGems >= targetGems)
        {
            WinGame();
        }
        else
        {
            gameAudio?.PlayPickup();
        }
    }

    void UpdateUI()
    {
        timerText.text =
            "Time: " + Mathf.CeilToInt(remainingTime);

        gemCountText.text =
            "Gems: " + collectedGems + " / " + targetGems;
    }

    void WinGame()
    {
        IsGameOver = true;
        resultText.text = "YOU WIN!";
        gameAudio?.PlayResult(true);
    }

    void LoseGame()
    {
        IsGameOver = true;
        resultText.text = "TIME UP! TRY AGAIN";
        gameAudio?.PlayResult(false);
    }
}
