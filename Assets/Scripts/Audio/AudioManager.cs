using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Volumes")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f; // 전체 소리 최종 볼륨
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;    // 배경음 볼륨
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;    // 효과음 볼륨

    [Header("BGM")]
    [SerializeField] private AudioSource titlebgmSource; // 타이틀 배경음악

    [Header("2D SFX")]
    [SerializeField] private AudioSource sfx2DSource; // UI, 기합 같은 2D 사운드 전용 AudioSource

    [Header("3D SFX")]
    [SerializeField] private AudioSource sfx3DPrefab; // 위치 기반 사운드를 위한 AudioSource 프리팹

    [Header("Ambient")]
    [SerializeField] private AudioSource blizzardSource; // 눈보라 배경음
    [SerializeField] private AudioSource caveSource;     // 동굴 배경음
    [SerializeField] private AudioSource strongBlizzardSource; //강한 눈보라 배경음
    [SerializeField] private float ambientFadeTime = 1f; // 환경음 전환 시 페이드인, 아웃 시간

    public static AudioManager Instance { get; private set; } // 싱글톤

    private Coroutine ambientFadeCo; // 페이드 코루틴 중복 방지용
    private EnvironmentController.EnvironmentState currentAmbientState = EnvironmentController.EnvironmentState.Normal;
    private bool isGameMode = false; // 현재 게임 씬 오디오 모드인지

    private const string MASTER_VOLUME_KEY = "MasterVolume"; // PlayerPrefs 키
    private const string BGM_VOLUME_KEY = "BgmVolume";
    private const string SFX_VOLUME_KEY = "SfxVolume";

    public float MasterVolume => masterVolume;
    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 이미 존재하면 현재 객체 삭제
            return;
        }

        Instance = this; // 싱글톤 등록
        LoadVolumeSettings(); // 저장된 볼륨 설정 불러오기
        DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지

        if (sfx2DSource == null)
        {
            GameObject go = new GameObject("2D_SFX_Source");
            go.transform.SetParent(transform);

            sfx2DSource = go.AddComponent<AudioSource>();
            sfx2DSource.playOnAwake = false;
            sfx2DSource.spatialBlend = 0f;
        }
    }

    // 게임 시작 시 저장된 볼륨 설정 불러오기
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    // 게임 종료 시 볼륨 설정 저장
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    // 2D SFX
    public void Play2DSfx(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || sfx2DSource == null) return;

        sfx2DSource.pitch = pitch;
        sfx2DSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
        sfx2DSource.pitch = 1f;
    }

    // 랜덤하게 여러 클립 중 하나 재생
    public void PlayRandom2DSfx(AudioClip[] clips, float volume = 1f, float pitchMin = 1f, float pitchMax = 1f)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        float pitch = Random.Range(pitchMin, pitchMax);

        Play2DSfx(clip, volume, pitch);
    }

    // 3D SFX
    public void Play3DSfx(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource src = Create3DSource(position);

        src.pitch = pitch;
        src.volume = volume * sfxVolume * masterVolume;
        src.clip = clip;
        src.Play();

        Destroy(src.gameObject, clip.length + 0.1f);
    }

    // 랜덤하게 여러 클립 중 하나 재생
    public void PlayRandom3DSfx(AudioClip[] clips, Vector3 position, float volume = 1f, float pitchMin = 1f, float pitchMax = 1f)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        float pitch = Random.Range(pitchMin, pitchMax);

        Play3DSfx(clip, position, volume, pitch);
    }

    // 타이틀 BGM 재생
    public void PlayBgm()
    {
        if (titlebgmSource == null) return;

        titlebgmSource.volume = bgmVolume * masterVolume;
        titlebgmSource.Play();
    }

    // 3D 사운드용 AudioSource 생성 (위치 기반)
    private AudioSource Create3DSource(Vector3 position)
    {
        AudioSource src;

        if (sfx3DPrefab != null)
        {
            src = Instantiate(sfx3DPrefab, position, Quaternion.identity);
        }
        else
        {
            GameObject go = new GameObject("3D_SFX");
            go.transform.position = position;

            src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
        }

        return src;
    }

    // Ambient (환경음)
    public void SetAmbientState(EnvironmentController.EnvironmentState state)
    {
        currentAmbientState = state;

        if (!isGameMode) return;

        if (blizzardSource != null && !blizzardSource.isPlaying)
            blizzardSource.Play();

        if (caveSource != null && !caveSource.isPlaying)
            caveSource.Play();

        if (strongBlizzardSource != null && !strongBlizzardSource.isPlaying)
            strongBlizzardSource.Play();

        if (ambientFadeCo != null)
            StopCoroutine(ambientFadeCo);

        ambientFadeCo = StartCoroutine(FadeAmbientRoutine(state));
    }

    // 환경음 페이드 코루틴
    private IEnumerator FadeAmbientRoutine(EnvironmentController.EnvironmentState state)
    {
        if (blizzardSource == null || caveSource == null || strongBlizzardSource == null)
            yield break;

        float time = 0f;

        float startNormal = blizzardSource.volume;
        float startCave = caveSource.volume;
        float startStrong = strongBlizzardSource.volume;

        float volume = bgmVolume * masterVolume;

        float targetNormal = 0f;
        float targetCave = 0f;
        float targetStrong = 0f;

        switch (state)
        {
            case EnvironmentController.EnvironmentState.Normal:
                targetNormal = volume;
                break;

            case EnvironmentController.EnvironmentState.Cave:
                targetCave = volume;
                break;

            case EnvironmentController.EnvironmentState.StrongBlizzard:
                targetStrong = volume;
                break;
        }

        while (time < ambientFadeTime)
        {
            time += Time.deltaTime;
            float t = time / ambientFadeTime;

            blizzardSource.volume = Mathf.Lerp(startNormal, targetNormal, t);
            caveSource.volume = Mathf.Lerp(startCave, targetCave, t);
            strongBlizzardSource.volume = Mathf.Lerp(startStrong, targetStrong, t);

            yield return null;
        }

        blizzardSource.volume = targetNormal;
        caveSource.volume = targetCave;
        strongBlizzardSource.volume = targetStrong;
    }

    // 환경음 볼륨 즉시 반영 (설정 변경 시)
    private void ApplyAmbientVolumeImmediate()
    {
        if (!isGameMode) return;

        float volume = bgmVolume * masterVolume;

        if (blizzardSource != null)
        {
            blizzardSource.volume = (currentAmbientState == EnvironmentController.EnvironmentState.Normal) ? volume : 0f;
            if (!blizzardSource.isPlaying) blizzardSource.Play();
        }

        if (caveSource != null)
        {
            caveSource.volume = (currentAmbientState == EnvironmentController.EnvironmentState.Cave) ? volume : 0f;
            if (!caveSource.isPlaying) caveSource.Play();
        }

        if (strongBlizzardSource != null)
        {
            strongBlizzardSource.volume = (currentAmbientState == EnvironmentController.EnvironmentState.StrongBlizzard) ? volume : 0f;
            if (!strongBlizzardSource.isPlaying) strongBlizzardSource.Play();
        }
    }

    // 타이틀 BGM 볼륨 즉시 반영 (설정 변경 시)
    private void ApplyBgmVolumeImmediate()
    {
        if (titlebgmSource == null) return;

        titlebgmSource.volume = bgmVolume * masterVolume;
    }

    // 에디터에서 값 변경 시 볼륨 범위 유지 및 즉시 반영
    private void OnValidate()
    {
        masterVolume = Mathf.Clamp01(masterVolume);
        bgmVolume = Mathf.Clamp01(bgmVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);

        ApplyAmbientVolumeImmediate();
        ApplyBgmVolumeImmediate();
    }

    // 외부에서 볼륨 설정 변경 시 즉시 반영
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyAmbientVolumeImmediate(); // 환경음 즉시 반영
        ApplyBgmVolumeImmediate(); // 타이틀 BGM 즉시 반영

        SaveVolumeSettings(); // 변경된 설정 저장
    }

    // 외부에서 볼륨 설정 변경 시 즉시 반영
    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        ApplyAmbientVolumeImmediate(); // 환경음 즉시 반영
        ApplyBgmVolumeImmediate(); // 타이틀 BGM 즉시 반영

        SaveVolumeSettings(); // 변경된 설정 저장
    }

    // 외부에서 볼륨 설정 변경 시 즉시 반영
    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        SaveVolumeSettings(); // 변경된 설정 저장
    }

    // 타이틀 모드 진입 시 환경음 끄고 타이틀 BGM 켜기
    public void EnterTitleMode()
    {
        isGameMode = false;
        // 환경음 전부 끄기
        if (blizzardSource != null)
        {
            blizzardSource.Stop();
            blizzardSource.volume = 0f;
        }

        if (caveSource != null)
        {
            caveSource.Stop();
            caveSource.volume = 0f;
        }

        // 타이틀 BGM 켜기
        if (titlebgmSource != null)
        {
            titlebgmSource.volume = bgmVolume * masterVolume;

            if (!titlebgmSource.isPlaying)
                titlebgmSource.Play();
        }
    }

    // 게임 모드 진입 시 타이틀 BGM 끄고 현재 환경 상태 기준으로 환경음 적용
    public void EnterGameMode()
    {
        isGameMode = true;
        // 타이틀 음악 끄기
        if (titlebgmSource != null)
            titlebgmSource.Stop();

        // 현재 환경 상태 기준으로 환경음 적용
        ApplyAmbientVolumeImmediate();
    }

  

}