using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlotRegisterInput : MonoBehaviour
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private QuickSlotAssignPopup popup;

    private void Update()
    {
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        // 메뉴가 열려있고, 인벤 탭일 때만 등록 팝업이 열림.
        if (menuManager != null && menuManager.IsOpen && menuManager.CurrentTab == MenuTab.Inventory)
        {
            if (popup.IsOpen) popup.Close();
            else popup.OpenPopup();
        }
    }
}