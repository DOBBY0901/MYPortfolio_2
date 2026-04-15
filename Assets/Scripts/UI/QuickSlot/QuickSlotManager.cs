using UnityEngine;

public class QuickSlotManager : MonoBehaviour
{
    public const int SlotCount = 5;
    public int CurrentIndex { get; private set; } = 0;

    // 퀵슬롯에는 "아이템 ID"만 저장(저장/로드 안전)
    [SerializeField] private string[] slotItemIds = new string[SlotCount];

    public string GetItemId(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return null;
        return slotItemIds[slotIndex];
    }

    public void SetItemId(int slotIndex, string itemId)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return;
        slotItemIds[slotIndex] = itemId;
        Debug.Log($"[QuickSlot] Set slot {slotIndex + 1} = {itemId}");
    }

    public void Clear(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return;
        slotItemIds[slotIndex] = null;
    }
   
    public string GetCurrentItemId()
    {
        return GetItemId(CurrentIndex);
    }

    public void ScrollSelect(int direction)
    {
        if (direction == 0) return;

        CurrentIndex += direction;

        if (CurrentIndex >= SlotCount) CurrentIndex = 0;
        else if (CurrentIndex < 0) CurrentIndex = SlotCount - 1;
    }
}