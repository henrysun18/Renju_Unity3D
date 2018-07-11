using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntryForm : MonoBehaviour
{
    public GameObject EntryFormContainer;
    public Text RoomName;
    public Text PlayerName;

    public Toggle Player1Toggle;

    public Button PlayButton;

    void Start()
    {
        PlayButton.onClick.AddListener(OnPlayButtonPress);
    }

    void OnPlayButtonPress()
    {
        PlayerNumber playerNumber = Player1Toggle.isOn ? PlayerNumber.One : PlayerNumber.Two;
        
        StartCoroutine(FirebaseDao.JoinRoomGivenPlayerNameAndPlayerNumber(RoomName.text, PlayerName.text, playerNumber));

        Destroy(EntryFormContainer); //don't show form after entering user info
    }
}
