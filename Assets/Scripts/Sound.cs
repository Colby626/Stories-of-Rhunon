using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    [HideInInspector]
    public enum TypeOfSound { Music, SoundEffect };

    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
    [Range(0.1f, 3f)]
    public float pitch;
    public bool loop = false;
    public TypeOfSound type;

    [HideInInspector]
    public AudioMixerGroup audioMixer;
    [HideInInspector]
    public AudioSource audioSource;
}
