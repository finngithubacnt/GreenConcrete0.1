using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public int x, y; // Grid position
    public Item storedItem; // Reference to the stored item
    public Image itemIcon; // UI image for displaying the item
    public Button button;
    private void Start()
    {
        if (button != null)
            button.onClick.AddListener(OnSlotClicked);
    }

    public void SetGridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool IsEmpty()
    {
        return storedItem == null;
    }

    public void AssignItem(Item item)
    {
        storedItem = item;
        itemIcon.sprite = item.itemIcon;
        itemIcon.enabled = true;
    }

    public void RemoveItem()
    {
        storedItem = null;
        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }

    // Handles when an item is dropped onto this slot
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("testpt1");
        if (ItemDragHandler.itemBeingDragged != null)
        {
            Debug.Log("testpt2");
            InventorySlot previousSlot = ItemDragHandler.itemBeingDragged.GetComponentInParent<InventorySlot>();



            if (previousSlot = null)
            {

                Debug.Log("testpt3");
                // Swap items between slots
                Item tempItem = storedItem;
             //   AssignItem(previousSlot.storedItem);
             //   previousSlot.AssignItem(tempItem);
            }

        }
    }

    private void OnSlotClicked()
    {
        if (storedItem != null)
        {
            storedItem.UseItem(GameObject.FindWithTag("Player"));
        }
    }

}
