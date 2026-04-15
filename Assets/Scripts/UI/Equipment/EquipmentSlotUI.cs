using UnityEngine;

public class EquipmentSlotUI : MonoBehaviour
{
    [SerializeField] private EquipmentSlot slotType;
    [SerializeField] private EquipmentSelectPopup popup;

    public void OnClickSlot()
    {
        if (popup == null) return;
        popup.Open(slotType);
    }
}