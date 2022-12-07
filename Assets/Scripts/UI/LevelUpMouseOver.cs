using TMPro;
using UnityEngine;

public class LevelUpMouseOver : MonoBehaviour
{
    public TextMeshProUGUI correspondingText;
    private string originalText;
    private int finalIndex;
    private int onesPlace;
    private int tensPlace;
    private bool hasTens;

    private void ChangeColor()
    {
        originalText = correspondingText.text;
        finalIndex = correspondingText.text.Length - 1;

        hasTens = int.TryParse(correspondingText.text[finalIndex - 1].ToString(), out tensPlace);
        if (hasTens)
        {
            onesPlace = int.Parse(correspondingText.text[finalIndex].ToString()) + 1;
            if (onesPlace == 0)
            {
                tensPlace += 1;
            }
            if (onesPlace == 10)
            {
                tensPlace += 1;
                onesPlace = 0;
            }

            correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex-1].ToString(), tensPlace.ToString());
            correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex].ToString(), onesPlace.ToString());
            finalIndex = correspondingText.text.Length - 1;
            correspondingText.text = correspondingText.text.Insert(finalIndex - 1, "<color=#49AE04>");
            finalIndex = correspondingText.text.Length;
            correspondingText.text = correspondingText.text.Insert(finalIndex, "</color>");
        }
        else //Doesn't have a tens place
        {
            onesPlace = int.Parse(correspondingText.text[finalIndex].ToString()) + 1;
            if (onesPlace == 10) //The ones place was 9 and so now it has a tens place 
            {
                correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex].ToString(), "<color=#49AE04>10</color>");
            }
            else
            {
                correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex].ToString(), onesPlace.ToString());
                correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex].ToString(), "<color=#49AE04>" + correspondingText.text[finalIndex].ToString() + "</color>");
            }
        }
    }

    private void LevelupPressed()
    {
        ChangeColor();
    }

    private void DefaultColor()
    {
        correspondingText.text = originalText;
    }
}
