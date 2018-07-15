using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntryForm : MonoBehaviour
{

    public Text RoomName;
    public Text PlayerName;
    public Toggle Player1Toggle;
    public Button PlayButton;

    private static GameObject EntryFormContainer;

    void Start()
    {
        EntryFormContainer = GameObject.Find(GameConstants.ONLINE_MATCHMAKING_FORM);

        if (RenjuBoard.IsOnlineGame)
        {
            PlayButton.onClick.AddListener(OnPlayButtonPress);
        }
        else
        {
            EntryFormContainer.SetActive(false);
        }
    }

    void OnPlayButtonPress()
    {
        PlayerNumber playerNumber = Player1Toggle.isOn ? PlayerNumber.One : PlayerNumber.Two;
        
        StartCoroutine(FirebaseDao.JoinRoomGivenPlayerNameAndPlayerNumber(RoomName.text, PlayerName.text, playerNumber));

        EntryFormContainer.SetActive(false); //don't show form after entering user info
    }

    public static void CreateNewOnlineGame()
    {
        EntryFormContainer.SetActive(true);
    }
}
