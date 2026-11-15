using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    Running,
    Paused,
    Victory,
    GameOver
}

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game rules")]
    [SerializeField] private float fallHeight = -15f;
    [SerializeField] private Transform player;

    private readonly HashSet<Collectible> registeredCollectibles = new HashSet<Collectible>();
    private readonly HashSet<Collectible> collectedItems = new HashSet<Collectible>();
    private int score;
    private int totalCollectibles;
    private int collectedCount;
    private GameState state = GameState.Running;

    public event Action<int> ScoreChanged;
    public event Action<int, int> CollectibleCountChanged;
    public event Action<GameState> StateChanged;

    public GameState State => state;
    public int Score => score;
    public int TotalCollectibles => totalCollectibles;
    public int CollectedCount => collectedCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Only one GameManager can be active in a scene.", this);
            enabled = false;
            return;
        }

        Instance = this;
        if (player == null)
        {
            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        state = GameState.Running;
        StateChanged?.Invoke(state);
        ScoreChanged?.Invoke(score);
        CollectibleCountChanged?.Invoke(collectedCount, totalCollectibles);
    }

    private void Update()
    {
        if (player != null && state == GameState.Running && player.position.y < fallHeight)
        {
            TryLose();
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (state == GameState.Running)
            {
                PauseGame();
            }
            else if (state == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }

    public void RegisterCollectible(Collectible collectible)
    {
        if (collectible == null || collectedItems.Contains(collectible) ||
            !registeredCollectibles.Add(collectible))
        {
            return;
        }

        totalCollectibles++;
        NotifyCollectibleCountChanged();
    }

    public void UnregisterCollectible(Collectible collectible)
    {
        if (collectible == null || collectedItems.Contains(collectible) ||
            !registeredCollectibles.Remove(collectible))
        {
            return;
        }

        totalCollectibles = Mathf.Max(0, totalCollectibles - 1);
        NotifyCollectibleCountChanged();
    }

    public bool TryCollect(Collectible collectible, int points)
    {
        if (state != GameState.Running || collectible == null ||
            !registeredCollectibles.Contains(collectible) || !collectedItems.Add(collectible))
        {
            return false;
        }

        collectedCount++;
        score += Mathf.Max(0, points);
        ScoreChanged?.Invoke(score);
        NotifyCollectibleCountChanged();

        return true;
    }

    public void TryWin()
    {
        if (state != GameState.Running)
        {
            return;
        }

        SetState(GameState.Victory);
    }

    public void TryLose()
    {
        if (state != GameState.Running)
        {
            return;
        }

        SetState(GameState.GameOver);
    }

    public void PauseGame()
    {
        if (state == GameState.Running)
        {
            SetState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (state == GameState.Paused)
        {
            SetState(GameState.Running);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
            return;
        }

        if (!string.IsNullOrEmpty(activeScene.name))
        {
            SceneManager.LoadScene(activeScene.name);
            return;
        }

        Debug.LogError("Cannot restart because the active scene has no valid name.", this);
    }

    private void SetState(GameState newState)
    {
        state = newState;
        if (state == GameState.Running)
        {
            Time.timeScale = 1f;
        }
        else if (state == GameState.Paused || state == GameState.Victory || state == GameState.GameOver)
        {
            Time.timeScale = 0f;
        }

        if (state == GameState.Victory || state == GameState.GameOver)
        {
            Rigidbody playerBody = player != null
                ? player.GetComponent<Rigidbody>() ?? player.GetComponentInParent<Rigidbody>()
                : null;
            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
            }
        }

        StateChanged?.Invoke(state);
    }

    private void NotifyCollectibleCountChanged()
    {
        CollectibleCountChanged?.Invoke(collectedCount, totalCollectibles);
    }
}
