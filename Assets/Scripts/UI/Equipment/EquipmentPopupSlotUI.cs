using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPopupSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    private string _itemId;
    private System.Action<string> _onClick;

    public void Bind(System.Action<string> onClick)
    {
        _onClick = onClick;
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    public void Set(ItemDataSO data)
    {
        _itemId = data != null ? data.Id : null;

        if (iconImage != null)
        {
            iconImage.sprite = data != null ? data.Icon : null;
            iconImage.enabled = (data != null && data.Icon != null);
        }


        if (button != null)
            button.interactable = !string.IsNullOrEmpty(_itemId);
    }

    public void SetEmpty()
    {
        _itemId = null;

        if (iconImage != null) { iconImage.sprite = null; iconImage.enabled = false; }
        if (button != null) button.interactable = false;
    }

    private void HandleClick()
    {
        if (string.IsNullOrEmpty(_itemId)) return;
        _onClick?.Invoke(_itemId);
    }
}