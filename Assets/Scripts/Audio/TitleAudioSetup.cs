using UnityEngine;

public class TitleAudioSetup : MonoBehaviour
{
    private void Start()
    {
        AudioManager.Instance?.EnterTitleMode();
    }
}