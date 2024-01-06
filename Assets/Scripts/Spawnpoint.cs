using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    [SerializeField] GameObject graphics;

    // Function to make the spawnpoint invisible
    void Awake()
    {
        graphics.SetActive(false);
    }
}
