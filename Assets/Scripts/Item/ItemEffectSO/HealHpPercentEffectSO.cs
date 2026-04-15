using UnityEngine;

[CreateAssetMenu(menuName = "Items/Effects/Heal HP Percent")]
public class HealHpPercentEffectSO : ItemEffectSO
{
    [Range(0f, 1f)]
    [SerializeField] private float healHpPercent;

    public override bool Apply(GameObject user)
    {
        if (user == null) return false;

        var hp = user.GetComponent<PlayerHealth>();
        if (hp == null) return false;

        // 사망 시  사용 불가로 처리
        if (hp.CurrentHP <= 0) return false;

        // 체력이 100%면 사용불가
        if (hp.CurrentHP >= hp.MaxHP) return false;

        int amount = Mathf.RoundToInt(hp.MaxHP * healHpPercent);
        hp.Heal(amount);
        return true;
    }
}