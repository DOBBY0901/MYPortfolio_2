using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static UnityEditor.PlayerSettings;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 100;
    private int hp;
    private bool isDead;

    public int CurrentHp => hp;
    public int MaxHp => maxHp;
    public event Action<int, int> OnHpChanged;

    [Header("Death")]
    [SerializeField] private float despawnDelay = 3f;     // 시체가 사라질 시간
    [SerializeField] private AudioClip deathSfx;          // 사망 사운드
    [Range(0f, 1f)][SerializeField] private float deathVolume = 0.9f;

    private EnemyHitReaction hitReaction;
    private Animator animator;
    private EnemyCombatAI combatAI;
    private int dieHash;

    private Collider[] colliders;

    private void Awake()
    {
        hp = maxHp;

        hitReaction = GetComponent<EnemyHitReaction>();
        animator = GetComponentInChildren<Animator>();
        dieHash = Animator.StringToHash("Die");

        colliders = GetComponentsInChildren<Collider>();
        combatAI = GetComponent<EnemyCombatAI>();

        OnHpChanged?.Invoke(hp, maxHp); //초기 UI 변경
    }

    public void TakeDamage(int damage, Vector3 attackerPos,Transform attacker)
    {
        if (isDead) return;

        hp = Mathf.Clamp(hp - damage, 0, maxHp);
        // 피격 피드백
        hitReaction?.PlayHitFeedback(attackerPos);

        OnHpChanged?.Invoke(hp, maxHp); //UI 갱신

        combatAI?.OnDamage(attacker); //공격자 위치 찾기
        if (hp <= 0)
        {
            Die();    
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 5) 이동, 순찰 전부 제거 - 위로 올림.
        combatAI.Die();

        GetComponent<EnemyDropToInventory>()?.DropOnce();

        // 1) 사망 사운드
        PlayDieSfx(transform.position);

        // 2) 애니메이션
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hit");
            animator.SetTrigger(dieHash);
        }

        if (hitReaction != null) hitReaction.enabled = false;
        
        // 3) 콜라이더 제거
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        // 4) 일정 시간 후 제거
        StartCoroutine(DespawnRoutine());

   
        //6) 미니맵 아이콘 제거
        if (MinimapEnemyIconManager.Instance != null)
        {
            MinimapEnemyIconManager.Instance.UnregisterEnemy(transform);
        }

    }
    private void PlayDieSfx(Vector3 pos)
    {
        if (deathSfx == null) return;

        AudioManager.Instance?.Play3DSfx(deathSfx, pos, deathVolume);
    }
    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(despawnDelay);
        Destroy(gameObject);
    }
}
