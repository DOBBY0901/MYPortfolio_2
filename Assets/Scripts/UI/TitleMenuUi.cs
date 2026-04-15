using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel; // 설정 패널

    [Header("Scene Name")]
    [SerializeField] private string gameSceneName = "Game"; // 게임 씬

    [Header("Audio Settings")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private TMP_Text masterValueText;
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text sfxValueText;

    private void Start()
    {
        Time.timeScale = 1f; // 타이틀에서는 시간 정지 해제

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // 슬라이더 이벤트 연결
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    private void OnDestroy()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
    }

    // 설정 패널 열기
    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        RefreshAudioUI();
        // 패널 열릴 때 현재 AudioManager 값으로 슬라이더/텍스트 갱신
    }

    // 설정 패널 닫기
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // 게임 시작
    public void StartGame()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            LoadingSceneController.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("게임 씬 이름이 틀렸습니다.");
        }
    }

    // 게임 종료
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("에디터 - 게임종료");
#endif
    }

    private void RefreshAudioUI()
    {
        if (AudioManager.Instance == null) return;

        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(AudioManager.Instance.MasterVolume);

        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(AudioManager.Instance.BgmVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);

        UpdateVolumeText(masterSlider, masterValueText);
        UpdateVolumeText(bgmSlider, bgmValueText);
        UpdateVolumeText(sfxSlider, sfxValueText);
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMasterVolume(value);
        UpdateVolumeText(masterSlider, masterValueText);
    }

    private void OnBgmVolumeChanged(float value)
    {
        AudioManager.Instance?.SetBgmVolume(value);
        UpdateVolumeText(bgmSlider, bgmValueText);
    }

    private void OnSfxVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSfxVolume(value);
        UpdateVolumeText(sfxSlider, sfxValueText);
    }

    private void UpdateVolumeText(Slider slider, TMP_Text text)
    {
        if (slider == null || text == null) return;

        int percent = Mathf.RoundToInt(slider.value * 100f);
        text.text = percent.ToString();
    }
}