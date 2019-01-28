using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class OnlineRoomSelection : MonoBehaviour
{
    public GameObject RoomsGameObject;

    private static List<RoomSummary> RoomSummaries;

    void Start()
    {
        /*JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore, //need to be able to update some properties of Room without overwriting others
        };*/
        InvokeRepeating("RefreshLobbyIfNotInGame", 0.2f, 3f); //refresh every 3s
    }

    void RefreshLobbyIfNotInGame()
    {
        if (OnlineMultiplayerClient.OnlineRoomInfo == null)
        {
            StartCoroutine(RefreshLobby());
        }
    }

    IEnumerator RefreshLobby()
    {
        using (WWW www = new WWW(GameConfiguration.ServerUrl + "refresh-lobby"))
        {
            yield return www;
            //MAJOR KEY, otherwise anything written here assumes www is null
            //(spent hrs debugging this then realized I stumbled across this before)
            if (www.isDone) 
            {
                RoomSummaries = JsonConvert.DeserializeObject<List<RoomSummary>>(www.text);
                int currentRoomIndex = 0;
                foreach (Transform roomTransform in RoomsGameObject.transform)
                {
                    foreach (Transform t in roomTransform)
                    {
                        if (t.name.Equals("BlackPlayerName"))
                        {
                            t.GetComponent<Text>().text = RoomSummaries[currentRoomIndex].P1;
                        }
                        if (t.name.Equals("WhitePlayerName"))
                        {
                            t.GetComponent<Text>().text = RoomSummaries[currentRoomIndex].P2;
                        }
                        if (t.name.Equals("JoinButton"))
                        {
                            //((Button)t).onClick.AddListener(OnJoinRoom);
                        }
                    }
                    currentRoomIndex++;
                }
            }
        }
    }

    public static IEnumerator JoinRoomGivenRoomNumberAndPlayerName(int roomNumber, string playerName)
    {
        using (WWW www = new WWW(GameConfiguration.ServerUrl + "join"))
        {
            if (www.text == "1")
            {
                OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.One;
                //OnlineMultiplayerClient.OnlineRoomInfo.SetPlayer1Name();
            }
            else if (www.text == "2")
            {
                OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.Two;
            }
            else
            {
                // ask user to retry, since someone else may have taken their place?
            }
            yield return www;
        }
    }
}
