using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image fillImage; // 빨간 Fill의 Image

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHpChanged += HandleHpChanged;
            HandleHpChanged(playerHealth.CurrentHP, playerHealth.MaxHP); // 초기 반영
        }
        else
        {
            return;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHpChanged -= HandleHpChanged;
        }
        else
        {
            return;
        }
    }

    private void HandleHpChanged(int current, int max)  
    {
        float t = (max <= 0) ? 0f : (float)current / max;
        fillImage.fillAmount = t;
    }
}
