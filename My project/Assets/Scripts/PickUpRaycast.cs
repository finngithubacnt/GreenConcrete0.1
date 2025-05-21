using UnityEngine;

public class PickUpRaycast : MonoBehaviour
{
    public float pickupRange = 3f;
    private PickupItem currentItem = null;

    void Update()
    {
        Camera cam = Camera.main;
        Vector3 rayOrigin = cam.transform.position;
        Vector3 rayDirection = cam.transform.forward;

        //raycast vision
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, pickupRange))
        {
            Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.green); // visable ray when hitting an object
            Debug.Log(hit);
            PickupItem item = hit.collider.GetComponent<PickupItem>();

            if (item != null)
            {
                if (currentItem != item)
                {
                    currentItem?.OnHoverExit();
                    currentItem = item;
                    currentItem.OnHoverEnter();
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    currentItem?.PickUpItem();
                }
            }
            else
            {
                currentItem?.OnHoverExit();
                currentItem = null;
            }
        }
        else
        {
            Debug.DrawRay(rayOrigin, rayDirection * pickupRange, Color.red); // Visible ray when not hitting anything
            currentItem?.OnHoverExit();
            currentItem = null;
        }
    }
}
