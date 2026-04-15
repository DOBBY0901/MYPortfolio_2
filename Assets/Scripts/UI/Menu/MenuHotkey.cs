using UnityEngine;
using UnityEngine.InputSystem;

public class MenuHotkey : MonoBehaviour
{
    [SerializeField] private MenuManager menu;

    private void Update()
    {
        if (menu == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.iKey.wasPressedThisFrame) // I키를 누르면 인벤토리
            menu.ToggleInventory();

        if (kb.uKey.wasPressedThisFrame) // U키를 누르면 장비창
            menu.ToggleEquipment();

        if (kb.mKey.wasPressedThisFrame) // M키를 누르면 맵
            menu.ToggleMap();

        if (kb.escapeKey.wasPressedThisFrame) // ESC키를 누르면 설정/닫기
            menu.EscAction();
    }
}
