using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using InteractionTypes;
using Interaction;
using System.Threading;

public class PlayerScoreManager : MonoBehaviour
{
    [Header("UI References For Player Score")]
    [SerializeField] private List<TextMeshProUGUI> availableUiSlots = new List<TextMeshProUGUI>();

    // DICTIONARY - KEYED PAIR
    private Dictionary<int, int> _playerScores = new Dictionary<int, int>();

    private void Awake()
    {
        // HIDE UI UNTIL PLAYERS JOIN
        foreach (var slot in availableUiSlots.Where(slot => slot != null))
        {
            slot.gameObject.SetActive(false);
        }
    }

    //INTERACTION MANAGER ON PLAYER
    private void OnEnable()
    {
        InteractionManager.OnRubbishDroppedOff += HandleRubbishDroppedOff;
    }

    private void OnDisable()
    {
        InteractionManager.OnRubbishDroppedOff -= HandleRubbishDroppedOff;
    }

    public void RegisterPlayer(int playerID, int initialScore = 0)
    {
        // PLAYER ID AS REF FOR SCORE
        if (playerID >= 0 && playerID < availableUiSlots.Count)
        {
            TextMeshProUGUI slot = availableUiSlots[playerID];
            if (slot != null && !slot.gameObject.activeSelf)
            {
                slot.gameObject.SetActive(true);
                _playerScores.TryAdd(playerID, initialScore);
                UpdateUIText(playerID);
            }
        }
    }

    private void HandleRubbishDroppedOff(RubbishType rubbishType, int playerID)
    {
        if (!_playerScores.ContainsKey(playerID))
        {
            RegisterPlayer(playerID);

            // CHECK THAT PLAYER EXISTS
            _playerScores.TryAdd(playerID, 0);
        }

        _playerScores[playerID]++;
        UpdateUIText(playerID);
    }

    private void UpdateUIText(int playerID)
    {
        if (playerID < 0 || playerID >= availableUiSlots.Count) return;
        var slot = availableUiSlots[playerID];
        if (!slot) return;
        // TRY GET VALUE TO DEAL WITH ERRORS ON PLAYER ID MISS MATCH
        var currentScore = 0;
        _playerScores.TryGetValue(playerID, out currentScore);

        slot.text = $"Player {playerID + 1} Score: {currentScore}";
    }

    // PUBLIC GETTER GOT UI PANEL FOR SCORES
    public int GetPlayerScore(int playerID)
    {
        return _playerScores.GetValueOrDefault(playerID, 0); // Returns 0 if the player hasn't scored or joined yet
    }

    // RESET FUNCTION TO WIPE ALL SCORES
    public void ResetAllScores()
    {
        _playerScores.Clear();
        var count = 0;
        foreach (var slot in availableUiSlots.Where(slot => slot != null))
        {
            //slot.gameObject.SetActive(false);
            UpdateUIText(count++);
        }
    }

    // RETURNS PLAYER ID OF HIGHEST SCORE PLAYER
    public int GetLeadingPlayerID()
    {
        var leadingPlayerID = -1;
        var highestScore = -1;

        foreach (var pair in _playerScores.Where(pair => pair.Value > highestScore))
        {
            highestScore = pair.Value;
            leadingPlayerID = pair.Key;
        }

        return leadingPlayerID;
    }
}