using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    public Slider slider;

    public void SetHealth(int health)
    {
        slider.value = health;
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    //void Update()
    //{
    //    nameText.text = name;
    //    healthText.text = "HP: " + GetComponent<CharacterSheet>().Health.ToString();
    //    healthbar.fillAmount = (float)GetComponent<CharacterSheet>().Health / (float)GetComponent<CharacterSheet>().MaxHealth;
    //}

}
