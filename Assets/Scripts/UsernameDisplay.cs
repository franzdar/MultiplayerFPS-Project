using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UsernameDisplay : MonoBehaviour
{
    [SerializeField] PhotonView playerPV;
    [SerializeField] TMP_Text text;

    void Start()
    {
        // Disable Local Player's Username Display
        if(playerPV.IsMine)
        {
            gameObject.SetActive(false);
        }

        // Change the Local Player's Username to their Networked Nickname
        text.text = playerPV.Owner.NickName;
    }
}
