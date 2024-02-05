using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public bool isAutomatic;
    public float timeBetweenShots = 0.1f, heatParShot = 1f;
    public GameObject muzzelFlash;
    public int ShotDamage;
    public float adsZoom;
}
