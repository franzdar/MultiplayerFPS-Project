using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera cam;

    void Update()
    {
        if(cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }

        if (cam == null)
            return;

        // Sets the UI Text to always face the cam
        transform.LookAt(cam.transform); 
        transform.Rotate(Vector3.up * 180);
    }
}
