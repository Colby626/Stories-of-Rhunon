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

    public void ChangeColor() //Requires check for the ones place being 9
    {
        originalText = correspondingText.text;
        finalIndex = correspondingText.text.Length - 1;
        correspondingText = correspondingText.GetComponent<TextMeshProUGUI>();

        hasTens = int.TryParse(correspondingText.text[finalIndex - 1].ToString(), out tensPlace);
        if (hasTens)
        {
            onesPlace = int.Parse(correspondingText.text[finalIndex].ToString()) + 1;
            if (onesPlace == 10)
            {
                tensPlace += 1;
                onesPlace = 0;
            }

            correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex-1].ToString(), tensPlace.ToString());
            correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex].ToString(), onesPlace.ToString());
            correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex-1].ToString(), "<color=green>" + correspondingText.text[finalIndex-1].ToString() + "</color>");
            //O is ones place, T is tens place
            //Strength: <color=green>T</color>O
            //finalIndex ^
            finalIndex = correspondingText.text.Length - 1;
            //Strength: <color=green>T</color>O
            //finalIndex                      ^
            correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex].ToString(), "<color=green>" + correspondingText.text[finalIndex].ToString() + "</color>");
        }
        else //Doesn't have a tens place
        {
            onesPlace = int.Parse(correspondingText.text[finalIndex].ToString()) + 1;
            if (onesPlace == 10) //The ones place was 9 and so now it has a tens place 
            {
                correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex].ToString(), "<color=green>10</color>");
            }
            else
            {
                correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex].ToString(), onesPlace.ToString());
                correspondingText.text = correspondingText.text.Replace(correspondingText.text[finalIndex].ToString(), "<color=green>" + correspondingText.text[finalIndex].ToString() + "</color>");
            }
        }
    }

    public void DefaultColor()
    {
        correspondingText.text = originalText;
    }
}
