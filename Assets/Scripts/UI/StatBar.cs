using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI barText;
    //public TextMeshProUGUI nameText;
    //public TextMeshProUGUI healthText;
    //public Image healthbar;

    public void SetBar(int stat)
    {
        slider.value = stat;
        barText.text = stat + "/" + slider.maxValue;
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
