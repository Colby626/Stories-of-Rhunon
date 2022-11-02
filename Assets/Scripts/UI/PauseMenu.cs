using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool gamePaused = false;
    public GameObject pauseMenu;
    public GameObject battleHud;
    public GameObject optionsMenu;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundEffectVolumeSlider;
    public AudioMixer audioMixer;
    public BattleMaster battleMaster;
    private float audioVolume;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && battleMaster.battleStarted == true)
        {
            if (gamePaused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Unpause()
    {
        battleHud.SetActive(true);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        gamePaused = false;
    }

    void Pause()
    {
        battleHud.SetActive(false);
        pauseMenu.SetActive(true);
        //Make sounds quieter
        Time.timeScale = 0f;
        gamePaused = true;
    }

    public void ExitToMainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
        AudioManager.instance.Stop("BattleMusic");
        AudioManager.instance.Play("MainMenuMusic");
    }

    public void OpenOptions()
    {
        pauseMenu.SetActive(false);
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
        pauseMenu.SetActive(true);
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
