using UnityEngine;
using StarterAssets;

public class EnvironmentController : MonoBehaviour
{
    public enum EnvironmentState
    {
        Normal,           // 기본 지역
        Cave,             // 동굴 지역
        StrongBlizzard    // 강한 눈보라 지역
    }

    [Header("Current State")]
    [SerializeField] private EnvironmentState currentState = EnvironmentState.Normal;
    // 현재 적용 중인 환경 상태

    [Header("Particles")]
    [SerializeField] private ParticleSystem normalBlizzardParticle;
    // 기본 설원에서 사용할 눈보라 파티클

    [SerializeField] private ParticleSystem strongBlizzardParticle;
    // 강한 눈보라 지역에서 사용할 강한 파티클

    [Header("Fog")]
    [SerializeField] private bool useFog = true;

    [SerializeField] private float caveFogDensity = 0.02f;
    // 동굴 Fog 밀도

    [SerializeField] private float strongBlizzardFogDensity = 0.03f;
    // 강한 눈보라 Fog 밀도

    [SerializeField] private float fogLerpSpeed = 2f;
    // Fog 밀도 변화 속도

    [Header("Lighting")]
    [SerializeField] private Light sunLight;

    [SerializeField] private float lightLerpSpeed = 2f;
    // 조명 변화 속도

    // 기본 상태(설원)
    [SerializeField] private float normalSunIntensity = 1f;
    [SerializeField] private Color normalAmbientColor;
    [SerializeField] private float normalAmbientIntensity = 0.8f;
    [SerializeField] private float normalReflectionIntensity = 1f;

    // 동굴 상태
    [SerializeField] private float caveSunIntensity = 1f;
    [SerializeField] private Color caveAmbientColor = Color.black;
    [SerializeField] private float caveAmbientIntensity = 0f;
    [SerializeField] private float caveReflectionIntensity = 0.2f;

    [Header("Player")]
    [SerializeField] private ThirdPersonController playerController;
    // 플레이어 이동속도 배율을 바꾸기 위한 참조

    [SerializeField] private float normalMoveSpeedMultiplier = 1f;
    // 기본 이동속도 배율

    [SerializeField] private float strongBlizzardMoveSpeedMultiplier = 0.6f;
    // 강한 눈보라 지역에서 느려질 배율

    public EnvironmentState CurrentState => currentState;
    // 다른 스크립트에서 현재 상태를 읽을 수 있게 공개

    private Coroutine fogCoroutine; // Fog 밀도 전환 코루틴 참조
    private float _defaultFogDensity; // 게임 시작 시 기본 Fog 밀도 저장
    private Coroutine lightCoroutine; // 조명 전환 코루틴 참조

    private void Awake()
    {

        RenderSettings.fog = useFog;
        _defaultFogDensity = RenderSettings.fogDensity;
        //게임 시작 시 기본 Fog 밀도 저장

        ApplyState(currentState);
        // 게임 시작 시 Inspector에 설정된 초기 상태를 바로 적용
    }

    public void SetEnvironmentState(EnvironmentState newState)
    {
        if (currentState == newState) return;
        // [이미 같은 상태면 중복 실행 방지

        currentState = newState;
        // 현재 상태 갱신

        ApplyState(currentState);
        // 바뀐 상태를 실제 환경에 적용
    } 

    private void ApplyState(EnvironmentState state)
    {

        UpdateParticles(state);
        // 상태에 맞는 파티클 적용

        UpdatePlayerSpeed(state);
        // 상태에 맞는 이동속도 적용

        UpdateAmbientAudio(state);
        // 상태에 맞는 환경음 적용

        UpdateFog(state);
        // 상태에 맞는 Fog 적용

        UpdateLights(state);
        // 상태에 맞는 조명 적용
    }

    private void UpdateParticles(EnvironmentState state)
    {
        switch (state)
        {
            case EnvironmentState.Normal:
                PlayParticle(normalBlizzardParticle);
                StopParticle(strongBlizzardParticle);
                break;
            // 기본 설원: 기본 눈보라만 켬

            case EnvironmentState.Cave:
                StopParticle(normalBlizzardParticle);
                StopParticle(strongBlizzardParticle);
                break;
            // 동굴: 파티클 전부 끔

            case EnvironmentState.StrongBlizzard:
                StopParticle(normalBlizzardParticle);
                PlayParticle(strongBlizzardParticle);
                break;
            // 강한 눈보라: 강한 파티클만 켬
        }
    }

