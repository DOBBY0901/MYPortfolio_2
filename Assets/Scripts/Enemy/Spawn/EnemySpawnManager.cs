using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject enemyPrefab; //스폰할 적 프리팹
    [SerializeField] private Transform[] spawnPoints; //적이 스폰될 위치

    [Header("Option")]
    [SerializeField] private bool spawnOnlyOnce = true; //적이 한 번만 스폰되도록 설정하는 옵션

    private bool hasSpawned = false; //적이 이미 스폰되었는지 여부
    private readonly List<GameObject> spawnedEnemies = new(); //스폰된 적들을 추적하기 위한 리스트

    public void SpawnEnemies() //적 스폰
    {
        if (spawnOnlyOnce && hasSpawned) //적이 한 번만 스폰되도록 설정되어 있고 이미 스폰된 경우, 함수를 종료
            return;

        hasSpawned = true; //적이 스폰되었음을 표시

        foreach (Transform point in spawnPoints) //각 스폰 포인트에 대해 반복
        {
            if (point == null) continue;

            GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);
            spawnedEnemies.Add(enemy);
        }
    }
}