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
    public static string OnlineRoomName = "23333";
    public static PlayerNumber OnlinePlayerNumber = PlayerNumber.One;
    public static RoomDto OnlineRoomInfo;

    private static string PATCH_PARAM = "?x-http-method-override=PATCH";
    private static string POST_PARAM = "?x-http-method-override=POST";
    private static string PUT_PARAM = "?x-http-method-override=PUT";
    private static string GET_PARAM = "?x-http-method-override=GET";
    private static string roomsListUrl = "https://henrys-firebase-db.firebaseio.com/Renju/Rooms/";
    private static string baseUrlJson = "https://henrys-firebase-db.firebaseio.com/Renju.json";
    private static string roomsUrlJson = "https://henrys-firebase-db.firebaseio.com/Renju/Rooms.json";


    public static IEnumerator JoinRoomGivenPlayerNameAndPlayerNumber(string roomName, string playerName, PlayerNumber playerNumber)
    {
        OnlineRoomName = roomName;
        OnlinePlayerNumber = playerNumber;
        OnlineRoomInfo = playerNumber == PlayerNumber.One
            ? new RoomDto {Player1 = playerName, Player2 = "placeholder", IsBlacksTurn = true, LastMoveByWhite = Point.At(999, 999), LastMoveByBlack = Point.At(999, 999)}
            : new RoomDto {Player2 = playerName, IsBlacksTurn = true, LastMoveByWhite = Point.At(999, 999), LastMoveByBlack = Point.At(999, 999)};

        string data = JsonConvert.SerializeObject(OnlineRoomInfo);
        byte[] rawData = Encoding.ASCII.GetBytes(data);

        using (WWW www = new WWW(roomsListUrl + roomName + ".json" + PATCH_PARAM, rawData))
        {
            yield return www;
        }
    }

    public static IEnumerator GetRoomInfo()
    {
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + GET_PARAM))
        {
            yield return www;
            OnlineRoomInfo = JsonConvert.DeserializeObject<RoomDto>(www.text);

            Text P1Label = GameObject.FindGameObjectWithTag("P1Label").GetComponent<Text>();
            P1Label.text = "P1: " + OnlineRoomInfo.Player1;

            Text P2Label = GameObject.FindGameObjectWithTag("P2Label").GetComponent<Text>();
            P2Label.text = "P2: " + OnlineRoomInfo.Player2;
        }

        
    }

    public static IEnumerator SetTurnOverAfterMyMove(Point myMove)
    {
        RoomDto roomDto;
        if (OnlinePlayerNumber == PlayerNumber.One)
        {
            roomDto = new RoomDto
            {
                LastMoveByBlack = myMove,
                IsBlacksTurn = false,
                IsWhitesTurn = true
            };
        }
        else
        {
            roomDto = new RoomDto
            {
                LastMoveByWhite = myMove,
                IsBlacksTurn = true,
                IsWhitesTurn = false
            };
        }
        string data = JsonConvert.SerializeObject(roomDto);
        byte[] rawData = Encoding.ASCII.GetBytes(data);

        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + PATCH_PARAM, rawData))
        {
            yield return www;
        }
    }

    public static bool IsMyTurn()
    {
        return OnlineRoomInfo.IsBlacksTurn && OnlinePlayerNumber.Equals(PlayerNumber.One) ||
               OnlineRoomInfo.IsWhitesTurn && OnlinePlayerNumber.Equals(PlayerNumber.Two);
    }

    public static Point GetOpponentsLastMove()
    {
        return OnlineRoomInfo.IsBlacksTurn ? OnlineRoomInfo.LastMoveByWhite : OnlineRoomInfo.LastMoveByBlack;
    }
}
