using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // Singleton instance!!

    public GameObject inventoryUI;  // Reference inventory UI panal
    public GridLayoutGroup gridLayout;  // Grid to hold items
    public GameObject slotPrefab;  // Slot prefab for inventory

    public int columns = 5; // Number of columns in the grid
    public int rows = 4; // Number of rows
    private InventorySlot[,] slots; // 2D array to hold inventory slots

    public bool isInventoryOpen = true; // Tracks whether the inventory is open

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Assign singleton instance
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CreateGrid();
       
        
    }

    public void Update()
    {
        
        // Toggle inventory when pressing "I"
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
            Debug.Log("InventoryToggled");
        }
    }

    void CreateGrid()
    {
        
        slots = new InventorySlot[columns, rows];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject newSlot = Instantiate(slotPrefab, gridLayout.transform);
                InventorySlot slotComponent = newSlot.GetComponent<InventorySlot>();
                slotComponent.SetGridPosition(x, y); 
                slots[x, y] = slotComponent;
            }
        }
    }

    // Toggle inventory visibility
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryUI.GetComponent<CanvasGroup>().enabled = (isInventoryOpen);

        if (isInventoryOpen == false)
        {
            Cursor.lockState = CursorLockMode.None; // Unhide and Unlock the cursor
        }

        if (isInventoryOpen == true)
        {
            Cursor.lockState = CursorLockMode.Locked; // Hide and Lock the cursor
        }
    }

    // AddItemToInventory Implementation
    public bool AddItemToInventory(Item newItem)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (slots[x, y].IsEmpty())
                {
                    slots[x, y].AssignItem(newItem);
                    return true;
                }
            }
        }
        return false; // No space available
    }

    

}
