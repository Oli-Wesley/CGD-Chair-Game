using UnityEngine;

public class PlayerStatusUI : MonoBehaviour
{
    [System.Serializable]
    public class PlayerSlot
    {
        public GameObject notJoined; // e.g. "Player1"
        public GameObject joined;    // e.g. "Player1_joined"
    }

    [SerializeField] private PlayerSlot[] slots = new PlayerSlot[4];

    void Start()
    {
        // everyone starts as "not joined"
        for (int i = 0; i < slots.Length; i++)
        {
            SetSlot(i, false);
        }
    }

    public void UpdatePlayerStatusUI(int playerID)
    {
        SetSlot(playerID, true);
    }

    // optional: use this if a player can leave
    public void SetSlot(int playerID, bool hasJoined)
    {
        if (playerID < 0 || playerID >= slots.Length) return;

        slots[playerID].notJoined.SetActive(!hasJoined);
        slots[playerID].joined.SetActive(hasJoined);
    }
}