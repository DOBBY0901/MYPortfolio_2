using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MonoBehaviour
{
    [Header("Scout")]
    [SerializeField] private float scoutRadius = 6f;     // 동굴 안에서 돌아다닐 반경(조절)
    [SerializeField] private float chooseNewDestination = 3f;   // 몇 초마다 새 목적지
    [SerializeField] private float arriveDistance = 0.6f; // 도착 판정
    [SerializeField] private float waitMin = 0.6f; //대기 시간 최소
    [SerializeField] private float waitMax = 1.8f; //대기 시간 최대

    [Header("Animation")]
    [SerializeField] private Animator animator;
  

    private float waitTimer = 0f;
    private string speedParam = "Speed"; // Animator 파라미터 이름
    private NavMeshAgent agent;
    private float timer;
    private Vector3 origin;

    private int speedHash;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>(true);

        origin = transform.position;
        speedHash = Animator.StringToHash(speedParam);
    }

    private void Start()
    {
        PickNewDestination();
        timer = chooseNewDestination;
    }

    private void Update()
    {
        // 애니메이션(걷기) 연동 : 현재 속도를 파라미터로 넣기
        if (animator != null)
        {
            // agent.velocity.magnitude는 실제 이동 속도
            animator.SetFloat(speedHash, agent.velocity.magnitude);
        }
        bool arrived = !agent.pathPending && agent.remainingDistance <= arriveDistance;

        // 도착했을 때: 대기 처리
        if (arrived)
        {
            // 대기 시간 설정
            if (waitTimer <= 0f)
            {
                waitTimer = Random.Range(waitMin, waitMax);
            }

            waitTimer -= Time.deltaTime;

            // 대기 중엔 아무 것도 안 함 (이동 로직만 멈춤)
            if (waitTimer > 0f)
                return;

            // 대기 끝 → 다음 목적지
            PickNewDestination();
            timer = chooseNewDestination;
            return;
        }

        // 이동 중일 때만 타이머 감소
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            PickNewDestination();
            timer = chooseNewDestination;
        }
    }

    private void PickNewDestination()
    {
        if (!agent.isOnNavMesh) return;

        for (int i = 0; i < 12; i++)
        {
            Vector2 rand = Random.insideUnitCircle * scoutRadius;
            Vector3 candidate = origin + new Vector3(rand.x, 0f, rand.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // 플레이 중이면 origin 사용, 아니면 현재 위치 기준
        Vector3 center = Application.isPlaying ? origin : transform.position;

        Gizmos.DrawWireSphere(center, scoutRadius);
    }
#endif
}
