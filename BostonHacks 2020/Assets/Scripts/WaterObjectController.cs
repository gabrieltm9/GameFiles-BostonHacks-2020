using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterObjectController : MonoBehaviour
{
    public GameController gc;
    public bool defaultOn;

    public void ToggleWater()
    {
        if (defaultOn)
        {
            gc.ChangePollutionVals(2, -3);
            defaultOn = false;
        }

        if (transform.GetChild(0).GetComponent<ParticleSystem>().isPlaying)
            transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        else
            transform.GetChild(0).GetComponent<ParticleSystem>().Play();
    }

    public void SetWaterOn()
    {
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        defaultOn = true;
    }
}
