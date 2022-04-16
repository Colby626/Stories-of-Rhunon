using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public Image healthbar;
    public TMPro.TextMeshProUGUI nameText;
    public TMPro.TextMeshProUGUI healthText;

    void Update()
    {
        nameText.text = name;
        healthText.text = "HP: " + GetComponent<CharacterSheet>().Health.ToString();
        healthbar.fillAmount = (float)GetComponent<CharacterSheet>().Health / (float)GetComponent<CharacterSheet>().MaxHealth;
    }

}
