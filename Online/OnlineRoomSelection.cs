﻿using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class OnlineRoomSelection : MonoBehaviour
{
    public GameObject NameInputField;
    public GameObject RoomsGameObject;
    public Text PlayerName;
    public Text P1Label;
    public Text P2Label;

    private static List<RoomSummary> RoomSummaries;

    private static TextAsset AnimalsTextFile;
    private static string[] animals;

    void Start()
    {
        if (GameConfiguration.IsOnlineGame)
        {
            AnimalsTextFile = Resources.Load<TextAsset>("animals");
            animals = AnimalsTextFile.text.Split();

            InvokeRepeating("RefreshLobbyIfNotInGame", 0.2f, 3f); //refresh every 3s
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
        if (string.IsNullOrEmpty(OnlineMultiplayerClient.OnlineRoomInfo.P1()) || string.IsNullOrEmpty(OnlineMultiplayerClient.OnlineRoomInfo.P2()))
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

                if (OnlineMultiplayerClient.OnlinePlayerNumber == PlayerNumber.Neither)
                {
                    // user is still outside in the lobby
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
                            if (t.name == "JoinButton")
                            {
                                string buttonText = t.GetChild(0).GetComponent<Text>().text;
                                int roomNumber = Int32.Parse(buttonText.Substring(buttonText.IndexOf('#') + 1));
                                t.GetComponent<Button>().onClick.AddListener(() => OnJoinButtonPress(roomNumber));
                            }
                        }
                        currentRoomIndex++;
                    }
                }
                else
                {
                    // user already joined a room and is waiting for other player
                    string P1 = RoomSummaries[OnlineMultiplayerClient.OnlineRoomNumber].P1;
                    string P2 = RoomSummaries[OnlineMultiplayerClient.OnlineRoomNumber].P2;
                    OnlineMultiplayerClient.OnlineRoomInfo.SetP1(P1);
                    OnlineMultiplayerClient.OnlineRoomInfo.SetP2(P2);
                    P1Label.text = P1;
                    P2Label.text = P2;
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
}