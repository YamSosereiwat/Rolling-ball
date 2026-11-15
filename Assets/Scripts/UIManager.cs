using UnityEngine;
using UnityEngine.UI;

public sealed class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text collectibleText;
    [SerializeField] private Text instructionText;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Text endMessageText;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text restartButtonText;
    [SerializeField] private GameObject pausePanel;

    private void Start()
    {
        Font uiFont = Resources.Load<Font>("Fonts/Lato-Regular");
        if (uiFont == null)
        {
            Debug.LogError("UIManager could not load Assets/Resources/Fonts/Lato-Regular.ttf.", this);
        }
        else
        {
            AssignFont(scoreText, uiFont);
            AssignFont(collectibleText, uiFont);
            AssignFont(instructionText, uiFont);
            AssignFont(endMessageText, uiFont);
            AssignFont(finalScoreText, uiFont);
            AssignFont(restartButtonText, uiFont);
        }

        if (restartButtonText != null)
        {
            restartButtonText.text = "Restart";
        }

        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        if (gameManager == null)
        {
            Debug.LogError("UIManager requires a GameManager in the active scene.", this);
            return;
        }

        gameManager.ScoreChanged += UpdateScore;
        gameManager.CollectibleCountChanged += UpdateCollectibles;
        gameManager.StateChanged += UpdateGameState;

        UpdateScore(gameManager.Score);
        UpdateCollectibles(gameManager.CollectedCount, gameManager.TotalCollectibles);
        UpdateGameState(gameManager.State);
    }

    private void OnDestroy()
    {
        if (gameManager == null)
        {
            return;
        }

        gameManager.ScoreChanged -= UpdateScore;
        gameManager.CollectibleCountChanged -= UpdateCollectibles;
        gameManager.StateChanged -= UpdateGameState;
    }

    public void RestartGame()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    public void ResumeGame()
    {
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
    }

    private static void AssignFont(Text text, Font font)
    {
        if (text != null)
        {
            text.font = font;
        }
    }

    private void UpdateScore(int currentScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }

        if (finalScoreText != null && gameManager != null)
        {
            UpdateFinalResult(currentScore, gameManager.CollectedCount, gameManager.TotalCollectibles);
        }
    }

    private void UpdateCollectibles(int collected, int total)
    {
        if (collectibleText != null)
        {
            collectibleText.text = "Orbs Collected: " + collected + "/" + total;
        }

        if (gameManager != null)
        {
            UpdateFinalResult(gameManager.Score, collected, total);
        }
    }

    private void UpdateFinalResult(int currentScore, int collected, int total)
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + currentScore +
                "\nOrbs Collected: " + collected + "/" + total;
        }
    }

    private void UpdateGameState(GameState newState)
    {
        bool showEndPanel = newState == GameState.Victory || newState == GameState.GameOver;
        if (endPanel != null)
        {
            endPanel.SetActive(showEndPanel);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(newState == GameState.Paused);
        }

        if (endMessageText != null)
        {
            if (newState == GameState.Victory)
            {
                endMessageText.text = "You Win!";
            }
            else if (newState == GameState.GameOver)
            {
                endMessageText.text = "Game Over";
            }
            else
            {
                endMessageText.text = string.Empty;
            }
        }

        if (instructionText != null)
        {
            instructionText.text = "WASD / Arrows: Move   Space: Jump   Left Shift: Boost   Esc: Pause";
        }
    }
}
