using UnityEngine;
using UnityEngine.InputSystem;

public enum MenuTab
{
    Inventory,
    Equipment,
    Map,
    Settings
}

public class MenuManager : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject menuRoot;

    [Header("Panels")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Panels")]
    [SerializeField] private GameObject minimap;
  
    [Header("Options")]
    [SerializeField] private bool pauseTime = true;

    // 메뉴탭 열면 잠금
    private PlayerInput playerInput; // Input System 

    public bool IsOpen => menuRoot != null && menuRoot.activeSelf;
    public MenuTab CurrentTab { get; private set; }

    private void Awake()
    {
        // menuRoot 없으면 방어
        if (menuRoot != null) menuRoot.SetActive(false);

        CacheControls();

        // 게임 시작 상태 정리
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void CacheControls()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
    }

    // 메뉴 패널 열기
    private void OpenMenu(MenuTab tab)
    {
        if (menuRoot == null) return;

        menuRoot.SetActive(true);
        minimap.SetActive(false); // 메뉴 열 때 미니맵 숨김
        SwitchTab(tab);
        SetPlayerControl(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseTime) Time.timeScale = 0f;
    }

    // 메뉴 패널 닫기
    private void CloseMenu()
    {
        if (menuRoot == null) return;

        menuRoot.SetActive(false);
        minimap.SetActive(true); // 메뉴 닫을 때 미니맵 다시 보이게
        SetPlayerControl(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (pauseTime) Time.timeScale = 1f;
    }

    // 메뉴 패널 전환
    private void SwitchTab(MenuTab tab)
    {
        CurrentTab = tab;

        //패널 아직 없을시 방어코드
        if (inventoryPanel != null)inventoryPanel.SetActive(tab == MenuTab.Inventory);
        if (equipmentPanel != null) equipmentPanel.SetActive(tab == MenuTab.Equipment);
        if (mapPanel != null) mapPanel.SetActive(tab == MenuTab.Map);
        if (settingsPanel != null) settingsPanel.SetActive(tab == MenuTab.Settings);

    }

    //입력 잠금
    private void SetPlayerControl(bool enable)
    {
        if (playerInput != null) playerInput.enabled = enable;
    }

    // 키 동작 규칙
    public void ToggleInventory()
    {
        if (!IsOpen) { OpenMenu(MenuTab.Inventory); return; }
        if (CurrentTab == MenuTab.Inventory) { CloseMenu(); return; }
        SwitchTab(MenuTab.Inventory);
    }

    public void ToggleEquipment()
    {
        if (!IsOpen) { OpenMenu(MenuTab.Equipment); return; }
        if (CurrentTab == MenuTab.Equipment) { CloseMenu(); return; }
        SwitchTab(MenuTab.Equipment);
    }

    public void ToggleMap()
    {
        if (!IsOpen) { OpenMenu(MenuTab.Map); return; }
        if (CurrentTab == MenuTab.Map) { CloseMenu(); return; }
        SwitchTab(MenuTab.Map);
    }

    public void ToggleSetting() //메뉴 패널 클릭용 설정창 열기
    {
        if (!IsOpen) { OpenMenu(MenuTab.Settings); return; }
        SwitchTab(MenuTab.Settings);
    }

    // ESC 규칙
    public void EscAction()
    {
        if (!IsOpen) { OpenMenu(MenuTab.Settings); return; }
        CloseMenu();
    }

    public void Resume() => CloseMenu();

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
