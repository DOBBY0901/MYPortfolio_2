using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlotGameInput : MonoBehaviour
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private QuickSlotManager quickSlot;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Use")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemDatabaseSO database;

    [Header("UseFeedback")]
    [SerializeField] private Transform vfxSpawnPoint; 
    [SerializeField] private AudioSource audioSource;

    private bool clearSlotWhenEmpty = false;
    private readonly Dictionary<string, float> _nextUseTimeById = new();
    private readonly Dictionary<string, float> _cooldownDurationById = new();

    private void Update()
    {
        if (quickSlot == null) return;

        // 메뉴가 열려있으면 게임 내 퀵슬롯 조작 금지
        if (menuManager != null && menuManager.IsOpen) return;

        // 마우스 휠로 현재 슬롯 변경
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0.01f) quickSlot.ScrollSelect(-1);
        else if (scroll < -0.01f) quickSlot.ScrollSelect(+1);

        // E키로 현재 슬롯 아이템 사용
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryUseCurrent();
        }
    }

    //현재 슬롯 아이템 사용
    private void TryUseCurrent()
    {
        if (inventory == null || database == null) return;

        string id = quickSlot.GetCurrentItemId();
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        var data = database.Get(id);
        if (data == null)
        {
            return;
        }

        // 소모품만 사용
        if (data.Category != ItemCategory.Consumable)
        {
            return;
        }

        // 인벤에 실제 수량이 있는지 확인
        int count = inventory.GetTotalCount(id);
        if (count <= 0)
        {
            if (clearSlotWhenEmpty)
                quickSlot.Clear(quickSlot.CurrentIndex);
            return;
        }
        
        //쿨타임 체크
        if (IsOnCooldown(id))
            return;

        //아이템 사용
        bool applied = ItemEffectRunner.TryApplyUseEffects(data, playerHealth.gameObject);
        if (!applied) return;

        // 효과 성공했을 때만 수량 차감
        bool consumed = inventory.UseItem(id, 1);
        if (!consumed) return;

        //사운드. 이펙트
        PlayUseFeedback(data);

        //쿨타임 시작
        StartCooldown(id, data.UseCooldown);

        // 다 썼으면 슬롯 비우기
        if (clearSlotWhenEmpty && inventory.GetTotalCount(id) <= 0)
            quickSlot.Clear(quickSlot.CurrentIndex);
    }

    //쿨타임 중
    private bool IsOnCooldown(string id)
    {
        if (_nextUseTimeById.TryGetValue(id, out float t))
            return Time.time < t;
        return false;
    }

    //쿨타임 시작
    private void StartCooldown(string id, float cooldown)
    {
        if (cooldown <= 0f) return;
        _nextUseTimeById[id] = Time.time + cooldown;
        _cooldownDurationById[id] = cooldown;
    }

    //쿨타임 시간 가지고 오기
    public bool TryGetCooldown(string id, out float remain, out float duration)
    {
        remain = 0f;
        duration = 0f;

        if (string.IsNullOrEmpty(id)) return false;
        if (!_nextUseTimeById.TryGetValue(id, out float nextUseTime)) return false;
        if (!_cooldownDurationById.TryGetValue(id, out duration)) return false;

        remain = Mathf.Max(0f, nextUseTime - Time.time);
        return remain > 0f && duration > 0f;
    }

    private void PlayUseFeedback(ItemDataSO data)
    {
        // SFX
        if (data.UseSfx != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(data.UseSfx, data.UseSfxVolume);
            else
                AudioSource.PlayClipAtPoint(data.UseSfx, playerHealth.transform.position, data.UseSfxVolume);
        }

        // VFX
        if (data.UseVfxPrefab != null)
        {
            Transform parent = (vfxSpawnPoint != null) ? vfxSpawnPoint : playerHealth.transform;

            GameObject vfxObj = Instantiate(data.UseVfxPrefab, parent.position, parent.rotation, parent);

            vfxObj.transform.localPosition = Vector3.zero;
            vfxObj.transform.localRotation = Quaternion.identity;
        }
    }


   }