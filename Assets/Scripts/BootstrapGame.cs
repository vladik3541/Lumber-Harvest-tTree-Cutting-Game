using UnityEngine;

public class BootstrapGame : MonoBehaviour
{
    public GameManager Manager;
    public PlayerDamageSystem playerDamageSystem;

    void Awake()
    {
        Debug.Log("[BootstrapGame] Awake");
        Manager.Initialize();
        playerDamageSystem.Initialize();
        Debug.Log($"[BootstrapGame] GameManager.Instance = {GameManager.Instance}");
        Debug.Log($"[BootstrapGame] WorldSpawner.Instance = {WorldSpawner.Instance}");
        Debug.Log($"[BootstrapGame] SaveLoadManager.Instance = {SaveLoadManager.Instance}");
    }

    void Start()
    {
        Debug.Log("[BootstrapGame] Start");
        Debug.Log($"[BootstrapGame] WorldSpawner.Instance = {WorldSpawner.Instance}");

        bool hasSaveManager = SaveLoadManager.Instance != null;
        bool hasSaveFile = hasSaveManager && SaveLoadManager.Instance.HasSaveFile();
        Debug.Log($"[BootstrapGame] hasSaveManager={hasSaveManager}, hasSaveFile={hasSaveFile}");

        if (hasSaveFile)
        {
            Debug.Log("[BootstrapGame] Save found -> LoadGame()");
            SaveLoadManager.Instance.LoadGame();
        }
        else
        {
            Debug.Log("[BootstrapGame] No save -> InitializeNewGame()");
            GameManager.Instance.InitializeNewGame();
        }
    }
}
