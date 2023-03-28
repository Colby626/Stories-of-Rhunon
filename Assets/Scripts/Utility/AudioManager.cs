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

        else if (SceneManager.GetSceneByName("GrassPlains_1").isLoaded || SceneManager.GetSceneByName("GrassPlainsDemo").isLoaded)
        {
            Play("ExploringMusic");
        }

        else if (SceneManager.GetSceneByName("ColbyDemo").isLoaded)
        {
            Play("BattleMusic");
        }
    }

    public void Play(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
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

    public void StopAll()
    {
        foreach (Sound sound in sounds)
        {
            sound.audioSource.Stop();
        }
    }
}