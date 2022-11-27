using System;
using System.Collections;
using System.Collections.Generic;
//using Firebase.Database;
//using Firebase.Extensions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.Networking;
using Assets.Scripts.Online;
using TMPro;

public class OnlineRoomSelection : MonoBehaviour
{
    public GameObject NameInputField;
    public GameObject RoomsGameObject;
    public GameObject OnlineRoomsSelectionUI;
    public Text PlayerName;
    public TMP_Text P1Label;
    public TMP_Text P2Label;
    public GameObject LeaveButton;
    public AudioSource GameStartedSound; // play this when both players are ready

    private static RenjuBoard RenjuBoard;
    private static List<RoomSummary> RoomSummaries;

    private static TextAsset AnimalsTextFile;
    private static string[] animals;

    public void Init(RenjuBoard renjuBoard)
    {
        RenjuBoard = renjuBoard;
    }

    private IEnumerator GetAndCacheServerURL() 
    {
        string url = "https://henrys-firebase-db.firebaseio.com/renju3d-server-ip.json";
        return RestAPIUtil.GetRequest(url, (response) =>
        {
            string serverUrl = JsonConvert.DeserializeObject<string>(response);
            Debug.Log("rest api get : " + response + " serverUrl: " + serverUrl);
            GameConfiguration.ServerUrl = serverUrl;
        });

        // old native approach that doesn't work on WebGL build
        /*FirebaseDatabase.DefaultInstance.GetReference("renju3d-server-ip").GetValueAsync().ContinueWithOnMainThread(task =>
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
        });*/
    }

    void Start()
    {
        if (GameConfiguration.IsOnlineGame)
        {
            // grab server URL from firebase in case Compute Engine instance's IP changes
            StartCoroutine(GetAndCacheServerURL());

            // default names if not provided by user
            AnimalsTextFile = Resources.Load<TextAsset>("animals");
            animals = AnimalsTextFile.text.Split();

            InvokeRepeating("RefreshLobbyIfNotInGame", 0.2f, 3f); //refresh every 3s
            InvokeRepeating("RefreshRoomIfWaitingForOpponent", 0.2f, 1f);
            InvokeRepeating("KeepConnectionToServerAlive", 0.2f, 1f); //evict player if this is not called after a while

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
        return RestAPIUtil.GetRequest(url, (response) =>
        {
            RoomSummaries = JsonConvert.DeserializeObject<List<RoomSummary>>(response);

            int currentRoomIndex = 0;
            foreach (Transform roomTransform in RoomsGameObject.transform)
            {
                foreach (Transform t in roomTransform)
                {
                    if (t.name == "JoinButton")
                    {
                        // TODO: let's disable the Join Room button when room is full, since spectate mode doesn't work yet
                        if (RoomSummaries[currentRoomIndex].IsBothPlayersPresent())
                        {
                            t.GetComponent<Button>().interactable = false;
                        } else
                        {
                            t.GetComponent<Button>().interactable = true;
                        }
                    }
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
        });
    }

    IEnumerator RefreshRoom()
    {
        string url = GameConfiguration.ServerUrl + "refresh-room?room=" + OnlineMultiplayerClient.OnlineRoomNumber;
        return RestAPIUtil.GetRequest(url, (response) =>
        {
            RoomSummary summary = JsonConvert.DeserializeObject<RoomSummary>(response);
            Debug.Log("refreshing room " + OnlineMultiplayerClient.OnlineRoomNumber + " " + summary.ToString());

            OnlineMultiplayerClient.OnlineRoomInfo.SetP1(summary.P1);
            OnlineMultiplayerClient.OnlineRoomInfo.SetP2(summary.P2);
            P1Label.text = summary.P1;
            P2Label.text = summary.P2;

            if (OnlineMultiplayerClient.OnlineRoomInfo.IsBothPlayersPresent())
            {
                Debug.Log("both players are now present. playing sound");
                GameStartedSound.Play();
            }

            // rotate P1Label and P2Label towards black when on Android
            if (GameConfiguration.IsAndroidGame)
            {
                P1Label.transform.rotation = GameConstants.QuaternionTowardsBlack;
                P2Label.transform.rotation = GameConstants.QuaternionTowardsBlack;
                /*if (OnlineMultiplayerClient.OnlinePlayerNumber == PlayerNumber.One)
                {
                    P1Label.transform.rotation = GameConstants.QuaternionTowardsBlack;
                    P2Label.transform.rotation = GameConstants.QuaternionTowardsBlack;
                }
                else if (OnlineMultiplayerClient.OnlinePlayerNumber == PlayerNumber.Two)
                {
                    P1Label.transform.rotation = GameConstants.QuaternionTowardsWhite;
                    P2Label.transform.rotation = GameConstants.QuaternionTowardsWhite;
                } else
                {
                    P1Label.transform.rotation = GameConstants.QuaternionTowardsBlack;
                    P2Label.transform.rotation = GameConstants.QuaternionTowardsWhite;
                }*/
            }
        });
    }

    IEnumerator KeepAlive()
    {
        string url = GameConfiguration.ServerUrl + "keep-alive?room=" + OnlineMultiplayerClient.OnlineRoomNumber + "&player-number=" + OnlineMultiplayerClient.OnlinePlayerNumber;
        return RestAPIUtil.GetRequest(url, (response) =>
        {
            // Check if server gave us a -1 error code, meaning opponent disconnected / forfeit / ragequit
            if (response == "-1")
            {
                RenjuBoard.SetWinner(OnlineMultiplayerClient.OnlinePlayerNumber);
                RenjuBoard.WinMessage.GetComponent<TextMesh>().text = "The opponent has left. \nYou win!";
                OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.Neither; //prevent KeepAlive being called again
            }
            else if (response == "1")
            {
                Debug.Log("keepAlive was acked by server");
            }
        });
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
        string url = GameConfiguration.ServerUrl + "join?room=" + roomNumber + "&name=" + playerName;
        return RestAPIUtil.GetRequest(url, (response) => {
            Debug.Log("joined room" + roomNumber + " as player " + response + " name " + playerName);
            if (response == "1")
            {
                OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.One;
                P1Label.text = playerName;
            }
            else if (response == "2")
            {
                OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.Two;
                P2Label.text = playerName;
            }
            else
            {
                // TODO: assume room is full, so we can only spectate. This is a fallback in case button disabling didn't work
                //OnlineMultiplayerClient.OnlinePlayerNumber = PlayerNumber.Spectator;
                // P1Label and P2Label will be set when polling server for moves
                Debug.Log("Room " + roomNumber + " is full! Please join another room. Though this codepath should never have been hit in the first place");
                return;
            }

            OnlineMultiplayerClient.OnlineRoomNumber = roomNumber;
            HideRoomSelectionUI();
        });
    }

    private void HideRoomSelectionUI()
    {
        NameInputField.SetActive(false);
        RoomsGameObject.SetActive(false);
        LeaveButton.SetActive(true); // show the Leave room button
    }

    public void ExitBackToLobby()
    {
        NameInputField.SetActive(true);
        RoomsGameObject.SetActive(true);
        PlayerName.text = "";
        P1Label.text = "";
        P2Label.text = "";
        LeaveButton.SetActive(false); // hide the Leave room button
    }
}
