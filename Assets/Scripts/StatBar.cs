using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    public Slider slider;

    public void SetBar(int stat)
    {
        slider.value = stat;
    }

    public void SetBarMax(int stat)
    {
        slider.maxValue = stat;
        slider.value = stat;
    }

    private void Start()
    {
        //nameText.text = name;
    }
    void Update()
    {
        //healthText.text = "HP: " + GetComponent<CharacterSheet>().Health.ToString();
        //healthbar.fillAmount = (float)GetComponent<CharacterSheet>().Health / (float)GetComponent<CharacterSheet>().MaxHealth;
    }

}
