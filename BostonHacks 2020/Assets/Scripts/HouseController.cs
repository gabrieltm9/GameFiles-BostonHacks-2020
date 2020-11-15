using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HouseController : MonoBehaviour
{
    public string userDataID;

    public Transform exitSpawnpoint;

    public ParticleSystem waterLeak;
    public ParticleSystem smokeStack1;
    public ParticleSystem smokeStack2;

    public TMP_Text nameText;
    public TMP_Text scoreText;
}