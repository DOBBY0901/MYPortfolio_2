using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ItemDatabaseSO database;
    [SerializeField] private Inventory inventory;

    [Header("Slots")]
    [SerializeField] private Transform slotGridRoot;

    [Header("Detail Panel")]
    [SerializeField] private Image detailIcon;      // 가운데 큰 아이콘
    [SerializeField] private TMP_Text nameText;     // ItemName_Text
    [SerializeField] private TMP_Text effectText;   // ItemEffect_Text
    [SerializeField] private TMP_Text descText;     // ItemDescription_Text

    private InventorySlotUI[] slotUIs;

    private int _selectedIndex = -1;

    private void Awake()
    {
        slotUIs = slotGridRoot.GetComponentsInChildren<InventorySlotUI>(true);

        for (int i = 0; i < slotUIs.Length; i++)
            slotUIs[i].Bind(i, OnClickSlot);

        HideDetailContents(); //시작은 아이콘/텍스트만 숨김
    }

    //다시 인벤토리 창 열었을 때 상세정보창 초기화
    private void OnEnable()
    {
        _selectedIndex = -1;
        HideDetailContents();

        if (inventory != null)
            inventory.OnChanged += Refresh;

        Refresh();
    }

    //인벤토리 탭 닫을 때 실행 (안전코드)
    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnChanged -= Refresh;

        HideDetailContents();
    }

    //UI 새로고침
    public void Refresh()
    {
        var slots = inventory.Slots;

        for (int i = 0; i < Inventory.SlotCount; i++)
        {
            if (i >= slotUIs.Length) break;

            if (slotUIs[i] == null)
            {
                continue;
            }

            if (slots[i].IsEmpty)
            {
                slotUIs[i].SetEmpty();
                continue;
            }

            var data = database.Get(slots[i].id);
            if (data == null)
            {
                slotUIs[i].SetEmpty();
                continue;
            }

            
            slotUIs[i].Set(data, slots[i].count);
        }
    }

    //슬롯 클릭
    private void OnClickSlot(int index)
    {
        Debug.Log($"[InventoryUI] Click slot index={index}", this);

        _selectedIndex = index;
        ShowDetail(index);
    }

    //상세 내용 출력
    private void ShowDetail(int index)
    {
        Debug.Log($"[InventoryUI] ShowDetail index={index}", this);
        var slot = inventory.Slots[index];
        Debug.Log($"[InventoryUI] slot id='{slot.id}', count={slot.count}, isEmpty={slot.IsEmpty}", this);

        // 빈 슬롯 클릭하면 상세 내용 숨김
        if (slot.IsEmpty)
        {
            HideDetailContents();
            return;
        }

        var data = database.Get(slot.id);
        if (data == null)
        {
            HideDetailContents();
            return;
        }

        // 상세 내용 표시
        if (detailIcon != null)
        {
            detailIcon.sprite = data.Icon;
            detailIcon.enabled = (data.Icon != null); // HideDetailContents에서 껐으니 다시 켜기
            detailIcon.gameObject.SetActive(true);
        }

        if (nameText != null)
        {
            nameText.text = data.DisplayName;
            nameText.gameObject.SetActive(true);
        }

        if (effectText != null)
        {
            effectText.text = data.EffectDesc;
            effectText.gameObject.SetActive(true);
        }

        if (descText != null)
        {
            descText.text = data.Description;
            descText.gameObject.SetActive(true);
        }
    }

    //상세 내용 숨기기
    private void HideDetailContents()
    {
        if (detailIcon != null)
        {
            detailIcon.sprite = null;
            detailIcon.enabled = false;
        }

        if (nameText != null)
        {
            nameText.text = "";
            nameText.gameObject.SetActive(false);
        }

        if (effectText != null)
        {
            effectText.text = "";
            effectText.gameObject.SetActive(false);
        }

        if (descText != null)
        {
            descText.text = "";
            descText.gameObject.SetActive(false);
        }
    }

    //선택한 아이템의 ID 가지고 오기
    public string GetSelectedItemId()
    {
        if (_selectedIndex < 0 || _selectedIndex >= Inventory.SlotCount) return null;
        var slot = inventory.Slots[_selectedIndex];
        return slot.IsEmpty ? null : slot.id;
    }

}