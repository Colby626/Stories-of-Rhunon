using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public Slider volumeSlider;
    private float audioVolume;

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

    public void OpenOptions()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
        audioMixer.GetFloat("MasterVolume", out audioVolume);
        audioVolume /= 20;
        audioVolume = Mathf.Pow(10, audioVolume);
        volumeSlider.value = audioVolume;
    }

    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void VolumeSlider(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1.0f); 
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }
}
