using System;
using UnityEngine;
/// <summary>
/// Головний менеджер гри
/// Координує всі системи та керує станом гри
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private bool isPaused = false;
    
    [Header("Auto Save")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 120f; // 2 хвилини
    
    private float autoSaveTimer;

    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnNewGameStarted;

    public void Initialize()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // Підписуємось на події збереження
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.OnAfterSave += OnGameSaved;
            SaveLoadManager.Instance.OnAfterLoad += OnGameLoaded;
        }
    }

    private void Update()
    {
        // Автозбереження
        if (enableAutoSave && !isPaused)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                AutoSave();
                autoSaveTimer = 0f;
            }
        }

        // Гарячі клавіші
        HandleHotkeys();
    }

    private void HandleHotkeys()
    {
        // F5 - Швидке збереження
        if (Input.GetKeyDown(KeyCode.F5))
        {
            QuickSave();
        }

        // F9 - Швидке завантаження
        if (Input.GetKeyDown(KeyCode.F9))
        {
            QuickLoad();
        }

        // DELETE - Видалити збереження (ДОДАЙ ЦЕ)
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.DeleteSave();
                Debug.Log("🗑️ Збереження видалено!");
            }
        }

        // ESC - Пауза
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    #region Game Control

    public void InitializeNewGame()
    {
        Debug.Log("[GameManager] InitializeNewGame called");
        Debug.Log($"[GameManager] WorldSpawner.Instance = {WorldSpawner.Instance}");

        if (PlayerUpgradeManager.Instance != null)
            PlayerUpgradeManager.Instance.ResetUpgrades();

        if (WorldSpawner.Instance != null)
            WorldSpawner.Instance.SpawnInitialWorld();
        else
            Debug.LogError("[GameManager] WorldSpawner.Instance is NULL! Is WorldSpawner in the scene?");

        OnNewGameStarted?.Invoke();
    }

    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
            Debug.Log("⏸️ Гра призупинена");
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1f;
            OnGameResumed?.Invoke();
            Debug.Log("▶️ Гра продовжена");
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        Bootstrap.RestartGame();
    }

    #endregion

    #region Save/Load Shortcuts

    public void QuickSave()
    {
        SaveLoadManager.Instance?.SaveGame();
        Debug.Log("💾 Швидке збереження (F5)");
    }

    public void QuickLoad()
    {
        if (SaveLoadManager.Instance?.HasSaveFile() == true)
        {
            SaveLoadManager.Instance.LoadGame();
            Debug.Log("📂 Швидке завантаження (F9)");
        }
        else
        {
            Debug.LogWarning("⚠️ Немає збереження для завантаження");
        }
    }

    private void AutoSave()
    {
        SaveLoadManager.Instance?.SaveGame();
        Debug.Log("💾 Автозбереження");
        
        // Можна додати UI повідомлення
        ShowNotification("Гру збережено");
    }

    #endregion

    #region Event Handlers

    private void OnGameSaved()
    {
        Debug.Log("✅ Гру збережено успішно");
    }

    private void OnGameLoaded(GameSaveData data)
    {
        Debug.Log($"✅ Гру завантажено: {data.saveDateTime}");
    }

    #endregion

    #region Notifications

    private void ShowNotification(string message)
    {
        // TODO: Показати UI повідомлення
        Debug.Log($"📢 {message}");
    }

    #endregion

    private void OnApplicationQuit()
    {
        // Автоматичне збереження при виході
        if (enableAutoSave && SaveLoadManager.Instance != null)
        {
            //SaveLoadManager.Instance.SaveGame();
            Debug.Log("💾 Автозбереження при виході");
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Автозбереження при переключенні додатку (мобілка/Android)
        if (pauseStatus && enableAutoSave && SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
        }
    }
}
