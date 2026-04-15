using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlotAssignPopup : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject root;
    [SerializeField] private QuickSlotManager quickSlot;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private ItemDatabaseSO database;

   
    [Header("QuickSlot UI")]
    [SerializeField] private QuickSlotUI[] slotUIs = new QuickSlotUI[QuickSlotManager.SlotCount];
    [SerializeField] private Inventory inventory;

    public bool IsOpen => root != null && root.activeSelf;

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    //팝업열기
    public void OpenPopup()
    {
        if (root != null) root.SetActive(true);

        //열릴 때 5칸 상태 갱신
        RefreshPopupSlots();
    }

    //팝업닫기
    public void Close()
    {
        if (root != null) root.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen) return;

        //1~5번 키를 눌러 등록.
        if (Keyboard.current.digit1Key.wasPressedThisFrame) Assign(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) Assign(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) Assign(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) Assign(3);
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) Assign(4);

    }
    
    //아이템 등록
    private void Assign(int slotIndex)
    {
        string id = inventoryUI.GetSelectedItemId();
        if (string.IsNullOrEmpty(id)) { Close(); return; }

        var data = database.Get(id);
        if (data == null || data.Category != ItemCategory.Consumable) {return;}

        quickSlot.SetItemId(slotIndex, id);

        //등록 직후 UI갱신
        RefreshPopupSlots();
    }

    //팝업 5칸 UI 갱신
    private void RefreshPopupSlots()
    {
        for (int i = 0; i < QuickSlotManager.SlotCount; i++)
        {
            if (i >= slotUIs.Length || slotUIs[i] == null) continue;

            string id = quickSlot != null ? quickSlot.GetItemId(i) : null;

            if (string.IsNullOrEmpty(id))
            {
                slotUIs[i].SetEmpty();
                continue;
            }

            var data = database != null ? database.Get(id) : null;
            if (data == null)
            {
                slotUIs[i].SetEmpty();
                continue;
            }

            int count = 0;
            if (inventory != null)
            {
                count = inventory.GetTotalCount(id);
            }

            slotUIs[i].Set(data.Icon, count);
        }
    }
}