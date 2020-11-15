using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public GameController gc;
    public bool defaultOn;

    public void ToggleLight()
    {
        if(defaultOn)
        {
            gc.ChangePollutionVals(1, -2);
            defaultOn = false;
        }

        transform.GetChild(0).GetComponent<Light>().enabled = !transform.GetChild(0).GetComponent<Light>().enabled;
    }

    public void SetLightOn()
    {
        transform.GetChild(0).GetComponent<Light>().enabled = true;
        defaultOn = true;
    }
}
