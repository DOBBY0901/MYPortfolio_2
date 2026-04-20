using UnityEngine;

public class PlayerHitReaction   : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTriggerName = "Hit"; // 적 Animator에 Hit 트리거가 있으면 사용

    [Header("VFX")]
    [SerializeField] private GameObject hitVfxPrefab;   // 파티클 프리팹(선택)
    [SerializeField] private Transform vfxPoint;            // 파티클 생성 위치(없으면 루트)

    [Header("SFX")]
    [SerializeField] private AudioClip[] hitSfx;
    [SerializeField] private float pitchMin = 0.95f;
    [SerializeField] private float pitchMax = 1.05f;
    [SerializeField] private float hitVolume = 0.8f;


    private int hitHash;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        hitHash = Animator.StringToHash(hitTriggerName);

        if (vfxPoint == null) vfxPoint = transform;


    }

    public void PlayHitFeedback(Vector3 attackerPos)
    {
        // 1) 애니메이션
        if (animator != null && !string.IsNullOrEmpty(hitTriggerName))
            animator.SetTrigger(hitHash);

        // 2) VFX
        if (hitVfxPrefab != null)
        {
            GameObject vfx = Instantiate(hitVfxPrefab, vfxPoint.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        // 3) SFX
        if (hitSfx != null)
        {
            AudioManager.Instance?.PlayRandom3DSfx(
                hitSfx,
                transform.position,
                hitVolume,
                pitchMin,
                pitchMax
            );
        }
    }
   
}

