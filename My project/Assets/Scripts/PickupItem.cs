using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public Item itemData;
    public bool isHovered = false;

    public void OnHoverEnter()
    {
        if (!isHovered)
        {
            
            isHovered = true;
            PickupUI.Instance?.ShowMessage("Press E to pick up " + itemData.itemName);
        }
    }

    public void OnHoverExit()
    {
        if (isHovered)
        {
            isHovered = false;
            PickupUI.Instance?.HideMessage();
        }
    }

    public void PickUpItem()
    {
        if (InventoryManager.Instance.AddItemToInventory(itemData))
        {
            Debug.Log("Item picked up dumb ahh");
            PickupUI.Instance?.HideMessage();
            Destroy(gameObject);
        }
    }
}
