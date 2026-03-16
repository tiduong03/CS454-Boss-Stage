using UnityEngine;

public class BossFightMusic : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;

    private bool stoppedForEnd;
    private bool pausedForMenu;

    private void Start()
    {
        if (musicSource != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    private void Update()
    {
        if (musicSource == null || GameUIManager.Instance == null || stoppedForEnd)
            return;

        GameUIManager ui = GameUIManager.Instance;

        // Win or lose = stop completely
        if (ui.HasWon || ui.HasLost)
        {
            stoppedForEnd = true;
            pausedForMenu = false;
            musicSource.Stop();
            return;
        }

        // Pause menu = pause music only
        if (ui.IsPaused)
        {
            if (!pausedForMenu)
            {
                pausedForMenu = true;
                musicSource.Pause();
            }
        }
        else
        {
            if (pausedForMenu)
            {
                pausedForMenu = false;
                musicSource.UnPause();
            }
        }
    }
}