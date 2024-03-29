using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI barText;
    public bool wantText;

    public void SetBar(int stat)
    {
        slider.value = stat;
        if (wantText)
        {
            barText.text = stat + "/" + slider.maxValue;
        }
    }

    public void SetBarMax(int stat)
    {
        slider.maxValue = stat;
        slider.value = stat;
    }
}
