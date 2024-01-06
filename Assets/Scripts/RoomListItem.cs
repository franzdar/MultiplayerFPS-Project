using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text text; //Reference to Room Name Text 

    public RoomInfo roomInfo; // Placeholder Variable for Room Info

    // Setup Room Info 
    public void SetUp(RoomInfo info)
    {
        roomInfo = info;
        text.text = info.Name;
    }

    // Function to Join Room once the player clicked the item
    public void OnClick()
    {
        Launcher.Instance.JoinRoom(roomInfo);
    }
}
