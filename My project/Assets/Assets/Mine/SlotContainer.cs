using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotContainer : MonoBehaviour
{

    // Save initial parent slot container

    public Transform initSlot;

    public static HoverDetect hoverDetect; //Ref to get hover bool

    bool deb;

    void Awake()
    {
        initSlot = transform.parent;
    }

    void Update()
    {
        for (int i = 0; i < transform.parent.parent.childCount; i++)
        {
            var thisHover = transform.parent.parent.GetChild(i).GetComponent<HoverDetect>();

            if (!thisHover) return; //NOTHING!!

            if (thisHover.hover) 
            {
                hoverDetect = thisHover;
                //print(i);
            }
        }

        print(deb);

        if (Input.GetMouseButtonUp(0) && hoverDetect && !deb)
        {
            StartCoroutine(Cooldown(1));

            if (!hoverDetect.hover && !Item.dragging) return;

            print("REPLACE THIS!!");

            Item.focusedImg.SetParent(hoverDetect.transform);
            Item.focusedImg.transform.localPosition = Vector3.zero; // Reset pos to slot
            
           
           


            // Swap use var of init slot container to swop them
            // After swop update to new init slot container var
        }

    }

    IEnumerator Cooldown(float wait)
    {
        deb = true;

        yield return new WaitForSeconds(wait);

        deb = false;
    }
}
