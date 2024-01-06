using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam; // Reference to Local Player Camera

    PhotonView PV; // Reference to Photon View of Item/Gun 
    int objectHit; // Reference to the type of Object hit

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        waitTillNextFire = 1f;
    }

    void Update()
    {
        if(waitTillNextFire > 0f)
        {
            waitTillNextFire -= roundsPerSecond * Time.deltaTime;
        }
    }

    public override void Use()
    {
        if(waitTillNextFire <= 0)
        {
            Shoot();
            currentAmmo -= 1; // reduce ammo

            if (gunShootSound)
            {
                gunShootSound.Play(); 
            }

            Debug.Log("Shooting Gun" + itemInfo.itemName);
            waitTillNextFire = 1f;
        }
    }

    void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f)); // Always shoot from the center of the screen 
        ray.origin = cam.transform.position; // Re-position ray to where the camera is looking

        // Identifies if the local player hits another player
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            // Specifies the amount of damage the other player target takes
            hit.collider.gameObject.GetComponent<IDamageable>()?.DealDamage(((GunInfo)itemInfo).damage);
            Debug.Log("We hit " + hit.collider.gameObject.name);

            if(hit.collider.gameObject.name == "PlayerController(Clone)") // Blood Splatter
            {
                objectHit = 1;
            }    
            else // Bullet Impacts 
            {
                objectHit = 0;
            }

            // Generates Bullet Impacts / Blood Splatter at Ray Hit Position
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
        }
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal) // Instantiate on Normal Face of Objects 
    {
        // Identify the collider that the Bullet Impact is touching
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f); 
        if(colliders.Length != 0)  
        {
            if (objectHit == 1) // Player hit
            {
                // Generate Blood Splatter
                GameObject bloodSplatObj = Instantiate(bloodSplatPrefab, hitPosition + hitNormal * 0.1f,
                    Quaternion.LookRotation(hitNormal, Vector3.up) * bloodSplatPrefab.transform.rotation);
                bloodSplatObj.transform.SetParent(colliders[0].transform);
            }
            else if (objectHit == 0) // Object hit
            {
                // Generate Bullet Impacts
                GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f,
                    Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
                Destroy(bulletImpactObj, 5f); // Remove Bullet Impact after a certain amount of time
                bulletImpactObj.transform.SetParent(colliders[0].transform);
            }
            else
                return;
        }

        //Debug.Log(hitPosition);
    }
}
