using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class Scoreboard : MonoBehaviourPunCallbacks 
{
    [SerializeField] Transform container; // Reference to Scoreboard's Vertical Layout Group Position
    [SerializeField] GameObject scoreboardItemPrefab; // Reference to Scoreboard Item Prefab
    [SerializeField] CanvasGroup canvasGroup; // Reference to Scoreboard Canvas Group 

    // Creates a new dictionary to store scoreboard items
    Dictionary<Player, ScoreboardItem> scoreboardItems = new Dictionary<Player, ScoreboardItem>();

    // Create a scoreboard item for the players
    void Start()
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            AddScoreboardItem(player);
        }
    }

    // Updates the scoreboard list if a player joins after the game started 
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddScoreboardItem(newPlayer);
    }

    // Removes the scoreboard item of a player who left the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemoveScoreboardItem(otherPlayer);
    }
    
    // Function to instantiate a scoreboard item 
    void AddScoreboardItem(Player player)
    {
        ScoreboardItem item = Instantiate(scoreboardItemPrefab, container).GetComponent<ScoreboardItem>();
        item.Initialize(player);
        scoreboardItems[player] = item; // Add to dict
    }

    // Function to destroy a scoreboard item
    void RemoveScoreboardItem(Player player)
    {
        Destroy(scoreboardItems[player].gameObject);
        scoreboardItems.Remove(player); // Remove from dict
    }

    // Shows and Hides the scoreboard when tab key is clicked
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)) // Shows the scoreboard
        {
            canvasGroup.alpha = 1;
        }
        else if(Input.GetKeyUp(KeyCode.Tab)) // Hides the scoreboard
        {
            canvasGroup.alpha = 0;
        }
    }
}
