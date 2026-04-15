using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlotHUD : MonoBehaviour
{
    [SerializeField] private QuickSlotManager quickSlot;
    [SerializeField] private ItemDatabaseSO database;
    [SerializeField] private Inventory inventory;
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private GameObject hudRoot;

    [Header("HUD UI")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Image indexbgImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text indexText;

    [Header("Cooldown UI")]
    [SerializeField] private QuickSlotGameInput quickSlotInput;
    [SerializeField] private Image cooldownMaskImage; // Radial Fill용
    [SerializeField] private TMP_Text cooldownText;   // 남은 시간 표시

    private string _lastId;
    private int _lastIndex = -1;

    private void Awake()
    {
        SetCooldownUI(false, 0f, 0f); //시작하면 비활성화
    }

    private void OnEnable()
    {
        SetCooldownUI(false, 0f, 0f);

        // 인벤 변경 시(드랍/소모/장착 등) HUD 수량 즉시 갱신
        if (inventory != null)
            inventory.OnChanged += RefreshStatic;

        // 처음 켜질 때 한번 갱신
        _lastId = null;
        _lastIndex = -1;
        RefreshStatic();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnChanged -= RefreshStatic;

        SetCooldownUI(false, 0f, 0f);
    }

    private void LateUpdate()
    {
        //메뉴가 열려있으면 HUD는 비활성화
        if (menuManager != null && menuManager.IsOpen)
        {
            if (hudRoot != null) hudRoot.SetActive(false);
            SetCooldownUI(false, 0f, 0f);
            return;
        }
        else
        {
            if (hudRoot != null) hudRoot.SetActive(true);
        }

        if (quickSlot == null || database == null || inventory == null) return;

        // 슬롯이 바뀌었을 때만 아이콘/수량/UI 텍스트 갱신
        string id = quickSlot.GetCurrentItemId();
        int index = quickSlot.CurrentIndex;

        if (id != _lastId || index != _lastIndex)
        {
            _lastId = id;
            _lastIndex = index;
            RefreshStatic();
        }

        // 타임은 시간이 흐르므로 매 프레임 표시 업데이트
        UpdateCooldownUI(_lastId);
    }

    //아이콘/수량/인덱스 등 "정적인 표시"만 갱신
    private void RefreshStatic()
    {
        if (quickSlot == null || database == null || inventory == null) return;

        string id = quickSlot.GetCurrentItemId();

        if (indexText != null)
            indexText.text = (quickSlot.CurrentIndex + 1).ToString(); //인덱스 번호

        if (string.IsNullOrEmpty(id))
        {
            if (iconImage != null) { iconImage.sprite = null; iconImage.enabled = false; }
            if (countText != null) { countText.text = ""; }
            SetCooldownUI(false, 0f, 0f);
            return;
        }

        var data = database.Get(id);
        if (data == null)
        {
            if (iconImage != null) { iconImage.sprite = null; iconImage.enabled = false; }
            if (countText != null) { countText.text = ""; }
            SetCooldownUI(false, 0f, 0f);
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = data.Icon;
            iconImage.enabled = (data.Icon != null);
        }

        if (countText != null)
        {
            int count = inventory.GetTotalCount(id);
            countText.text = count > 0 ? count.ToString() : "0";
        }
    }

    //쿨타임UI 갱산
    private void UpdateCooldownUI(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            SetCooldownUI(false, 0f, 0f);
            return;
        }

        if (quickSlotInput == null)
        {
            SetCooldownUI(false, 0f, 0f);
            return;
        }

        if (quickSlotInput.TryGetCooldown(id, out float remain, out float duration))
        {
            if (cooldownMaskImage != null)
            {
                float t = (duration <= 0f) ? 0f : Mathf.Clamp01(remain / duration);
                cooldownMaskImage.fillAmount = t;
            }

            if (cooldownText != null)
            {
                // 1초 이상이면 정수, 1초 미만이면 소수 1자리
                cooldownText.text = (remain >= 1f) ? Mathf.CeilToInt(remain).ToString() : remain.ToString("0.0");
            }

            SetCooldownUI(true, remain, duration);
        }
        else
        {
            SetCooldownUI(false, 0f, 0f);
        }
    }

    //쿨타임UI 설정
    private void SetCooldownUI(bool active, float remain, float duration)
    {
        if (cooldownMaskImage != null) cooldownMaskImage.gameObject.SetActive(active);
        if (cooldownText != null) cooldownText.gameObject.SetActive(active);
    }
}