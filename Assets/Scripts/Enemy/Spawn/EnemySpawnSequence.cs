using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnSequence : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyCombatAI combatAI;
    [SerializeField] private EnemyMove enemyMove;
    [SerializeField] private CapsuleCollider bodyCollider;

    [Header("Spawn Animation")]
    [SerializeField] private string spawnTriggerName = "Spawn";
    [SerializeField] private float spawnDuration = 4.833f;


    [Header("SFX")]
    [SerializeField] private AudioClip spawnSfx;

    private int spawnHash;

    private void Awake() 
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (combatAI == null) combatAI = GetComponent<EnemyCombatAI>();
        if (enemyMove == null) enemyMove = GetComponent<EnemyMove>();
        if (bodyCollider == null) bodyCollider = GetComponent<CapsuleCollider>();

        spawnHash = Animator.StringToHash(spawnTriggerName);
    }

    private void OnEnable()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        PlaySpawnSfx(transform.position);

        // 1. AI/이동 끔
        if (combatAI != null) combatAI.enabled = false;
        if (enemyMove != null) enemyMove.enabled = false;

        if (agent != null)
        {
            if (agent.isOnNavMesh)
                agent.isStopped = true;

            agent.enabled = false;
        }

        // 2. 등장 애니메이션 재생
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("Die");
            animator.SetTrigger(spawnHash);
        }

        // 3. 애니메이션 끝날 때까지 대기
        yield return new WaitForSeconds(spawnDuration);

        // 4. NavMeshAgent 다시 켬
        if (agent != null)
        {
            agent.enabled = true;

            // 다시 켰을 때 NavMesh 위에 있는지 체크
            if (agent.isOnNavMesh)
                agent.isStopped = false;
        }

        // 5. AI/이동 활성화
        if (enemyMove != null) enemyMove.enabled = true;
        if (combatAI != null) combatAI.enabled = true;
    }

    private void PlaySpawnSfx(Vector3 pos)
    {
        if (spawnSfx == null) return;

        AudioManager.Instance?.Play3DSfx(spawnSfx, pos);
    }
}