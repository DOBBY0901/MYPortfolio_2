using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCombatAI : MonoBehaviour
{
    private enum State 
    {
        Idle, 
        Chase,
        Attack 
    }

    [SerializeField] private State state = State.Idle;

    [Header("Refs")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyMove scout; 

    [Header("Detect")]
    [SerializeField] private float detectRange = 10f; //탐지범위
    [SerializeField] private float viewAngle = 90f;   //시야각
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private float targetHeight = 1.2f;

    [SerializeField] private LayerMask playerMask;     // Player 레이어
    [SerializeField] private LayerMask obstacleMask;   // Obstacle 레이어

    [Header("Combat")]
    [SerializeField] private float attackRange = 2.0f; //공격범위
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float faceSpeed = 10f;

    [Header("Hit")]
    [SerializeField] private Transform hitOrigin;   // 공격 판정 중심(손/무기 근처)
    [SerializeField] private float hitRadius = 0.6f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask playerHitMask; // Player 레이어만


    private float lastAttackTime;
    private int attackHash;
    private string speedParam = "Speed";
    private int speedHash;
    private bool isDead; //사망 확인

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (scout == null) scout = GetComponent<EnemyMove>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        attackHash = Animator.StringToHash("Attack");
        speedHash = Animator.StringToHash(speedParam);
    }

    private void Update()
    {
        if (player == null || agent == null || animator == null || isDead == true) return;

        // 기본 상태에서 탐지되면 추적으로 전환
        if (state == State.Idle)
        {
            if (DetectPlayerRaycast())
            {
                // 순찰 끄기
                if (scout != null) scout.enabled = false;

                state = State.Chase;
            }
            return;
        }

        // 추적 / 공격
        float dist = Vector3.Distance(transform.position, player.position);

        if (state == State.Chase)
        {
            // 애니메이션(걷기) 연동 : 현재 속도를 파라미터로 넣기
            if (animator != null)
            {
                // agent.velocity.magnitude는 실제 이동 속도
                animator.SetFloat(speedHash, agent.velocity.magnitude);
            }

            // 공격 범위면 공격 상태
            if (dist <= attackRange)
            {
                state = State.Attack;
                agent.ResetPath();
                return;
            }

            // 계속 추적
            agent.SetDestination(player.position);

            //추적 중에도 시야를 잃으면 포기하고 순찰로 복귀 - 추후 확장가능성 있음
            //if (!DetectPlayerRaycast()) ReturnToWander();
        }
        else if (state == State.Attack)
        {

            // 애니메이션(걷기) 연동 : 현재 속도를 파라미터로 넣기
            if (animator != null)
            {
                // agent.velocity.magnitude는 실제 이동 속도
                animator.SetFloat(speedHash, agent.velocity.magnitude);
            }
            FacePlayer();

            // 범위 벗어나면 다시 추적
            if (dist > attackRange)
            {
                state = State.Chase;
                return;
            }

            // 쿨타임마다 공격 트리거
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                animator.ResetTrigger(attackHash);
                animator.SetTrigger(attackHash);
            }
        }
    }

    //레이캐스트 방식으로 플레이어 정찰
    private bool DetectPlayerRaycast()
    {
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = player.position + Vector3.up * targetHeight;

        Vector3 dir = targetPos - eyePos;
        float dist = dir.magnitude;

        // 거리
        if (dist > detectRange) return false;

        // 시야각
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        // 가림 체크 (Player 또는 Obstacle만 맞게)
        int mask = playerMask | obstacleMask;
        if (Physics.Raycast(eyePos, dir.normalized, out RaycastHit hit, detectRange, mask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    //플레이어 쪽으로 시선고정
    private void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * faceSpeed);
    }

    //사망
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 이동 완전 중단
        if (agent != null)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        // 순찰 스크립트도 같이 끔
        EnemyMove wander = GetComponent<EnemyMove>();
        if (wander != null)
            wander.enabled = false;
    }

    //공격
    public void AttackHit()
    {
        if (isDead) return;
        if (player == null) return;

        Vector3 center = (hitOrigin != null) ? hitOrigin.position : (transform.position + transform.forward * 1.0f + Vector3.up * 1.0f);

        // 범위 안 플레이어 찾기
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerHitMask, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0) return;

        // 한 번만 맞추기(가장 가까운 플레이어)
        PlayerHealth ph = hits[0].GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
        }
    }

    //데미지를 입을시
    public void OnDamage(Transform attacker)
    {
        if (isDead) return;

        if (attacker != null)
            player = attacker; // 공격자를 타겟으로 지정

        state = State.Chase;

        // 순찰 중이었다면 끄기
        if (scout != null)
            scout.enabled = false;
    }
    //시야 잃으면 순찰 복귀 - 확장 가능성 있음
    private void ReturnToWander()
    {
        state = State.Idle;
        agent.ResetPath();
        if (scout != null) scout.enabled = true;
    }

    //강제 추적
    public void ForceChase(Transform target)
    {
        if (isDead) return;
        if (target == null) return;
        if (agent == null) return;

        player = target;

        // 순찰/배회 로직 끄기
        if (scout != null)
            scout.enabled = false;

        // 공격 중이거나 idle이어도 강제로 추적 상태로 전환
        state = State.Chase;

        // NavMeshAgent가 살아 있으면 바로 목적지 지정
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(player.position);
        }

        // 혹시 공격 애니메이션 잔상 방지용
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.2f, left * detectRange);
        Gizmos.DrawRay(transform.position + Vector3.up * 0.2f, right * detectRange);

        if (hitOrigin != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(hitOrigin.position, hitRadius);
        }
    }

#endif
}
