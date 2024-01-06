using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text text; // Reference to Player Name Text 
    Player player; // Placeholder Variable for Local Player 

    // Setup Player Info
    public void SetUp(Player _player)
    {
        player = _player;
        text.text = _player.NickName;
    }

    // Destroy other player info once they leave 
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    // Destroy own player info once we leave
    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
