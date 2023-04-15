using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Item item;
    public Image icon;
    public GameObject blurbHolder;

    public void AddItem(Item newItem)
    {
        item = newItem;
        icon.sprite = newItem.icon;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
    }

    // Use the item
    public void UseItem()
    {
        if (item != null)
        {
            RemoveBlurb();
            if (transform.parent.GetComponent<ChestUI>())
            {
                item.MoveFromChestToInventory(item);
            }
            else
            {
                item.Use();
            }
        }
    }

    public void Unequip()
    {
        if (item != null)
        {
            RemoveBlurb();
            BattleMaster battleMaster = transform.parent.GetComponent<EquipmentManager>().battleMaster;
            if (battleMaster.chestMenu.activeSelf)
            {
                battleMaster.chestEquipmentManager.Unequip((Equipment)item);
            }
            else
            {
                battleMaster.equipmentManager.Unequip((Equipment)item);
            }
            
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null)
        {
            DisplayBlurb();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (item != null)
        {
            RemoveBlurb();
        }
    }

    private void DisplayBlurb()
    {
        blurbHolder.SetActive(true);
        blurbHolder.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.blurb;
    }

    private void RemoveBlurb()
    {
        blurbHolder.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = null;
        blurbHolder.SetActive(false);
    }
}
