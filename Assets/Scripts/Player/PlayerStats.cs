using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int baseAttack = 0; //기본 공격력
    [SerializeField] private int baseDefense = 0; //기본 방어력
    [SerializeField] private int baseMoveSpeed = 0; //기본 이동속도 (0이면 100% 유지, 양수면 증가, 음수면 감소)

    [Header("Refs")]
    [SerializeField] private EquipmentManager equipment;

    public event Action OnStatsChanged;

    // 최종 공격력
    public int FinalAttack
    {
        get
        {
            int equip = equipment != null ? equipment.GetTotalStat(StatType.Attack) : 0;
            return baseAttack + equip;
        }
    }

    //최종 방어력
    public int FinalDefense
    {
        get
        {
            int equip = equipment != null ? equipment.GetTotalStat(StatType.Defense) : 0;
            return baseDefense + equip;
        }
    }

    // 최종 이동속도
    public int FinalMoveSpeed
    {
        get
        {
            int equip = equipment != null ? equipment.GetTotalStat(StatType.MoveSpeed) : 0;
            return baseMoveSpeed + equip;
        }
    }

    // 공격력 5당 데미지 +1
    public int DamageBonusFromAttack
    {
        get
        {
            return FinalAttack / 5;
        }
    }

    // 방어력 5당 받는 피해 -1
    public int DamageReductionFromDefense
    {
        get
        {
            return FinalDefense / 5;
        }
    }

    // 이동속도 1당 ±1%
    public float MoveSpeedMultiplier
    {
        get
        {
            float multiplier = 1f + (FinalMoveSpeed * 0.01f);

            // 너무 느려지는 것 방지
            return Mathf.Max(0.1f, multiplier);
        }
    }


    private void OnEnable()
    {
        if (equipment != null)
            equipment.OnChanged += HandleEquipmentChanged;
    }

    private void OnDisable()
    {
        if (equipment != null)
            equipment.OnChanged -= HandleEquipmentChanged;
    }

    private void HandleEquipmentChanged()
    {
        OnStatsChanged?.Invoke();
    }
}