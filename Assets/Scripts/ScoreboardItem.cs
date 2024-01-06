using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEditor;

public class ScoreboardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text usernameText;
    public TMP_Text killsText;
    public TMP_Text deathsText;

    Player player;

    // Setups the player information to the UI Text of their Scoreboard Item Prefab
    public void Initialize(Player player)
    {
        usernameText.text = player.NickName;

        this.player = player;
        UpdateStats();
    }

    // Update player's scoreboard stats
    void UpdateStats()
    {
        // Updates player's custom properties for kills
        if(player.CustomProperties.TryGetValue("kills", out object kills))
        {
            killsText.text = kills.ToString();
        }

        // Updates player's custom properties for deaths
        if (player.CustomProperties.TryGetValue("deaths", out object deaths))
        {
            deathsText.text = deaths.ToString();
        }
    }

    // Player custom properties update 
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(targetPlayer == player) // Finds the specific player to update 
        {
            if(changedProps.ContainsKey("kills") || changedProps.ContainsKey("deaths"))
            {
                UpdateStats();
            }
        }
    }
}
