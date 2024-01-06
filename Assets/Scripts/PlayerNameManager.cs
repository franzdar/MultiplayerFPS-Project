using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameManager : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameInput;

    [SerializeField] Button createRoomButton;
    [SerializeField] Button findRoomButton;

    void Update()
    {
        // Checks if the local player has a saved nickname
        if (PlayerPrefs.GetString("username") == "") // blank nickname saved
        {
            createRoomButton.interactable = false;
            findRoomButton.interactable = false;
        }

        if(PlayerPrefs.HasKey("username")) // has nickname saved
        {
            // Set the saved nickname
            usernameInput.text = PlayerPrefs.GetString("username");
            OnUsernameInputValueChanged();

            // Enable Room Buttons
            createRoomButton.interactable = true;
            findRoomButton.interactable = true;
        }
        else // no nickname saved
        {
            createRoomButton.interactable = false;
            findRoomButton.interactable = false;
        }
    }

    public void OnUsernameInputValueChanged()
    {
        PhotonNetwork.NickName = usernameInput.text;
        PlayerPrefs.SetString("username", usernameInput.text);
    }
}
