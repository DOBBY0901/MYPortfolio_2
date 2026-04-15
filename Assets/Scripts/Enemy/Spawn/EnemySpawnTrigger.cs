using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    [SerializeField] private EnemySpawnManager spawnManager;

    private void OnTriggerEnter(Collider other) //트리거에 다른 콜라이더가 들어왔을 때 호출되는 메서드
    {
        if (!other.CompareTag("Player")) return;

        if (spawnManager != null)
        {
            spawnManager.SpawnEnemies();
        }
    }
}