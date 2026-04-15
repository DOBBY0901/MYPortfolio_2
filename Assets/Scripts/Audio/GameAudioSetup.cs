using UnityEngine;

public class GameAudioSetup : MonoBehaviour
{
    private void Start()
    {
        AudioManager.Instance?.EnterGameMode();
    }
}