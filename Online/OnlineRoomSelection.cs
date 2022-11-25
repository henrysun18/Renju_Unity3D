using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Firebase.Database;
using Firebase.Extensions;
public class OnlineRoomSelection : MonoBehaviour
{
    public GameObject NameInputField;
    public GameObject RoomsGameObject;
    public Text PlayerName;
    public Text P1Label;
    public Text P2Label;

    private static RenjuBoard RenjuBoard;
    private static List<RoomSummary> RoomSummaries;

    private static TextAsset AnimalsTextFile;
    private static string[] animals;

    public void Init(RenjuBoard renjuBoard)
    {
        RenjuBoard = renjuBoard;
    }

    void Start()
    {
        if (GameConfiguration.IsOnlineGame)
        {
            // grab server URL from firebase in case Compute Engine instance's IP changes
            FirebaseDatabase.DefaultInstance.GetReference("renju3d-server-ip").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    GameConfiguration.ServerUrl = task.Result.Value.ToString();
                }
                else
                {
                    GameConfiguration.ServerUrl = "http://localhost:8080/";
                }
                Debug.Log("server URL:" + GameConfiguration.ServerUrl);
            });

            // default names if not provided by user
            AnimalsTextFile = Resources.Load<TextAsset>("animals");
            animals = AnimalsTextFile.text.Split();

            InvokeRepeating("RefreshLobbyIfNotInGame", 0.2f, 3f); //refresh every 3s
            InvokeRepeating("RefreshRoomIfWaitingForOpponent", 0.2f, 1f);
            InvokeRepeating("KeepConnectionToServerAlive", 0.2f, 5f); //evict player if this is not called after a while

            foreach (Transform roomTransform in RoomsGameObject.transform)
            {
                foreach (Transform t in roomTransform)
                { 
                    if (t.name == "JoinButton")
                    {
                        string buttonText = t.GetChild(0).GetComponent<Text>().text;
                        int roomNumber = Int32.Parse(buttonText.Substring(buttonText.IndexOf('#') + 1));
                        t.GetComponent<Button>().onClick.AddListener(() => OnJoinButtonPress(roomNumber));
                    }
                }
            }
        }
        else
        {
            HideRoomSelectionUI();
        }
        /*JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore, //need to be able to update some properties of Room without overwriting others
        };*/
    }

    void RefreshLobbyIfNotInGame()
    {
        if (OnlineMultiplayerClient.OnlinePlayerNumber == PlayerNumber.Neither)
        {
            StartCoroutine(RefreshLobby());
        }
    }

    void RefreshRoomIfWaitingForOpponent()
    {
        if (OnlineMultiplayerClient.OnlinePlayerNumber == PlayerNumber.One && string.IsNullOrEmpty(OnlineMultiplayerClient.OnlineRoomInfo.P2()) ||
            OnlineMultiplayerClient.OnlinePlayerNumber == PlayerNumber.Two && string.IsNullOrEmpty(OnlineMultiplayerClient.OnlineRoomInfo.P1()))
        {
            StartCoroutine(RefreshRoom());
        }
    }

    void KeepConnectionToServerAlive()
    {
        if (OnlineMultiplayerClient.OnlinePlayerNumber != PlayerNumber.Neither)
        {
            StartCoroutine(KeepAlive());
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
                        if (t.name == "BlackPlayerName")
                        {
                            t.GetComponent<Text>().text = RoomSummaries[currentRoomIndex].P1;
                        }
                        if (t.name == "WhitePlayerName")
                        {
                            t.GetComponent<Text>().text = RoomSummaries[currentRoomIndex].P2;
                        }
                    }
                    currentRoomIndex++;
                }
            }
        }
    }

    IEnumerator RefreshRoom()
    {
        using (WWW www = new WWW(GameConfiguration.ServerUrl + "refresh-room?room=" + OnlineMultiplayerClient.OnlineRoomNumber))
        {
            yield return www;
            if (www.isDone)
            {
                RoomSummary response = JsonConvert.DeserializeObject<RoomSummary>(www.text);

                OnlineMultiplayerClient.OnlineRoomInfo.SetP1(response.P1);
                OnlineMultiplayerClient.OnlineRoomInfo.SetP2(response.P2);
                P1Label.text = response.P1;
                P2Label.text = response.P2;
            }
        }
    }

    IEnumerator KeepAlive()
    {
        using (WWW www = new WWW(GameConfiguration.ServerUrl + "keep-alive?room=" + OnlineMultiplayerClient.OnlineRoomNumber + "&player-number=" + OnlineMultiplayerClient.OnlinePlayerNumber))
        {
            yield return www;
            if (www.isDone)
            {
                // Check if server gave us a -1 error code, meaning opponent disconnected / forfeit / ragequit
                if (www.text == "-1")
                {                    
                    RenjuBoard.SetWinner(OnlineMultiplayerClient.OnlinePlayerNumber);
                    RenjuBoard.WinMessage.GetComponent<TextMesh>().text = "The opponent has left. \nYou win!";
                    OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.Neither; //prevent KeepAlive being called again
                }
            }
        }
    }

    private void OnJoinButtonPress(int roomNumber)
    {
        StartCoroutine(JoinRoomGivenRoomNumberAndPlayerName(roomNumber, PlayerName.text));
    }

    public IEnumerator JoinRoomGivenRoomNumberAndPlayerName(int roomNumber, string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = animals[(int)(Random.value * animals.Length)];
        }
        using (WWW www = new WWW(GameConfiguration.ServerUrl + "join?room=" + roomNumber + "&name=" + playerName))
        {
            yield return www;
            if (www.isDone)
            {
                if (www.text == "1")
                {
                    OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.One;
                    OnlineMultiplayerClient.OnlineRoomNumber = roomNumber;
                    P1Label.text = playerName;
                    HideRoomSelectionUI();
                }
                else if (www.text == "2")
                {
                    OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.Two;
                    OnlineMultiplayerClient.OnlineRoomNumber = roomNumber;
                    P2Label.text = playerName;
                    HideRoomSelectionUI();
                }
                else
                {
                    // ask user to retry, since someone else may have taken their place?
                }
            }
        }
    }

    private void HideRoomSelectionUI()
    {
        NameInputField.SetActive(false);
        RoomsGameObject.SetActive(false);
    }

    public void ExitBackToLobby()
    {
        NameInputField.SetActive(true);
        RoomsGameObject.SetActive(true);
        PlayerName.text = "";
        P1Label.text = "";
        P2Label.text = "";
    }
}
