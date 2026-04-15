using UnityEngine;

public class PlayerCombatState : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float exitDelay = 3f;

    private float exitTimer;
    private bool isCombat;

    private int isCombatHash;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        isCombatHash = Animator.StringToHash("IsCombat");
    }

    private void Update()
    {
        if (!isCombat) return;

        exitTimer -= Time.deltaTime;
        if (exitTimer <= 0f)
            ExitCombat();
    }

    // ⭐ 공격 / 피격 / 스킬 등 전투 행동 발생 시 호출
    public void EnterCombat()
    {
        if (!isCombat)
        {
            animator.SetBool(isCombatHash, true);
            isCombat = true;
        }

        exitTimer = exitDelay;
    }

    public void ExitCombat()
    {
        isCombat = false;
        animator.SetBool(isCombatHash, false);
    }

}