    private void UpdatePlayerSpeed(EnvironmentState state)
    {
        if (playerController == null) return;
           // 플레이어 참조가 없으면 종료

        switch (state)
        {
            case EnvironmentState.Normal:
            case EnvironmentState.Cave:
                playerController.SetEnvironmentMoveMultiplier(normalMoveSpeedMultiplier);
                break;
            // 기본 지역과 동굴은 기본 속도 유지

            case EnvironmentState.StrongBlizzard:
                playerController.SetEnvironmentMoveMultiplier(strongBlizzardMoveSpeedMultiplier);
                break;
            // 강한 눈보라 지역은 느려짐
        }
    }

    private void UpdateAmbientAudio(EnvironmentState state)
    {
        AudioManager.Instance?.SetAmbientState(state);
        // 환경음은 AudioManager가 상태를 받아서 처리
    }

    private void UpdateLights(EnvironmentState state)
    {
        if (lightCoroutine != null)
            StopCoroutine(lightCoroutine);

        switch (state)
        {
            case EnvironmentState.Normal:
                lightCoroutine = StartCoroutine(LightTransition(
                    normalSunIntensity,
                    normalAmbientIntensity,
                    normalReflectionIntensity,
                    normalAmbientColor, // Ambient Color는 기본값 유지
                    true // Skybox
                ));
                break;

            case EnvironmentState.Cave:
                lightCoroutine = StartCoroutine(LightTransition(
                    caveSunIntensity,
                    caveAmbientIntensity,
                    caveReflectionIntensity,
                    caveAmbientColor, // 동굴은 어두운 색으로 변경
                    false // Color
                ));
                break;
        }
    }

    private void PlayParticle(ParticleSystem particle)
    {
        if (particle == null) return; //

        if (!particle.gameObject.activeSelf)
            particle.gameObject.SetActive(true);

        particle.Clear();
        particle.Play();
    }

    private void StopParticle(ParticleSystem particle)
    {
        if (particle == null) return;

        particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        // 방출 중지. 이미 나온 입자는 자연스럽게 사라짐
    }

    private void UpdateFog(EnvironmentState state) // 상태에 맞는 Fog 설정 적용
    {
        if (!useFog)
        {
            RenderSettings.fog = false;
            return;
        }

        RenderSettings.fog = true;

        switch (state)
        {
            case EnvironmentState.Normal:
                StartFogDensityTransition(_defaultFogDensity);
                break;

            case EnvironmentState.Cave:
                StartFogDensityTransition(caveFogDensity);
                break;

            case EnvironmentState.StrongBlizzard:
                StartFogDensityTransition(strongBlizzardFogDensity);
                break;
        }
    }

    private void StartFogDensityTransition(float targetDensity) // Fog 밀도 전환 시작
    {
        if (fogCoroutine != null)
            StopCoroutine(fogCoroutine);

        fogCoroutine = StartCoroutine(FogDensityTransition(targetDensity)); // 새로운 밀도로 전환하는 코루틴 시작
    }

    private System.Collections.IEnumerator FogDensityTransition(float targetDensity) // Fog 밀도를 부드럽게 전환하는 코루틴
    {
        float startDensity = RenderSettings.fogDensity;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * fogLerpSpeed;
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, targetDensity, t); // 선형 보간으로 밀도 전환
            yield return null;
        }

        RenderSettings.fogDensity = targetDensity; // 최종적으로 정확한 목표 밀도로 설정
    }

    private System.Collections.IEnumerator LightTransition(float targetSun, float targetAmbientIntensity,float targetReflection, Color targetAmbientclor, bool useSkybox) // 조명을 부드럽게 전환하는 코루틴
    {
        float startSun = sunLight.intensity; // 현재 태양광 강도 저장
        float startAmbient = RenderSettings.ambientIntensity; // 현재 주변광 강도 저장
        float startReflection = RenderSettings.reflectionIntensity; // 현재 반사광 강도 저장
        float startAmbientColorR = RenderSettings.ambientLight.r; // 현재 주변광 색상 저장

        float t = 0f; 

        while (t < 1f) 
        {
            t += Time.deltaTime * lightLerpSpeed; 

            sunLight.intensity = Mathf.Lerp(startSun, targetSun, t); 
            RenderSettings.ambientIntensity = Mathf.Lerp(startAmbient, targetAmbientIntensity, t); 
            RenderSettings.reflectionIntensity = Mathf.Lerp(startReflection, targetReflection, t);
            RenderSettings.ambientLight = Color.Lerp(new Color(startAmbientColorR, startAmbientColorR, startAmbientColorR), targetAmbientclor, t);

            yield return null;
        }
        sunLight.intensity = targetSun; 
        RenderSettings.ambientIntensity = targetAmbientIntensity;
        RenderSettings.reflectionIntensity = targetReflection;

        // 마지막에 모드 변경
        if (useSkybox)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        else
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    }

}