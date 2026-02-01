using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap - єдина точка входу в гру
/// Завжди запускай гру з цієї сцени!
/// </summary>
public class Bootstrap : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private bool loadGameSceneOnStart = true;
    
    [Header("Loading Settings")]
    [SerializeField] private float minimumLoadTime = 1f;
    [SerializeField] private bool showLoadingScreen = true;
    
    [Header("Save Settings")]
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool createNewGameIfNoSave = true;

    private void Start()
    {
        Debug.Log("🚀 Bootstrap: Ініціалізація гри...");
        
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        // 1. Ініціалізуємо системи
        yield return InitializeSystems();
        
        // 2. Перевіряємо збереження
        bool hasSave = SaveLoadManager.Instance.HasSaveFile();
        
        if (hasSave && autoLoadOnStart)
        {
            Debug.Log("💾 Знайдено збереження - завантажуємо...");
            // Завантажуємо дані (будуть застосовані після завантаження сцени)
        }
        else if (!hasSave && createNewGameIfNoSave)
        {
            Debug.Log("🆕 Збереження не знайдено - починаємо нову гру");
        }
        
        // 3. Завантажуємо ігрову сцену
        if (loadGameSceneOnStart)
        {
            yield return LoadGameScene(hasSave && autoLoadOnStart);
        }
        
        Debug.Log("✅ Bootstrap: Ініціалізація завершена!");
    }

    private IEnumerator InitializeSystems()
    {
        Debug.Log("⚙️ Ініціалізація систем...");
        
        // Перевіряємо/створюємо SaveLoadManager
        if (SaveLoadManager.Instance == null)
        {
            GameObject saveManager = new GameObject("SaveLoadManager");
            saveManager.AddComponent<SaveLoadManager>();
            Debug.Log("✅ SaveLoadManager створено");
        }
        
        // Перевіряємо GameManager
        if (GameManager.Instance == null)
        {
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();
            Debug.Log("✅ GameManager створено");
        }
        
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator LoadGameScene(bool shouldLoadSave)
    {
        float startTime = Time.time;
        
        Debug.Log($"🎮 Завантаження сцени: {gameSceneName}");
        
        // Завантажуємо сцену асинхронно
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
        asyncLoad.allowSceneActivation = false;
        
        // Чекаємо поки завантажується
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            // Логування прогресу
            if (asyncLoad.progress >= 0.9f)
            {
                // Дочекуємось мінімального часу завантаження
                float elapsed = Time.time - startTime;
                if (elapsed >= minimumLoadTime)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }
            
            yield return null;
        }
        
        Debug.Log("✅ Сцена завантажена");
        
        // Чекаємо один кадр щоб сцена проініціалізувалась
        yield return new WaitForEndOfFrame();
        
        // Якщо потрібно - завантажуємо збереження
        if (shouldLoadSave)
        {
            yield return new WaitForSeconds(0.2f); // Даємо час об'єктам створитись
            SaveLoadManager.Instance.LoadGame();
        }
        else
        {
            // Ініціалізуємо нову гру
            GameManager.Instance?.InitializeNewGame();
        }
    }

    // Для перезапуску гри через Bootstrap
    public static void RestartGame()
    {
        SceneManager.LoadScene(0); // Bootstrap завжди індекс 0
    }
}
