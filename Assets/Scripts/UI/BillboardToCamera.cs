using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // 카메라가 바라보는 방향으로 맞춤
        transform.forward = cam.transform.forward;
    }
}
