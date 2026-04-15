using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquipmentSelectPopup : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemDatabaseSO database;
    [SerializeField] private EquipmentManager equipment;

    [Header("UI")]
    [SerializeField] private EquipmentPopupSlotUI[] slotUIs; // 25개

    private EquipmentSlot _currentSlot;

    private void Awake()
    {
        // 슬롯 클릭 콜백 바인딩
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] != null)
                slotUIs[i].Bind(OnClickEquip);
        }

        gameObject.SetActive(false); // 시작은 꺼두기
    }

    public void Open(EquipmentSlot slot)
    {
        _currentSlot = slot;

        gameObject.SetActive(true);
        Rebuild();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Rebuild()
    {
        if (inventory == null || database == null || equipment == null) return;

        List<ItemDataSO> candidates = new();

        // 인벤 슬롯 25칸에서 후보 뽑기
        var invenSlots = inventory.Slots;
        for (int i = 0; i < invenSlots.Length; i++)
        {
            if (invenSlots[i].IsEmpty) continue;

            var data = database.Get(invenSlots[i].id);
            if (data == null) continue;

            if (data.Category != ItemCategory.Equipment) continue;
            if (data.EquipSlot != _currentSlot) continue;

            candidates.Add(data);
        }

        // 25칸 채우기
        int fill = Mathf.Min(candidates.Count, slotUIs.Length);

        for (int i = 0; i < fill; i++)
            slotUIs[i].Set(candidates[i]); 

        for (int i = fill; i < slotUIs.Length; i++)
            slotUIs[i].SetEmpty();
    }

    //아이템 장착
    private void OnClickEquip(string itemId)
    {
        bool ok = equipment.TryEquip(itemId);
        if (ok)
            Close();
    }

    //아이템 해제
    public void OnClickUnequip()
    {
        bool ok = equipment.Unequip(_currentSlot);
        if (ok)
            Close();
    }
}