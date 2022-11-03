using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayNextSong : MonoBehaviour
{
    public PercentageRandomness Randomness;
    public AudioSource audioSource;

    public void ChangeMusic()
    {
        audioSource.clip = Randomness.PickSound().clip;
        audioSource.Play();
    }
}
