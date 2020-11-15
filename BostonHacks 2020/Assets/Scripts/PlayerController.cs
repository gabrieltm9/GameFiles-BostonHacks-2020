using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameController gc;

    Ray ray;
    RaycastHit hit;

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 3))
        {
            if (hit.transform.tag == "EnterDoor" || hit.transform.tag == "ExitDoor" || hit.transform.GetComponent<LightController>() != null || hit.transform.GetComponent<WaterObjectController>() != null)
                gc.fakeCursor.enabled = true;
            else
                gc.fakeCursor.enabled = false;

            if (Input.GetMouseButtonDown(0))
            {
                //Door controls
                if (hit.transform.tag == "EnterDoor" && hit.transform.GetComponentInParent<HouseController>().userDataID == gc.fbm.user.UserId)
                    gc.GoIndoors();
                else if (hit.transform.tag == "ExitDoor")
                    gc.GoOutdoors();
                else if (hit.transform.GetComponent<LightController>() != null)
                    hit.transform.GetComponent<LightController>().ToggleLight();
                else if (hit.transform.GetComponent<WaterObjectController>() != null)
                    hit.transform.GetComponent<WaterObjectController>().ToggleWater();
            }
        }
    }
}
