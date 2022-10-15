using TMPro;
using UnityEngine;

public class LevelUpMouseOver : MonoBehaviour
{
    public TextMeshProUGUI correspondingText;
    private int indexToChange;
    private string originalText;
    private int newValue;

    public void ChangeColor()
    {
        indexToChange = correspondingText.text.Length - 1;
        correspondingText = correspondingText.GetComponent<TextMeshProUGUI>();
        originalText = correspondingText.text;

        newValue = int.Parse(correspondingText.text[indexToChange].ToString()) + 1;
        correspondingText.text = correspondingText.text.Replace(correspondingText.text[indexToChange].ToString(), newValue.ToString());
        correspondingText.text = correspondingText.text.Replace(correspondingText.text[indexToChange].ToString(), "<color=green>" + correspondingText.text[indexToChange].ToString() + "</color>");
    }

    public void DefaultColor()
    {
        correspondingText.text = originalText;
    }
}
