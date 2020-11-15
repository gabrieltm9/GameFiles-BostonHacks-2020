using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    public Image image;
    public Color color;
    
    // Update is called once per frame
    void Update()
    {
        color = Color.Lerp(Color.red, Color.green, GetComponent<Slider>().value);
        image.color = color;
    }
}