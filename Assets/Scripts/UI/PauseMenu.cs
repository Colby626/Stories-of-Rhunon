using System.Linq; //For getting list counts
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
    public GameObject statsMenu;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundEffectVolumeSlider;
    public BattleMaster battleMaster;
    public GameMaster gameMaster;
    public AudioMixer audioMixer;
    [Range(-80f, 0f)]
    public float amountQuieterWhenPaused;

    private float audioVolume;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
        battleMaster.openInventoryButton.SetActive(true);
        if (battleMaster.battleStarted && battleMaster.currentCharacter.isPlayer && battleMaster.currentCharacter.characterStats.XP >= battleMaster.currentCharacter.characterStats.XPtoLevelUp)
        {
            battleMaster.levelUpButton.SetActive(true);
            battleMaster.levelUpCharacter = battleMaster.currentCharacter;
        }
        else
        {
            for (int i = 0; i < battleMaster.characterList.Count(); i++)
            {
                if (battleMaster.characterList[i].characterStats.XP >= battleMaster.characterList[i].characterStats.XPtoLevelUp)
                {
                    battleMaster.levelUpButton.SetActive(true);
                    battleMaster.levelUpCharacter = battleMaster.characterList[i];
                }
            }
        }
        if (battleMaster.battleStarted)
        {
            battleHud.SetActive(true);
            battleHud.GetComponentInChildren<Animator>().SetBool("BattleStarted", true);
        }
        pauseMenu.SetActive(false);
        if (optionsMenu.activeSelf)
        {
            optionsMenu.SetActive(false);
        }
        if (statsMenu.activeSelf)
        {
            statsMenu.SetActive(false);
        }
        audioMixer.SetFloat("PausedMasterVolume", 0);
        Time.timeScale = 1f;
        gamePaused = false;
    }

    private void Pause()
    {
        battleHud.SetActive(false);
        battleMaster.openInventoryButton.SetActive(false);
        battleMaster.levelUpButton.SetActive(false);
        pauseMenu.SetActive(true);
        audioMixer.SetFloat("PausedMasterVolume", amountQuieterWhenPaused);
        Time.timeScale = 0f;
        gamePaused = true;
    }

    public void ExitToMainMenuButton()
    {
        if (gamePaused)
        {
            audioMixer.SetFloat("PausedMasterVolume", 0);
            Unpause();
            gamePaused = false;
        }
        gameMaster.EndBattle();
        battleMaster.Reset();
        pauseMenu.SetActive(false);

        SceneManager.LoadScene("MainMenu");
        AudioManager.instance.StopAll();
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
