using Newtonsoft.Json;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseDao : MonoBehaviour
{
    public static string OnlineRoomName;
    public static PlayerNumber OnlinePlayerNumber = PlayerNumber.One;
    public static RoomDto OnlineRoomInfo;

    private RenjuBoard RenjuBoard;

    private static string PATCH_PARAM = "?x-http-method-override=PATCH";
    private static string PUT_PARAM = "?x-http-method-override=PUT";
    private string GET_PARAM = "?x-http-method-override=GET";
    private static string roomsListUrl = "https://henrys-firebase-db.firebaseio.com/Renju/Rooms/";

    private bool IsUndoAcknowledged;
    private bool IsUndoPressed;

    void Start()
    {
        OnlineRoomInfo = new RoomDto
        {
            GameState = new GameStateDto
            {
                IsBlacksTurn = true,
                OpponentsLastMove = Point.At(-999, 999),
                UndoStates = new UndoStatesDto
                {
                    IsUndoButtonPressedByBlack = false,
                    IsUndoButtonPressedByWhite = false
                }
            },
            PlayerInfo = new PlayerInfoDto()
        };
        RenjuBoard = gameObject.GetComponent<RenjuBoard>(); //gets the board that this (optional) script is attached to

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore, //need to be able to update some properties of RoomDto without overwriting others
        };
        InvokeRepeating("SyncGameWithDB", 0.2f, 1f); //0.2s delay, repeat every 0.5s
    }

    void SyncGameWithDB()
    {
        if (!GameConfiguration.IsWaitingOnPlayerEntryForm)
        {
            StartCoroutine(GetRoomInfo());

            if (!IsUndoButtonAvailable() && !IsUndoAcknowledged) //respond to opponent's undo request
            {
                if (OnlinePlayerNumber == PlayerNumber.Two && OnlineRoomInfo.IsUndoButtonPressedByBlack() ||
                    OnlinePlayerNumber == PlayerNumber.One && OnlineRoomInfo.IsUndoButtonPressedByWhite())
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
            OnlineRoomInfo.SetPlayer1Name(playerName);
            string data = JsonConvert.SerializeObject(OnlineRoomInfo);
            byte[] rawData = Encoding.ASCII.GetBytes(data);
            using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + PUT_PARAM, rawData))
            {
                yield return www;
            }
        }
        else
        {
            string data = JsonConvert.SerializeObject(playerName);
            byte[] rawData = Encoding.ASCII.GetBytes(data);
            using (WWW www = new WWW(roomsListUrl + OnlineRoomName + "/PlayerInfo/Player2.json" + PUT_PARAM, rawData))
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
                    tmpRoomDto.GameState.UndoStates = OnlineRoomInfo.GameState.UndoStates; //dont double acknowledge
                    IsUndoAcknowledged = false; //only need this protection once
                }
                OnlineRoomInfo = tmpRoomDto;

                GameObject.Find(GameConstants.P1_LABEL_GAMEOBJECT).GetComponent<Text>().text = OnlineRoomInfo.Player1Name();
                GameObject.Find(GameConstants.P2_LABEL_GAMEOBJECT).GetComponent<Text>().text = OnlineRoomInfo.Player2Name();

                if (tmpRoomDto.GameState.UndoStates.IsUndoButtonPressedByBlack ||
                    tmpRoomDto.GameState.UndoStates.IsUndoButtonPressedByWhite || IsUndoPressed)
                {
                    GameObject.Find(GameConstants.SOLO_UNDO_BUTTON).GetComponent<Button>().enabled = false; //prevent undo button spam, reducing bugs
                }
                else
                {
                    GameObject.Find(GameConstants.SOLO_UNDO_BUTTON).GetComponent<Button>().enabled = true;
                }
            }
        }
    }

    public IEnumerator SetTurnOverAfterMyMove(Point myMove)
    {
        OnlineRoomInfo.SetOpponentsLastMove(myMove);
        OnlineRoomInfo.EndTurn(); //or set to OnlinePlayerNumber != PlayerNumber.One //if we are black and just moved, then notify that it's white's turn

        string data = JsonConvert.SerializeObject(OnlineRoomInfo);
        byte[] rawData = Encoding.ASCII.GetBytes(data);
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + PATCH_PARAM, rawData))
        {
            yield return www;
        }
    }

    public IEnumerator PressUndoButton()
    {
        IsUndoPressed = true;

        OnlineRoomInfo.SetUndoButtonPressedByBlack(OnlinePlayerNumber == PlayerNumber.One);
        OnlineRoomInfo.SetUndoButtonPressedByWhite(OnlinePlayerNumber == PlayerNumber.Two);
        OnlineRoomInfo.SetOpponentsLastMove(Point.At(-999, -999)); //remove when safe?
        OnlineRoomInfo.EndTurn();

        string data = JsonConvert.SerializeObject(OnlineRoomInfo);
        byte[] rawData = Encoding.ASCII.GetBytes(data);
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + ".json" + PATCH_PARAM, rawData))
        {
            yield return www;
        }
    }

    public bool IsMyTurn()
    {
        return OnlineRoomInfo.IsBlacksTurn() && OnlinePlayerNumber.Equals(PlayerNumber.One) && RenjuBoard.IsBlacksTurn ||
               !OnlineRoomInfo.IsBlacksTurn() && OnlinePlayerNumber.Equals(PlayerNumber.Two) && !RenjuBoard.IsBlacksTurn;
    }

    public bool IsOpponentDoneChoosingAMove()
    {
        return OnlineRoomInfo.IsBlacksTurn() && OnlinePlayerNumber.Equals(PlayerNumber.One) && !RenjuBoard.IsBlacksTurn || //previously white's turn
               !OnlineRoomInfo.IsBlacksTurn() && OnlinePlayerNumber.Equals(PlayerNumber.Two) && RenjuBoard.IsBlacksTurn; //need to simulate other player's move first
    }

    public bool IsUndoButtonAvailable()
    {
        //if sender's button press just propagated to db, don't break early
        if (OnlinePlayerNumber == PlayerNumber.One && OnlineRoomInfo.IsUndoButtonPressedByBlack() ||
            OnlinePlayerNumber == PlayerNumber.Two && OnlineRoomInfo.IsUndoButtonPressedByWhite())
        {
            IsUndoPressed = false;
        }

        //if sender presses button and it still hasn't propagated to db
        if (IsUndoPressed)
        {
            return false;
        }

        //by now, only sender sees undo being performed. both sides see UndoStates.IsUndoButtonPressedByOriginator
        
        if (OnlineRoomInfo.IsUndoButtonPressedByBlack() || OnlineRoomInfo.IsUndoButtonPressedByWhite())
        {
            return false; //undo button unavailable since we need to process sender's undo action first
        }

        return true;
    }

    public IEnumerator ConfirmUndoRequest()
    {
        OnlineRoomInfo.SetUndoButtonPressedByBlack(false);
        OnlineRoomInfo.SetUndoButtonPressedByWhite(false);
        IsUndoAcknowledged = true;

        //receiver tells sender to modifier IsUndoAcknowledged flag on sender side
        string data = JsonConvert.SerializeObject(OnlineRoomInfo.GameState.UndoStates);
        byte[] rawData = Encoding.ASCII.GetBytes(data);
        using (WWW www = new WWW(roomsListUrl + OnlineRoomName + "/GameState/UndoStates.json" + PATCH_PARAM, rawData))
        {
            yield return www;
        }
    }

    public Point GetOpponentsLastMove()
    {
        return OnlineRoomInfo.OpponentsLastMove();
    }


}
