using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingSceneUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image progressFillImage; //로딩바 이미지

    [Header("Speed")]
    [SerializeField] private float fillSpeed = 0.5f; //로딩바 채우는 속도
    [SerializeField] private float minimumLoadingTime = 0.5f; //최소 로딩 표시 시간

    private void Start()
    {
        StartCoroutine(LoadSceneRoutine());
    }

    // 씬 로딩 코루틴
    private IEnumerator LoadSceneRoutine()
    {
        string sceneName = LoadingSceneController.NextSceneName;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("다음 씬 이름이 비어 있습니다.");
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float displayProgress = 0f;
        float timer = 0f;

        while (!op.isDone)
        {
            timer += Time.deltaTime;

            // Unity async progress는 0~0.9까지만 먼저 감
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);

            // 부드럽게 로딩바 채우기
            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, fillSpeed * Time.deltaTime);

            if (progressFillImage != null)
                progressFillImage.fillAmount = displayProgress;

            // 실제 로딩 완료 + 로딩바 100% + 최소 표시 시간 만족 시 진입
            if (op.progress >= 0.9f && displayProgress >= 1f && timer >= minimumLoadingTime)
            {          
                yield return new WaitForSeconds(0.2f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}