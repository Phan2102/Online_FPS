using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class UIShooter : MonoBehaviour
{
    public Image[] HealthIndicators;

    private HPHandler hpHandler;
    private int lastHealth = 5;
    private NetworkRunner runner; // Thêm biến runner

    private void Start()
    {
        // Lấy NetworkRunner từ NetworkRunnerCallback
        runner = FindObjectOfType<NetworkRunner>();
        if (runner == null)
            return;

        NetworkObject localPlayer = runner.GetPlayerObject(runner.LocalPlayer);
        if (localPlayer != null)
        {
            hpHandler = localPlayer.GetComponent<HPHandler>();
            hpHandler.OnHealthChanged += UpdateHealthUI;
            UpdateHealthUI(hpHandler.HP);
        }
    }

    private void UpdateHealthUI(byte currentHealth)
    {
        for (int i = 0; i < HealthIndicators.Length; i++)
        {
            HealthIndicators[i].gameObject.SetActive(i < currentHealth);
        }
        lastHealth = currentHealth;
    }

    private void OnDestroy()
    {
        if (hpHandler != null)
        {
            hpHandler.OnHealthChanged -= UpdateHealthUI;
        }
    }
}