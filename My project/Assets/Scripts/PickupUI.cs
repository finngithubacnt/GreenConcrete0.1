using TMPro;
using UnityEngine;

public class PickupUI : MonoBehaviour
{
    public Item itemData; // The item data (ScriptableObject)
    private bool isPlayerInRange = false;
    public static PickupUI Instance;
    public TextMeshProUGUI pickupText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowMessage(string message)
    {
        pickupText.text = message;
        pickupText.gameObject.SetActive(true);
    }

    public void HideMessage()
    {
        pickupText.text = "";
        pickupText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (InventoryManager.Instance.AddItemToInventory(itemData))
            {
                PickupUI.Instance.HideMessage();
                Destroy(gameObject); // Remove the item from the world
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            PickupUI.Instance.ShowMessage("Press E to pick up " + itemData.itemName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            PickupUI.Instance.HideMessage();
        }
    }
}
