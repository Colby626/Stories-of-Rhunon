using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixerGroup masterMixer;
    public AudioMixerGroup musicMixer;
    public AudioMixerGroup soundEffectMixer;
    public Sound[] sounds;

    public static AudioManager instance;

    private int EricsInt;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);

        foreach (Sound sound in sounds)
        {
            sound.audioSource = gameObject.AddComponent<AudioSource>();
            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = sound.loop;
            if (sound.type == Sound.TypeOfSound.Music)
            {
                sound.audioSource.outputAudioMixerGroup = musicMixer;
            }
            else
            {
                sound.audioSource.outputAudioMixerGroup = soundEffectMixer;
            }
            
        }
    }

    private void Start()
    {
        if (SceneManager.GetSceneByName("MainMenu").isLoaded)
        {
            Play("MainMenuMusic");
        }

        else if (SceneManager.GetSceneByName("GrassPlains_1").isLoaded)
        {
            Play("ExploringMusic");
        }

        else if (SceneManager.GetSceneByName("ColbyDemo").isLoaded)
        {
            Play("BattleMusic");
        }

        //Everything under this within Start is for Eric
        else
        {
            EricsInt = 1;
            Play("MainMenuMusic");
        }
    }

    public void Play(string name)
    {
        Sound sound = Array.Find(sounds, sound=>sound.name == name);
        if (sound != null)
        {
            sound.audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Sound " + name + " was not found");
        }
    }

    public void Stop(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if (sound != null)
        {
            sound.audioSource.Stop();
        }
        else
        {
            Debug.LogWarning("Sound " + name + " was not found");
        }
    }

    //This function is for Eric
    public void NextSong()
    {
        switch (EricsInt)
        {
            case 1:
                Stop("MainMenuMusic");
                Play("ExploringMusic");
                EricsInt++;
                break;

            case 2:
                Stop("ExploringMusic");
                Play("BattleMusic");
                EricsInt++;
                break;

            case 3:
                Stop("BattleMusic");
                Play("MainMenuMusic");
                EricsInt = 1;
                break;
        }
    }
}
