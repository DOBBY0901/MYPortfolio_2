using UnityEngine;
using UnityEngine.UI;

public class EquipmentEquippedIconUI : MonoBehaviour
{
    [SerializeField] private EquipmentSlot slotType;
    [SerializeField] private EquipmentManager equipment;
    [SerializeField] private ItemDatabaseSO database;
    [SerializeField] private Image iconImage;

    private void OnEnable()
    {
        if (equipment != null)
            equipment.OnChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (equipment != null)
            equipment.OnChanged -= Refresh;
    }

    public void Refresh()
    {
        if (equipment == null || database == null || iconImage == null) return;

        string id = equipment.GetEquippedId(slotType);
        if (string.IsNullOrEmpty(id))
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            return;
        }

        var data = database.Get(id);
        iconImage.sprite = data != null ? data.Icon : null;
        iconImage.enabled = (iconImage.sprite != null);
    }
}