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
    public static RoomDto OnlineRoomInfo = new RoomDto();

    private static string PATCH_PARAM = "?x-http-method-override=PATCH";
    private static string POST_PARAM = "?x-http-method-override=POST";
    private static string PUT_PARAM = "?x-http-method-override=PUT";
    private static string GET_PARAM = "?x-http-method-override=GET";
    private static string roomsListUrl = "https://henrys-firebase-db.firebaseio.com/Renju/Rooms/";
    private static string baseUrlJson = "https://henrys-firebase-db.firebaseio.com/Renju.json";
    private static string roomsUrlJson = "https://henrys-firebase-db.firebaseio.com/Renju/Rooms.json";

    private static bool IsUndoAcknowledged = true;

    public static IEnumerator JoinRoomGivenPlayerNameAndPlayerNumber(string roomName, string playerName, PlayerNumber playerNumber)
    {
        OnlineRoomName = roomName;
        OnlinePlayerNumber = playerNumber;
        if (playerNumber == PlayerNumber.One)
        {
            OnlineRoomInfo = new RoomDto
            {
                Player1 = playerName, //wait for p2 to arrive before IsBlacksTurn is set
                IsBlacksTurn = true,
                UndoStates = new UndoStatesDto()
            };
        }
        else
        {
            OnlineRoomInfo = new RoomDto
            {
                Player2 = playerName,
                IsBlacksTurn = true
            };
        }

        string data = JsonConvert.SerializeObject(OnlineRoomInfo);
        byte[] rawData = Encoding.ASCII.GetBytes(data);
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + PATCH_PARAM, rawData))
        {
            yield return www;
        }
    }

    public static IEnumerator GetRoomInfo()
    {
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + GET_PARAM))
        {
            yield return www;
            if (www.isDone)
            {
                OnlineRoomInfo = JsonConvert.DeserializeObject<RoomDto>(www.text);
            }
        }
    }

    public static IEnumerator SetTurnOverAfterMyMove(Point myMove)
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

    public static IEnumerator PressUndoButton()
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

    public static bool IsMyTurn()
    {
        return OnlineRoomInfo.IsBlacksTurn && OnlinePlayerNumber.Equals(PlayerNumber.One) && RenjuBoard.IsBlacksTurn ||
               !OnlineRoomInfo.IsBlacksTurn && OnlinePlayerNumber.Equals(PlayerNumber.Two) && !RenjuBoard.IsBlacksTurn;
    }

    public static bool IsOpponentDoneChoosingAMove()
    {
        return OnlineRoomInfo.IsBlacksTurn && OnlinePlayerNumber.Equals(PlayerNumber.One) && !RenjuBoard.IsBlacksTurn || //previously white's turn
               !OnlineRoomInfo.IsBlacksTurn && OnlinePlayerNumber.Equals(PlayerNumber.Two) && RenjuBoard.IsBlacksTurn; //need to simulate other player's move first
    }

    public static bool IsUndoButtonUnacknowledged()
    {
        if (OnlinePlayerNumber == PlayerNumber.One && !OnlineRoomInfo.UndoStates.IsUndoButtonPressedByWhite ||
            OnlinePlayerNumber == PlayerNumber.Two && !OnlineRoomInfo.UndoStates.IsUndoButtonPressedByBlack)
        {
            IsUndoAcknowledged = false;
        }

        if (OnlinePlayerNumber == PlayerNumber.One)
        {
            return OnlineRoomInfo.UndoStates.IsUndoButtonPressedByWhite && !IsUndoAcknowledged;
        }
        else
        {
            return OnlineRoomInfo.UndoStates.IsUndoButtonPressedByBlack && !IsUndoAcknowledged;
        }
    }

    public static IEnumerator ConfirmUndoRequest()
    {
        IsUndoAcknowledged = true;

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

    public static Point GetOpponentsLastMove()
    {
        return OnlineRoomInfo.OpponentsLastMove;
    }


}
