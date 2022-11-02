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
    public Slider volumeSlider;
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
        volumeSlider.value = audioVolume;
    }

    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void VolumeSlider(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1.0f);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }
}
