using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverDetect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hover;

    // Hover events

    public void OnPointerEnter(PointerEventData e)
    {
        hover = true;
    }

    public void OnPointerExit(PointerEventData e)
    {
        hover = false;
    }

}
