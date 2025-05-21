using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GameObject itemBeingDragged;
    public Vector3 startPosition;
    public Transform startParent;
    public Image itemImage;
    public Transform newPos;
    public bool hover;
    Transform rootElement;

    private void Awake()
    {
        
        itemImage = GetComponent<Image>();
        itemImage.raycastTarget = true;

        rootElement = transform.parent.parent.parent.parent; //Canvas element
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        itemBeingDragged = gameObject;
        startPosition = transform.position;
        startParent = transform.parent;

        // Move UI object to the top layer so it appears above everything
        transform.SetParent(rootElement);
        transform.SetAsLastSibling();

        // Make the item slightly transparent when dragging

        itemImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        itemImage.raycastTarget = false;

        Item.dragging = true;
        Item.focusedImg = itemImage.transform;

        transform.position = Input.mousePosition + new Vector3(-10, -10); // Move object with mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;
        itemImage.raycastTarget = true;

        if (transform.parent == rootElement && SlotContainer.hoverDetect.hover)
        {
            SlotContainer.hoverDetect.transform.GetChild(0).GetComponent<Transform>().position = startPosition;
            SlotContainer.hoverDetect.transform.GetChild(0).SetParent(startParent);
        }

        // Return to the original slot if not dropped on another slot
        if (transform.parent == rootElement && !SlotContainer.hoverDetect.hover)
        {
            
            transform.SetParent(startParent);
            transform.position = startPosition;
        }
    }

    public void Update()
    {
        if (Input.GetMouseButtonUp(0) && Item.dragging) //On mouse up update `dragging`
        {
            FrameLate(() => Item.dragging = false);
        }
    }

    IEnumerator FrameLate(System.Action action)
    {
        yield return new WaitForEndOfFrame(); //Wait one frame

        action.Invoke();
    }
}
