using UnityEngine;

public class EnvironmentZone : MonoBehaviour
{
    [SerializeField] private EnvironmentController environment;
    // 환경 상태를 실제로 바꿀 컨트롤러 참조

    [SerializeField] private EnvironmentController.EnvironmentState zoneState;
    // 존에 들어왔을 때 적용할 상태

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        // 플레이어만 감지

        environment.SetEnvironmentState(zoneState);
        // 이 존에 설정된 상태로 환경 변경
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Normal 존은 나가도 굳이 처리 안 함
        if (zoneState == EnvironmentController.EnvironmentState.Cave ||
            zoneState == EnvironmentController.EnvironmentState.StrongBlizzard)
        {
            environment.SetEnvironmentState(EnvironmentController.EnvironmentState.Normal);
        }
    }
}