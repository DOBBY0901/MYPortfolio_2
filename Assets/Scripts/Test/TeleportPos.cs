using UnityEngine;

public class TeleportPos : MonoBehaviour
{

    [SerializeField] private Transform teleportPos;

    void Start()
    {
        if (teleportPos == null) return;

        var p = GetComponent<CharacterController>();
        if (p != null)
        {
            p.enabled = false;
        }

        transform.SetPositionAndRotation(teleportPos.position, teleportPos.rotation);

        if (p != null)
        {
            p.enabled = true;
        }
    }


}
