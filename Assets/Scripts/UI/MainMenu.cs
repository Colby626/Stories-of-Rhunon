using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public void PlayButton()
    {
        SceneManager.LoadScene("ColbyDemo");
        AudioManager.instance.Stop("MainMenuMusic");
        AudioManager.instance.Play("BattleMusic");
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void VolumeSlider(float value)
    {
        audioMixer.SetFloat("MasterVolume", value);
    }
}
