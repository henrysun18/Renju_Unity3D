using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FirebaseDao : MonoBehaviour
{
    public static string OnlineRoomName;
    public static PlayerNumber OnlinePlayerNumber = PlayerNumber.One;
    public static RoomDto OnlineRoomInfo = new RoomDto{UndoStates = new UndoStatesDto(), OpponentsLastMove = Point.At(-1, -1)};

    private RenjuBoard RenjuBoard;

    private static string PATCH_PARAM = "?x-http-method-override=PATCH";
    private static string PUT_PARAM = "?x-http-method-override=PUT";
    private string GET_PARAM = "?x-http-method-override=GET";
    private static string roomsListUrl = "https://henrys-firebase-db.firebaseio.com/Renju/Rooms/";

    private bool IsUndoAcknowledged;
    private bool IsUndoPressed;

    void Start()
    {
        RenjuBoard = gameObject.GetComponent<RenjuBoard>(); //gets the board that this (optional) script is attached to

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore, //need to be able to update some properties of RoomDto without overwriting others
        };
        InvokeRepeating("SyncGameWithDB", 0.2f, 0.5f); //0.2s delay, repeat every 0.5s
    }

    void SyncGameWithDB()
    {
        if (!GameConfiguration.IsWaitingOnPlayerEntryForm)
        {
            StartCoroutine(GetRoomInfo());

            if (!IsUndoButtonAvailable() && !IsUndoAcknowledged) //respond to opponent's undo request
            {
                if (OnlinePlayerNumber == PlayerNumber.Two && OnlineRoomInfo.UndoStates.IsUndoButtonPressedByBlack ||
                    OnlinePlayerNumber == PlayerNumber.One && OnlineRoomInfo.UndoStates.IsUndoButtonPressedByWhite)
                {
                    RenjuBoard.UndoOneMove();
                    StartCoroutine(ConfirmUndoRequest());
                }
            }
            else if (IsOpponentDoneChoosingAMove())
            {
                RenjuBoard.AttemptToPlaceStone(GetOpponentsLastMove());
            }
        }
    }

    public static IEnumerator JoinRoomGivenPlayerNameAndPlayerNumber(string roomName, string playerName, PlayerNumber playerNumber)
    {
        OnlineRoomName = roomName;
        OnlinePlayerNumber = playerNumber;

        if (playerName.Equals(""))
        {
            playerName = "Player " + playerNumber;
        }
        if (playerNumber == PlayerNumber.One)
        {
            RoomDto roomInfo = new RoomDto
            {
                Player1 = playerName,
                IsBlacksTurn = true,
                UndoStates = new UndoStatesDto(),
                OpponentsLastMove = Point.At(-1, -1)
            };
            string data = JsonConvert.SerializeObject(roomInfo);
            byte[] rawData = Encoding.ASCII.GetBytes(data);
            using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + PUT_PARAM, rawData))
            {
                yield return www;
            }
        }
        else
        {
            byte[] rawData = Encoding.ASCII.GetBytes(playerName);
            using (WWW www = new WWW(roomsListUrl + OnlineRoomName + "/Player2.json" + PUT_PARAM, rawData))
            {
                yield return www;
            }
        }
    }

    public IEnumerator GetRoomInfo()
    {
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + GET_PARAM))
        {
            yield return www;
            if (www.isDone)
            {
                RoomDto tmpRoomDto = JsonConvert.DeserializeObject<RoomDto>(www.text);
                if (IsUndoAcknowledged)
                {
                    tmpRoomDto.UndoStates = OnlineRoomInfo.UndoStates; //dont double acknowledge
                    IsUndoAcknowledged = false; //only need this protection once
                }
                OnlineRoomInfo = tmpRoomDto;

                GameObject.Find(GameConstants.P1_LABEL_GAMEOBJECT).GetComponent<Text>().text = "Black: " + OnlineRoomInfo.Player1;
                GameObject.Find(GameConstants.P2_LABEL_GAMEOBJECT).GetComponent<Text>().text = "White: " + OnlineRoomInfo.Player2;
            }
        }
    }

    public IEnumerator SetTurnOverAfterMyMove(Point myMove)
    {
        RoomDto roomDto = new RoomDto
        {
            OpponentsLastMove = myMove,
            IsBlacksTurn = OnlinePlayerNumber != PlayerNumber.One //if we are black and just moved, then notify that it's white's turn
        };

        string data = JsonConvert.SerializeObject(roomDto);
        byte[] rawData = Encoding.ASCII.GetBytes(data);
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + PATCH_PARAM, rawData))
        {
            yield return www;
        }
    }

    public IEnumerator PressUndoButton()
    {
        IsUndoPressed = true;

        RoomDto roomDto = new RoomDto
        {
            UndoStates = new UndoStatesDto
            {
                IsUndoButtonPressedByBlack = OnlinePlayerNumber == PlayerNumber.One,
                IsUndoButtonPressedByWhite = OnlinePlayerNumber == PlayerNumber.Two
            },
            OpponentsLastMove = Point.At(-999, -999), //remove when safe?
            IsBlacksTurn = !OnlineRoomInfo.IsBlacksTurn
        };

        string data = JsonConvert.SerializeObject(roomDto);
        byte[] rawData = Encoding.ASCII.GetBytes(data);
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + PATCH_PARAM, rawData))
        {
            yield return www;
        }
    }

    public bool IsMyTurn()
    {
        return OnlineRoomInfo.IsBlacksTurn && OnlinePlayerNumber.Equals(PlayerNumber.One) && RenjuBoard.IsBlacksTurn ||
               !OnlineRoomInfo.IsBlacksTurn && OnlinePlayerNumber.Equals(PlayerNumber.Two) && !RenjuBoard.IsBlacksTurn;
    }

    public bool IsOpponentDoneChoosingAMove()
    {
        return OnlineRoomInfo.IsBlacksTurn && OnlinePlayerNumber.Equals(PlayerNumber.One) && !RenjuBoard.IsBlacksTurn || //previously white's turn
               !OnlineRoomInfo.IsBlacksTurn && OnlinePlayerNumber.Equals(PlayerNumber.Two) && RenjuBoard.IsBlacksTurn; //need to simulate other player's move first
    }

    public bool IsUndoButtonAvailable()
    {
        if (OnlineRoomInfo == null || OnlineRoomInfo.UndoStates == null)
        {
            return true;
        }

        //if sender's button press just propagated to db, don't break early
        if (OnlinePlayerNumber == PlayerNumber.One && OnlineRoomInfo.UndoStates.IsUndoButtonPressedByBlack ||
            OnlinePlayerNumber == PlayerNumber.Two && OnlineRoomInfo.UndoStates.IsUndoButtonPressedByWhite)
        {
            IsUndoPressed = false;
        }

        //if sender presses button and it still hasn't propagated to db
        if (IsUndoPressed)
        {
            return false;
        }

        //by now, only sender sees undo being performed. both sides see UndoStates.IsUndoButtonPressedByOriginator
        
        if (OnlineRoomInfo.UndoStates.IsUndoButtonPressedByBlack || OnlineRoomInfo.UndoStates.IsUndoButtonPressedByWhite)
        {
            return false; //undo button unavailable since we need to process sender's undo action first
        }

        return true;
    }

    public IEnumerator ConfirmUndoRequest()
    {
        OnlineRoomInfo.UndoStates.IsUndoButtonPressedByBlack = false;
        OnlineRoomInfo.UndoStates.IsUndoButtonPressedByWhite = false;
        IsUndoAcknowledged = true;

        //receiver tells sender to modifier IsUndoAcknowledged flag on sender side
        string data = JsonConvert.SerializeObject(OnlineRoomInfo.UndoStates);
        byte[] rawData = Encoding.ASCII.GetBytes(data);
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + "/UndoStates.json" + PATCH_PARAM, rawData))
        {
            yield return www;
        }
    }

    public Point GetOpponentsLastMove()
    {
        return OnlineRoomInfo.OpponentsLastMove;
    }


}
