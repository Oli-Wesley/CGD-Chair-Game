using TMPro;
using UnityEngine;
using System;

public class PlayerText : MonoBehaviour
{
	[SerializeField]
    Canvas GameHUD;
	readonly GameStateManager GameStateManager;

	private int playerCount = 3;
	private TextMeshProUGUI[][] _playerStats;

    private void OnEnable()
    {
        _playerStats = new TextMeshProUGUI[playerCount][];

		UpdatePlayerDisplay(0);
    }

	/*
		Changes player name, score, and inventory to be (in)visible. Should be called each time a player joins the game.
	 */
    public void UpdatePlayerDisplay(int playerID, bool onOrOff = true)
	{
		playerID++;
		//get player text by ID
		GameObject player = GameObject.Find("Player" + playerID);
		
		TextMeshProUGUI thisPlayerText = player.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI[] thisPlayerStats = thisPlayerText.GetComponentsInChildren<TextMeshProUGUI>();
		_playerStats[playerID - 1] = thisPlayerStats;

        if (onOrOff)
		{
			//set player text alpha to 255 (visible)
			thisPlayerStats[0].color = new Color(thisPlayerText.color.r, thisPlayerText.color.g, thisPlayerText.color.b, 255);
			thisPlayerStats[1].color = new Color(thisPlayerText.color.r, thisPlayerText.color.g, thisPlayerText.color.b, 255);
			thisPlayerStats[2].color = new Color(thisPlayerText.color.r, thisPlayerText.color.g, thisPlayerText.color.b, 255);
		}
		else
		{
			//set player text alpha to 0 (invisible)
			thisPlayerStats[0].color = new Color(thisPlayerText.color.r, thisPlayerText.color.g, thisPlayerText.color.b, 0);
			thisPlayerStats[1].color = new Color(thisPlayerText.color.r, thisPlayerText.color.g, thisPlayerText.color.b, 0);
            thisPlayerStats[2].color = new Color(thisPlayerText.color.r, thisPlayerText.color.g, thisPlayerText.color.b, 0);
        }
    }

    public void UpdatePlayerScore(int playerID, int amount = 1) {
		if (_playerStats[playerID] != null) {
			//get score entry then convert to int
			int scoreNum = Int32.Parse(_playerStats[playerID][1].text);

			//add amount
			scoreNum += amount;

			//convert back to entry
			_playerStats[playerID][1].SetText(scoreNum.ToString());
		}
	}

    public void UpdatePlayerInventory(int playerID, int amount = 1)
    {
        if (_playerStats[playerID] != null)
        {
            //get score entry then convert to int
            int invNum = Int32.Parse(_playerStats[playerID][2].text);

            //add amount
            invNum += amount;

            //convert back to entry
            _playerStats[playerID][2].SetText(invNum.ToString());
        }
    }

}
