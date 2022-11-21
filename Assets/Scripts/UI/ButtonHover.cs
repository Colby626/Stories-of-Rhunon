using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameMaster.instance.hoveringOverButton = true; //Possibly should use OnPointerStay
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameMaster.instance.hoveringOverButton = false;
    }
}
