using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public void PlayButton()
    {
        SceneManager.LoadScene("ColbyDemo");
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
