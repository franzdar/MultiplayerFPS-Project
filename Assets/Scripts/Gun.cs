using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Item
{
    public abstract override void Use();

    // General Item Functions
    public float waitTillNextFire;
    public float roundsPerSecond;
    public string gunType;
    public AudioSource gunShootSound;

    // Ammunition
    public int ammoCap;
    public int currentAmmo;
    public float reloadTime;

    // On Hit VFX
    public GameObject bulletImpactPrefab;
    public GameObject bloodSplatPrefab;
}
