using UnityEngine;

public class Inventory : MonoBehaviour
{
    public const int SlotCount = 25;
    public event System.Action OnChanged;

    [System.Serializable]
    public struct Slot
    {
        public string id;
        public int count;
        public bool IsEmpty => string.IsNullOrEmpty(id) || count <= 0;
    }

    [SerializeField] private Slot[] slots = new Slot[SlotCount];
    public Slot[] Slots => slots;

    private void Reset()
    {
        slots = new Slot[SlotCount];
    }

    // 아이템 추가
    public bool AddItem(ItemDatabaseSO db, string id, int amount)
    {
        if (db == null || string.IsNullOrEmpty(id) || amount <= 0) return false;

        var data = db.Get(id);
        if (data == null) return false;

        bool changed = false;

        // 1) 스택 가능이면 기존 스택에 먼저 채우기
        if (data.Stackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty) continue;
                if (slots[i].id != id) continue;

                int space = data.MaxStack - slots[i].count;
                if (space <= 0) continue;

                int add = Mathf.Min(space, amount);
                slots[i].count += add;
                amount -= add;

                changed = true;

                if (amount <= 0)
                {
                    if (changed) OnChanged?.Invoke();
                    return true;
                }
            }
        }

        // 2) 빈 슬롯에 넣기
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty) continue;

            int add = data.Stackable ? Mathf.Min(data.MaxStack, amount) : 1;
            slots[i].id = id;
            slots[i].count = add;
            amount -= add;

            changed = true;

            if (amount <= 0)
            {
                if (changed) OnChanged?.Invoke();
                return true;
            }
        }

        // 공간 부족
        if (changed) OnChanged?.Invoke();
        return false;
    }

    // 아이템 제거
    public bool RemoveItem(string id, int amount)
    {
        if (string.IsNullOrEmpty(id) || amount <= 0) return false;

        int remaining = amount;
        bool changed = false;
        bool needCompact = false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty) continue;
            if (slots[i].id != id) continue;

            int take = Mathf.Min(slots[i].count, remaining);
            if (take <= 0) continue;

            slots[i].count -= take;
            remaining -= take;
            changed = true;

            if (slots[i].count <= 0)
            {
                slots[i].id = null;
                slots[i].count = 0;
                needCompact = true;
            }

            if (remaining <= 0)
            {
                if (needCompact)
                    CompactSlots();

                if (changed)
                    OnChanged?.Invoke();

                return true;
            }
        }

        if (needCompact)
            CompactSlots();

        if (changed)
            OnChanged?.Invoke();

        return false;
    }

    //소비 아이템 사용
    public bool UseItem(string id, int amount)
    {
        return RemoveItem(id, amount);
    }

    //아아템 수량 가져오기
    public int GetTotalCount(string id)
    {
        if (string.IsNullOrEmpty(id)) return 0;

        int total = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty) continue;
            if (slots[i].id != id) continue;
            total += slots[i].count;
        }
        return total;
    }

    //아이템 슬롯 당기기
    private void CompactSlots()
    {
        int writeIndex = 0;

        for (int readIndex = 0; readIndex < slots.Length; readIndex++)
        {
            if (slots[readIndex].IsEmpty)
                continue;

            if (readIndex != writeIndex)
            {
                slots[writeIndex] = slots[readIndex];
                slots[readIndex].id = null;
                slots[readIndex].count = 0;
            }

            writeIndex++;
        }

        for (int i = writeIndex; i < slots.Length; i++)
        {
            slots[i].id = null;
            slots[i].count = 0;
        }
    }
}