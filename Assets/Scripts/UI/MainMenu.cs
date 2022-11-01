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
        value = Mathf.Clamp(value, 0.0001f, 1.0f); 
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }
}
