using UnityEngine;

public class WolfEncounterTrigger : MonoBehaviour
{
    [SerializeField] private WolfEncounterSequence wolfSequence;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        if (wolfSequence == null) return;

        triggered = true;
        wolfSequence.StartEncounter();

        gameObject.SetActive(false); // ¶Ç´Â collider ²ô±â
    }
}