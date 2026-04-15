using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    [Header("Player Transform")]
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform rectTransform;

    private void Awake()
    {
        // UI 위치/회전을 위해 RectTransform 캐싱
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (player == null) return;

        // 플레이어의 Y축 회전값 (방향)
        float yRotation = player.eulerAngles.y;

        // UI는 Z축 기준으로 회전해야 함
        // 월드 회전 방향과 UI 회전 방향이 반대기 때문에 -yRotation 사용
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, -yRotation);

        // 플레이어는 항상 중앙
        rectTransform.anchoredPosition = Vector2.zero;
    }
}