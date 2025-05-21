using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject

{
    public static bool dragging;
    public static Transform focusedImg;

    public string itemName;
    public Sprite itemIcon;
    public int width = 1;
    public int height = 1;
    public string scriptName; // The name of the script to execute when clicked (for equipabbles or buildable etc)

      public void UseItem(GameObject user)
    {
        if (!string.IsNullOrEmpty(scriptName))
        {
            System.Type scriptType = System.Type.GetType(scriptName);
            if (scriptType != null)
            {
                MonoBehaviour scriptInstance = user.GetComponent(scriptType) as MonoBehaviour;
                if (scriptInstance != null)
                {
                    scriptInstance.Invoke("Execute", 0f);
                }
                else
                {
                    Debug.LogWarning($"User does not have {scriptName} attached.");
                }
            }
            else
            {
                Debug.LogWarning($"Script {scriptName} not found.");
            }
        }
    }
}

    //Float or pos needs to be set too individual collum and row positoin
