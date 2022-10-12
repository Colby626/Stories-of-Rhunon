using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool gamePaused = false;
    public GameObject pauseMenu;
    public GameObject battleHud;
    public BattleMaster battleMaster;

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
        Time.timeScale = 0f;
        gamePaused = true;
    }

    public void ExitToMainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
