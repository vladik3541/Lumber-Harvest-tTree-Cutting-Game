using UnityEngine;

public class BootstrapGame : MonoBehaviour
{
    public GameManager Manager;
    public PlayerDamageSystem playerDamageSystem;
    void Awake()
    {
        Manager.Initialize();
        playerDamageSystem.Initialize();
    }
}
