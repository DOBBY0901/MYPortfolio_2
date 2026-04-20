using StarterAssets;
using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 100; //최대체력

    [Header("Die")]
    private bool isDead = false;
    [SerializeField] private AudioClip deathSfx;          // 사망 사운드
    [Range(0f, 1f)][SerializeField] private float deathVolume = 0.9f;

    [Header("Stats")]
    [SerializeField] private PlayerStats playerStats;

    private StarterAssets.ThirdPersonController controller;

    public int CurrentHP { get; private set; }

    public int MaxHP => maxHP;
    public float NormalizedHP => maxHP <= 0 ? 0f : (float)CurrentHP / maxHP;
    private int dieHash;

    public event Action<int, int> OnHpChanged; // (current, max)
    private Animator animator;
    private PlayerHitReaction hitReaction;
    private PlayerCombatState combatState;
    
    private void Awake()
    {
        CurrentHP = maxHP;
        animator = GetComponentInChildren<Animator>();
        dieHash = Animator.StringToHash("Die");

        hitReaction = GetComponent<PlayerHitReaction>();
        combatState = GetComponent<PlayerCombatState>();
        OnHpChanged?.Invoke(CurrentHP, maxHP); // 시작 시 UI 초기화

        controller = GetComponent<StarterAssets.ThirdPersonController>();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0, maxHP);
        OnHpChanged?.Invoke(CurrentHP, maxHP);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || controller._isRolling) return;

        int defenseReduction = playerStats != null ? playerStats.DamageReductionFromDefense : 0;

        // 방어력 5당 피해 1 감소
        // 방어력이 음수면 defenseReduction도 음수가 되므로 추가 피해가 됨
        int finalDamage = damage - defenseReduction;

        // 최소 1 데미지는 받도록
        finalDamage = Mathf.Max(1, finalDamage);

        CurrentHP = Mathf.Clamp(CurrentHP - finalDamage, 0, maxHP);
        // HP 변경 알림
        OnHpChanged?.Invoke(CurrentHP, maxHP);

        Debug.Log($"입력 데미지={damage}, 방어보정={defenseReduction}, 최종 데미지={finalDamage}");

        if (CurrentHP <= 0)
        {
            Die();
            return;
        }

        // 피격
        hitReaction?.PlayHitFeedback(transform.position);
        combatState?.EnterCombat();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        var controller = GetComponent<StarterAssets.ThirdPersonController>();
        if (controller != null)
            controller.enabled = false;

        // 1) 사망 사운드
        if (deathSfx != null)
            AudioManager.Instance?.Play3DSfx(deathSfx, transform.position, deathVolume);

        // 2) 애니메이션
        if (animator != null)
            animator.SetTrigger(dieHash);

        

    }
}
