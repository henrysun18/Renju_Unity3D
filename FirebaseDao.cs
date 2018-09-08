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
    private PlayerEntryForm PlayerEntryForm;

    private static string PATCH_PARAM = "?x-http-method-override=PATCH";
    private string GET_PARAM = "?x-http-method-override=GET";
    private static string roomsListUrl = "https://henrys-firebase-db.firebaseio.com/Renju/Rooms/";

    private bool IsUndoUnacknowledged = true;

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

            if (IsUndoButtonUnacknowledged())
            {
                RenjuBoard.OnUndoButtonPress(); //do this first so OnUndoButtonPress goes through correct control flow
                StartCoroutine(ConfirmUndoRequest()); 
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
            OnlineRoomInfo = new RoomDto //don't set IsBlacksTurn to true yet, make sure white is in first
            {
                Player1 = playerName, //wait for p2 to arrive before IsBlacksTurn is set
                UndoStates = new UndoStatesDto(),
                OpponentsLastMove = Point.At(-1, -1)
            };
        }
        else
        {
            OnlineRoomInfo = new RoomDto
            {
                Player2 = playerName,
                IsBlacksTurn = true,
                UndoStates = new UndoStatesDto(),
                OpponentsLastMove = Point.At(-1, -1)
            };
        }

        string data = JsonConvert.SerializeObject(OnlineRoomInfo);
        byte[] rawData = Encoding.ASCII.GetBytes(data);
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + PATCH_PARAM, rawData))
        {
            yield return www;
        }
    }

    public IEnumerator GetRoomInfo()
    {
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + GET_PARAM))
        {
            yield return www;
            if (www.isDone)
            {
                OnlineRoomInfo = JsonConvert.DeserializeObject<RoomDto>(www.text);
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

    public bool IsUndoButtonUnacknowledged()
    {
        if (OnlineRoomInfo == null || OnlineRoomInfo.UndoStates == null)
        {
            return false;
        }

        if (OnlinePlayerNumber == PlayerNumber.One && !OnlineRoomInfo.UndoStates.IsUndoButtonPressedByWhite ||
            OnlinePlayerNumber == PlayerNumber.Two && !OnlineRoomInfo.UndoStates.IsUndoButtonPressedByBlack)
        {
            IsUndoUnacknowledged = true;
        }

        if (OnlinePlayerNumber == PlayerNumber.One)
        {
            return OnlineRoomInfo.UndoStates.IsUndoButtonPressedByWhite && IsUndoUnacknowledged;
        }
        else
        {
            return OnlineRoomInfo.UndoStates.IsUndoButtonPressedByBlack && IsUndoUnacknowledged;
        }
    }

    public IEnumerator ConfirmUndoRequest()
    {
        IsUndoUnacknowledged = false;

        UndoStatesDto undoStatesDto = new UndoStatesDto
        {
            IsUndoButtonPressedByBlack = false,
            IsUndoButtonPressedByWhite = false
        };

        string data = JsonConvert.SerializeObject(undoStatesDto);
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
