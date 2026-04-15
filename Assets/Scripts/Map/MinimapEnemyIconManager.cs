using System.Collections.Generic;
using UnityEngine;

public class MinimapEnemyIconManager : MonoBehaviour
{
    [Header("기준")]
    [SerializeField] private Transform player;          // 기준 플레이어
    [SerializeField] private Camera minimapCamera;      // 미니맵 카메라

    [Header("UI")]
    [SerializeField] private RectTransform iconRoot;    // 아이콘들이 들어갈 부모(위치)

    [Header("적 설정")]
    [SerializeField] private GameObject enemyIconPrefab; // 적 아이콘 프리팹
    [SerializeField] private List<Transform> enemies = new List<Transform>();

    [Header("미니맵 UI 반지름")]
    [SerializeField] private float mapRadius = 90f;     // UI 상 최대 이동 범위

    // 적 Transform ↔ 아이콘 매핑
    private Dictionary<Transform, RectTransform> enemyIcons = new Dictionary<Transform, RectTransform>();
    public static MinimapEnemyIconManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreateEnemyIcons();
    }

    private void LateUpdate()
    {
        if (player == null || minimapCamera == null) return;

        UpdateEnemyIcons();
    }


    // 적 아이콘 생성
    private void CreateEnemyIcons()
    {
        foreach (Transform enemy in enemies)
        {
            if (enemy == null || enemyIcons.ContainsKey(enemy)) continue;

            GameObject iconObj = Instantiate(enemyIconPrefab, iconRoot);

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();

            enemyIcons.Add(enemy, iconRect);
        }
    }

 
    // 적 아이콘 위치 업데이트
    private void UpdateEnemyIcons()
    {
        // 미니맵 카메라가 보여주는 월드 범위
        float worldHalfSize = minimapCamera.orthographicSize;

        // 월드 → UI 좌표 변환 비율
        float worldToUI = mapRadius / worldHalfSize;

        foreach (var pair in enemyIcons)
        {
            Transform enemy = pair.Key;
            RectTransform icon = pair.Value;

            if (enemy == null)
            {
                icon.gameObject.SetActive(false);
                continue;
            }

            // 플레이어 기준 상대 위치
            Vector3 offset = enemy.position - player.position;

            // XZ 평면 좌표만 사용 (위에서 보는 미니맵)
            Vector2 mapPos = new Vector2(offset.x, offset.z) * worldToUI;

            // 거리 계산 (미니맵 안인지 판단)
            float dist = new Vector2(offset.x, offset.z).magnitude;

            bool inRange = dist <= worldHalfSize;

            // -------------------------
            // 범위 안에 있을 때만 표시
            // -------------------------
            icon.gameObject.SetActive(inRange);

            if (!inRange) continue;

            // 위치 적용
            icon.anchoredPosition = mapPos;
        }
    }

    // 적 추가 (동적 생성용)
    public void RegisterEnemy(Transform enemy)
    {
        if (enemy == null || enemyIcons.ContainsKey(enemy)) return;

        GameObject iconObj = Instantiate(enemyIconPrefab, iconRoot);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();

        enemyIcons.Add(enemy, iconRect);
        enemies.Add(enemy);
    }


    // 적 제거 (죽었을 때)
    public void UnregisterEnemy(Transform enemy)
    {
        if (enemy == null) return;

        if (enemyIcons.TryGetValue(enemy, out RectTransform icon))
        {
            if (icon != null) Destroy(icon.gameObject);
            enemyIcons.Remove(enemy);
        }

        enemies.Remove(enemy);
    }
}
