using UnityEngine;
using UnityEngine.UI;

public class EnemyHpBarUI : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Image fillImage;

    private void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnHpChanged += HandleHpChanged;
            HandleHpChanged(enemyHealth.CurrentHp, enemyHealth.MaxHp);
        }
        else
        {
            return;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnHpChanged -= HandleHpChanged;
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
