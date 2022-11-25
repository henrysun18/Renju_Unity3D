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
using UnityEngine.Networking;
using UnityEditor.PackageManager.Requests;

public class OnlineRoomSelection : MonoBehaviour
{
    public GameObject NameInputField;
    public GameObject RoomsGameObject;
    public GameObject OnlineRoomsSelectionUI;
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
                    // assumes URL obtained from Firebase is prefixed with http:// and suffixed with :8080/
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
            // || OnlineMultiplayerClient.OnlinePlayerNumber == PlayerNumber.Spectator && string.IsNullOrEmpty(OnlineMultiplayerClient.OnlineRoomInfo.P1()))
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
        string url = GameConfiguration.ServerUrl + "refresh-lobby";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            //MAJOR KEY, otherwise anything written here assumes www is null
            //(spent hrs debugging this then realized I stumbled across this before)
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(url + ", error: " + www.error);
            }
            else
            {
                //Debug.Log(www.downloadHandler.text);
                RoomSummaries = JsonConvert.DeserializeObject<List<RoomSummary>>(www.downloadHandler.text);
                
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
                Debug.Log("refreshing room " + OnlineMultiplayerClient.OnlineRoomNumber + " " + response.ToString());

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
        Debug.Log("onJoinButtonPress" + roomNumber);
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
                Debug.Log("joined room" + roomNumber + " as player " + www.text + " name " + playerName);
                if (www.text == "1")
                {
                    OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.One;
                    P1Label.text = playerName;
                }
                else if (www.text == "2")
                {
                    OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.Two;
                    P2Label.text = playerName;
                }
                else
                {
                    // TODO: assume room is full, so we can only spectate
                    //OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.Spectator;
                    // P1Label and P2Label will be set when polling server for moves
                }

                OnlineMultiplayerClient.OnlineRoomNumber = roomNumber;
                HideRoomSelectionUI();
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
