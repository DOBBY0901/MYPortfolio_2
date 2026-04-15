using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    [Header("Scene Name")]
    [SerializeField] private string titleSceneName = "Title";

    [Header("Audio Settings")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private TMP_Text masterValueText;
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text sfxValueText;

    private void Start()
    {
        if (AudioManager.Instance == null) return;

        // 현재 AudioManager 값으로 슬라이더 초기화
        masterSlider.SetValueWithoutNotify(AudioManager.Instance.MasterVolume);
        bgmSlider.SetValueWithoutNotify(AudioManager.Instance.BgmVolume);
        sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);

        // 텍스트 초기화
        UpdateVolumeText(masterSlider, masterValueText);
        UpdateVolumeText(bgmSlider, bgmValueText);
        UpdateVolumeText(sfxSlider, sfxValueText);

        // 슬라이더 이벤트 연결
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    private void OnDestroy()
    {
        // 중복 등록 방지용 해제
        masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
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

    // 타이틀로 돌아가기 버튼
    public void ReturnToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }

    // 게임 종료 버튼
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("에디터 - 게임종료");
#endif
    }
}