using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundEffectVolumeSlider;
    private float audioVolume;

    public void PlayButton()
    {
        SceneManager.LoadScene("GrassPlains_1");
        AudioManager.instance.StopAll();
        AudioManager.instance.Play("ExploringMusic");
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
        masterVolumeSlider.value = audioVolume;

        audioMixer.GetFloat("MusicVolume", out audioVolume);
        audioVolume /= 20;
        audioVolume = Mathf.Pow(10, audioVolume);
        musicVolumeSlider.value = audioVolume;

        audioMixer.GetFloat("SoundEffectVolume", out audioVolume);
        audioVolume /= 20;
        audioVolume = Mathf.Pow(10, audioVolume);
        soundEffectVolumeSlider.value = audioVolume;
    }

    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void MasterVolumeSlider(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1.0f); 
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }

    public void MusicVolumeSlider(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1.0f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }

    public void SoundEffectVolumeSlider(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1.0f);
        audioMixer.SetFloat("SoundEffectVolume", Mathf.Log10(value) * 20);
    }
}
