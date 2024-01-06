using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] Image healthBarImage; // Reference to Health Bar UI 
    [SerializeField] Image reloadBarImage; // Reference to Reload Bar UI
    [SerializeField] GameObject ui; // Reference to Player's Canvas

    [SerializeField] GameObject cameraHolder; // Reference to Camera Holder

    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime; // Editable Control variables

    [SerializeField] Item[] items; // Reference to Items
    [SerializeField] CanvasGroup[] itemsUI; // Reference to Items UI
    [SerializeField] CanvasGroup reloadUI; // Reference to Reloading UI
    [SerializeField] GameObject leaveGameUI; // Reference to Leave Game UI (Esc)
                                          
    [SerializeField] TMP_Text ammoCapUI; // Reference to Ammo Cap UI
    [SerializeField] TMP_Text currentAmmoUI; // Reference to Current Ammo UI
    float waitToReload; // placeholder variable for reload time
    bool isReloading = false; // identifies if the player is reloading; default not reloading
    public AudioSource gunChangeSound;
    public AudioSource gunReloadSound;

    // Reference to Item Indexes
    int itemIndex;
    int previousItemIndex = -1; // default value -1 if player hasn't switched any items yet

    // Other Player Control Variables
    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb; // Reference to Local Player Rigidbody

    PhotonView PV; // Reference to Local Player PhotonView

    // Reference to Player Stats
    const float maxHealth = 100f;
    float currentHealth = maxHealth;

    PlayerManager playerManager; // reference to Local Player's Player Manager

    void Awake()
    {
        // Get Local Player's Rigidbody and PhotonView
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        // Find the Local Player's Instantiation Data
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>(); 
    }

    void Start()
    {
        // Set cursor to not be visible
        Cursor.visible = false;

        // Equip Default Item
        if(PV.IsMine)
        {
            EquipItem(0);
        }
        // Destroy other player's components if PhotonView is not the Local Player's
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject); // Camera
            Destroy(rb); // Rigid body
            Destroy(ui); // Canvas
        }
    }

    void Update()
    {
        // Only Update Local Player Functions
        if (!PV.IsMine)
            return;

        Look();
        Move();
        Jump();

        UpdateAmmoUI();

        // Open Leave Room UI
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            leaveGameUI.gameObject.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // Automatically Reload Gun when magazine is empty or Manual Reload using R Button
        if (Input.GetKeyDown(KeyCode.R) || items[itemIndex].GetComponent<Gun>().currentAmmo == 0)
        {
            if(items[itemIndex].GetComponent<Gun>().currentAmmo == items[itemIndex].GetComponent<Gun>().ammoCap)
            {
                return;
            }
            else
            {
                if (gunReloadSound)
                    gunReloadSound.Play();
                isReloading = true;
            }    
        }

        // Start Reloading Function
        if (isReloading)
        {
            reloadUI.alpha = 1.0f; // Make reload UI visible
            Reload();
        }
            
        // Set Reload Time when magazine is full or after reloading
        if (items[itemIndex].GetComponent<Gun>().currentAmmo == items[itemIndex].GetComponent<Gun>().ammoCap)
        {
            waitToReload = items[itemIndex].GetComponent<Gun>().reloadTime;
            reloadUI.alpha = 0f; // Make reload UI invisible
        }

        // Equip Item based on Player Number Input 
        for (int i = 0; i < items.Length; i++)
        {
            if(Input.GetKeyDown((i+1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        // Equip Item based on Player Schroll Wheel Input
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f) 
        {
            // Constraint to prevent player from going beyond the current Item List Index
            // Equips the First Item instead
            if(itemIndex >= items.Length - 1)
            {
                EquipItem(0);
            }
            // Equip Next Item
            else
            {
                EquipItem(itemIndex + 1);
            }
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f) 
        {
            // Constraint to prevent player from going beyond the current Item List Index
            // Equips the Last Item instead
            if (itemIndex <= 0)
            {
                EquipItem(items.Length - 1);
            }
            // Equip Previous Item
            else
            {
                EquipItem(itemIndex - 1);
            }
        }

        // LMB to use item
        if(itemIndex == 0) // Auto Rifle
        {
            if (!isReloading && Input.GetMouseButton(0))
            {
                items[itemIndex].Use();
            }
        }
        else if(itemIndex == 1) // Pistol
        {
            if (!isReloading && Input.GetMouseButtonDown(0))
            {
                items[itemIndex].Use();
            }
        }


        // Kills the player if player falls off the map
        if(transform.position.y < -10f)
        {
            Die();
        }
    }
        


    // Look Around Function
    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    // Move Function
    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }

    // Jump Function
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    // Equip Item Function
    void EquipItem(int _index)
    {
        if(!isReloading)
        {
            // Play Pull Weapon SFX
            if (gunChangeSound)
                gunChangeSound.Play();

            // Prevents the player to disable currently equipped item by clicking the same number twice
            if (_index == previousItemIndex)
                return;

            itemIndex = _index; // Set itemIndex to the equipped item's index

            items[itemIndex].itemGameObject.SetActive(true); // Make the equipped item visible
            itemsUI[itemIndex].alpha = 1f; // Make the UI of the equipped item visible

            // Condition if local player already switched item
            if (previousItemIndex != -1)
            {
                items[previousItemIndex].itemGameObject.SetActive(false);
                itemsUI[previousItemIndex].alpha = 0.3f; // Make the UI of the unequipped item barely visible
            }

            // Change the Previous Item Index with the Item Index of the Currently Equipped Item
            previousItemIndex = itemIndex;

            // Create the Local Player's Hashtable for Item Indexes
            if (PV.IsMine)
            {
                Hashtable hash = new Hashtable();
                hash.Add("itemIndex", itemIndex);
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            }
        }
        else
        {
            Debug.Log("Reloading... Cannot Switch");
        }
        
    }

    // Update Ammo UI
    void UpdateAmmoUI()
    {
        ammoCapUI.text = items[itemIndex].GetComponent<Gun>().ammoCap.ToString();
        currentAmmoUI.text = items[itemIndex].GetComponent<Gun>().currentAmmo.ToString();
    }

    // Reload Current Gun
    void Reload()
    {
        waitToReload -= Time.deltaTime;
        reloadBarImage.fillAmount = waitToReload / items[itemIndex].GetComponent<Gun>().reloadTime;
        Debug.Log("Reloading" + waitToReload);

        if (waitToReload <= 0f)
        {
            items[itemIndex].GetComponent<Gun>().currentAmmo = items[itemIndex].GetComponent<Gun>().ammoCap;
            isReloading = false;
        }
    }

    // Identifies other player's properties update
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // Shows the equipped item or item switched by other players 
        if (changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    // Identifies the player's grounded state
    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    // Fixed update for Local Player's Rigidbody
    void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    // This function runs to the player shooting
    public void DealDamage(float damage)
    {
        PV.RPC(nameof(RPC_TakeDamage), PV.Owner, damage); // Send the damage info across the network
        Debug.Log("Damage dealt: " + damage);
    }

    // This RPC runs to everyone in the network but is only limited to the one receiving the shot
    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        currentHealth -= damage; // Reduce the player's hp according to the damage received

        healthBarImage.fillAmount = currentHealth / maxHealth; // Update the Local Player's Health in percentage

        // Kills the local player if they lose all of their health
        if(currentHealth <= 0)
        {
            Die();

            // Finds the player who made this RPC Call to the local player
            // Gives that player a kill
            PlayerManager.Find(info.Sender).GetKill();  
        }

        Debug.Log("Damage taken: " + damage);
    }

    // Function for player death
    void Die()
    {
        playerManager.Die();
    }
    
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("MainMenu");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
    }

    public void Continue()
    {
        leaveGameUI.gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
