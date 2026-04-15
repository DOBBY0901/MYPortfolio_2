using UnityEngine;

public class PlayerAttackHit : MonoBehaviour
{
    [Header("Hit Area")]
    [SerializeField] private Transform hitOrigin;   // 캐릭터 앞 기준점
    [SerializeField] private float hitRange = 1.8f; // 앞쪽 거리
    [SerializeField] private float hitRadius = 0.7f; // 판정 반경
    [SerializeField] private LayerMask enemyLayers;

    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("Hit Enemy Count")]
    [SerializeField] private int maxHits = 16; // 한 번 공격에 처리할 최대 적 수(안전장치)
    private readonly Collider[] _results = new Collider[32];

    [Header("Stats")]
    [SerializeField] private PlayerStats playerStats;

    private void Awake()
    {
        if (hitOrigin == null)
        {
            hitOrigin = transform;
        }
    }

    //Attack 애니메이션 이벤트로 호출
    public void DoHit()
    {
        Vector3 center = hitOrigin.position + hitOrigin.forward * (hitRange * 0.5f);

        int count = Physics.OverlapSphereNonAlloc(center, hitRadius, _results, enemyLayers, QueryTriggerInteraction.Ignore);

        if (count <= 0) return;

        int applied = 0;
        int bonusDamage = playerStats != null ? playerStats.DamageBonusFromAttack : 0;
        int finalDamage = Mathf.Max(1, damage + bonusDamage);

        for (int i = 0; i < count; i++)
        {
            if (_results[i] == null) continue;
           
            // 같은 적에 여러 콜라이더가 있을 수 있으니 부모에서 EnemyHealth 찾기
            var hp = _results[i].GetComponentInParent<EnemyHealth>();
            if (hp == null) continue;

            hp.TakeDamage(finalDamage, transform.position, gameObject.transform);

            applied++;
            if (applied >= maxHits) break; // 너무 많으면 끊기
        }

    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (hitOrigin == null) return;
        Vector3 center = hitOrigin.position + hitOrigin.forward * (hitRange * 0.5f);
        Gizmos.DrawWireSphere(center, hitRadius);
    }
#endif
}
