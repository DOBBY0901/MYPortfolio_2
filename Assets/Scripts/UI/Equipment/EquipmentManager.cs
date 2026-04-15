using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private ItemDatabaseSO database;
    [SerializeField] private Inventory inventory;

    private readonly Dictionary<EquipmentSlot, string> equipped = new();

    public event Action OnChanged;

    public string GetEquippedId(EquipmentSlot slot)
        => equipped.TryGetValue(slot, out var id) ? id : null;

    // 장착 -> 인벤에서 1개 제거하고 슬롯에 저장
    public bool TryEquip(string itemId)
    {
        if (database == null || inventory == null) return false;

        var data = database.Get(itemId);
        if (data == null) return false;
        if (data.Category != ItemCategory.Equipment) return false;

        // 인벤에 있어야 함
        if (inventory.GetTotalCount(itemId) <= 0) return false;

        var slot = data.EquipSlot;

        // 이미 장착된 게 있으면 해제 후 교체
        if (equipped.TryGetValue(slot, out var oldId) && !string.IsNullOrEmpty(oldId))
        {
            Unequip(slot);
        }

        // 인벤에서 제거
        if (!inventory.RemoveItem(itemId, 1)) return false;

        equipped[slot] = itemId;
        OnChanged?.Invoke();
        return true;
    }

    // 해제 -> 인벤으로 되돌리기
    public bool Unequip(EquipmentSlot slot)
    {
        if (!equipped.TryGetValue(slot, out var itemId)) return false;
        if (string.IsNullOrEmpty(itemId)) return false;

        // 인벤 공간 확인하면서 다시 넣기
        bool add = inventory.AddItem(database, itemId, 1);
        if (!add) return false;

        equipped[slot] = null;
        OnChanged?.Invoke();
        return true;
    }

    //스탯 계산 -> 장착된 아이템들의 스탯 합산
    public int GetTotalStat(StatType statType)
    {
        if (database == null) return 0;

        int total = 0;

        foreach (var pair in equipped)
        {
            string itemId = pair.Value;

            if (string.IsNullOrEmpty(itemId))
                continue;

            var data = database.Get(itemId);
            if (data == null)
                continue;

            var stats = data.EquipStats;
            if (stats == null || stats.Length == 0)
                continue;

            for (int i = 0; i < stats.Length; i++)
            {
                if (stats[i].statType != statType)
                    continue;

                total += stats[i].value;
            }
        }

        return total;
    }
}