using TMPro;
using UnityEngine;

public class EquipmentStatusPanelUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EquipmentManager equipment;

    [Header("Texts")]
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text moveSpeedText;

    //활성화
    private void OnEnable()
    {
        if (equipment != null)
            equipment.OnChanged += Refresh;

        Refresh();
    }

    //비활성화
    private void OnDisable()
    {
        if (equipment != null)
            equipment.OnChanged -= Refresh;
    }

    //UI 갱신
    public void Refresh()
    {
        if (equipment == null) return;

        int attack = equipment.GetTotalStat(StatType.Attack);
        int defense = equipment.GetTotalStat(StatType.Defense);
        int moveSpeed = equipment.GetTotalStat(StatType.MoveSpeed);

        if (attackText != null)
            attackText.text = $"공격력 : {attack}";

        if (defenseText != null)
            defenseText.text = $"방어력 : {defense}";

        if (moveSpeedText != null)
            moveSpeedText.text = $"이동속도 : {moveSpeed}";
    }
}