using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WolfEncounterSequence : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyCombatAI combatAI;
    [SerializeField] private EnemyMove enemyMove;
    [SerializeField] private Transform player;

    [Header("Animation")]
    [SerializeField] private float howlDuration = 2f; 

    [Header("Rush")]
    [SerializeField] private float rushSpeed = 7f;
    [SerializeField] private float rushDuration = 0.8f;

    [Header("SFX")]
    [SerializeField] private AudioClip howlSfx;

    private int howlHash;
    private int speedHash;
    private bool hasStarted = false;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();    
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (combatAI == null) combatAI = GetComponent<EnemyCombatAI>();
        if (enemyMove == null) enemyMove = GetComponent<EnemyMove>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        howlHash = Animator.StringToHash("Howl");
        speedHash = Animator.StringToHash("Speed");
    }

    private void Start()
    {
        if (combatAI != null) combatAI.enabled = false;
        if (enemyMove != null) enemyMove.enabled = false;

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;
    }

    public void StartEncounter() 
    {
        if (hasStarted) return;
        StartCoroutine(EncounterRoutine());
    }

    private IEnumerator EncounterRoutine()
    {
        hasStarted = true;

        if (combatAI != null) combatAI.enabled = false;
        if (enemyMove != null) enemyMove.enabled = false;
        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        AudioManager.Instance?.Play3DSfx(howlSfx, transform.position, 15f); //늑대 하울링 사운드 출력
        
        animator.SetTrigger("Howl"); //Howl 트리거 실행
        yield return new WaitForSeconds(howlDuration);

        animator.StopPlayback();
        animator.SetFloat("Speed", 1f); //달리기 애니메이션 용 파라미터 제어
        float timer = 0f;

        while (timer < rushDuration)
        {
            timer += Time.deltaTime;
            transform.position += transform.forward * rushSpeed * Time.deltaTime;
            yield return null;
        }

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;

        if (combatAI != null)
        {
            combatAI.enabled = true;
            combatAI.ForceChase(player);
        }

   
    }
}