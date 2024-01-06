using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;

    GameObject controller; // placeholder variable for instantiated player controller

    int kills; // Reference to kill count
    int deaths; // Reference to death count

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    
    // Create Local Player's Own Player Controller
    void Start()
    {
        if(PV.IsMine)
        {
            CreateController();
        }
    }

    // Instantiate the Local Player's Player Controller
    void CreateController()
    {
        // Gets the spawn point of the player controller to be instantiated
        Transform spawnpoint = SpawnManager.instance.GetSpawnPoint(); 

        // Instantiates the player controller
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
    }

    // Player Death function
    public void Die()
    {
        PhotonNetwork.Destroy(controller); // Destroy Local Player Controller
        CreateController(); // Respawn

        deaths++;

        Hashtable hash = new Hashtable();
        hash.Add("deaths", deaths);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    // Gives the player a kill
    public void GetKill()
    {
        PV.RPC(nameof(RPC_GetKill), PV.Owner);
    }

    [PunRPC]
    void RPC_GetKill()
    {
        kills++;

        Hashtable hash = new Hashtable();
        hash.Add("kills", kills);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    // Identifies the Player Manager of all players 
    public static PlayerManager Find(Player player)
    {
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner == player);
    }
}
