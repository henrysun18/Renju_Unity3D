using Newtonsoft.Json;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class OnlineMultiplayerClient : MonoBehaviour
{
    public static int OnlinePlayerNumber = PlayerNumber.Neither;
    public static int OnlineRoomNumber = -1;
    public static Room OnlineRoomInfo = new Room();

    private static RenjuBoard RenjuBoard;

    public void Init(RenjuBoard renjuBoard)
    {
        RenjuBoard = renjuBoard;
    }

    void Start()
    {
        Debug.Log("start() of OnlineMultiplayerClient, invokeRepeating checkForOpponentAction (if needed)");
        InvokeRepeating("CheckForOpponentAction", 0.2f, 1f); //refresh every 1s
    }

    // called every time a user tries to click on a point on the board during online play. If true, user is allowed to put a piece down
    public bool IsMyTurn()
    {
        bool isMyTurnAsBlack = OnlinePlayerNumber == PlayerNumber.One && RenjuBoard.IsBlacksTurn && !string.IsNullOrEmpty(OnlineRoomInfo.RoomSummary.P2);
        bool isMyTurnAsWhite = OnlinePlayerNumber == PlayerNumber.Two && !RenjuBoard.IsBlacksTurn;
        Debug.Log("checking if it's my turn. playernumber / isMyTurnAsBlack / isMyTurnAsWhite: " + OnlinePlayerNumber + " " + isMyTurnAsBlack + " " + isMyTurnAsWhite);
        return isMyTurnAsBlack || isMyTurnAsWhite;
    }

    void CheckForOpponentAction()
    {
        //Black will call this in the beginning to verify with server if he can start
        //White will call this as soon as both players are in the room
        Debug.Log("CheckForOpponentAction as player " + OnlinePlayerNumber + " isBlacksTurn: " + RenjuBoard.IsBlacksTurn);
        if (GameConfiguration.IsOnlineGame && OnlineRoomNumber >= 0)
        {
            if (OnlinePlayerNumber == PlayerNumber.One && !RenjuBoard.IsBlacksTurn
            || OnlinePlayerNumber == PlayerNumber.Two && RenjuBoard.IsBlacksTurn)
            {
                StartCoroutine(CheckIsMyTurn());
            }
        }
    } 

    public IEnumerator CheckIsMyTurn()
    {
        string url = GameConfiguration.ServerUrl + "is-my-turn?room=" + OnlineRoomNumber + "&player-number=" + OnlinePlayerNumber;
        using (WWW www = new WWW(url))
        {
            yield return www;
            if (www.isDone)
            {
                Debug.Log("CheckIsMyTurn url: " + url);
                if (www.text.Equals("true"))
                {
                    StartCoroutine(GetOpponentsMostRecentMove());
                }
            }
        }
    }

    IEnumerator GetOpponentsMostRecentMove() //only usage is in IsMyTurn()
    {
        string url = GameConfiguration.ServerUrl + "most-recent-move?room=" + OnlineRoomNumber;
        using (WWW www = new WWW(url))
        {
            yield return www;
            if (www.isDone)
            {
                Debug.Log("GetOpponentsMostRecentMove url: " + url);
                Point mostRecentMove = JsonConvert.DeserializeObject<Point>(www.text);
                if (mostRecentMove.X == -1 && mostRecentMove.Y == -1)
                {
                    //opponent pressed undo
                }

                RenjuBoard.AttemptToPlaceStone(mostRecentMove);
            }
        }
    }

    public IEnumerator MakeMove(Point point)
    {
        Debug.Log("making move with updated renjuboard.isblacksturn " + RenjuBoard.IsBlacksTurn);
        // notify server that we made a move, so server can then in turn notify the other player to make the next move
        string url = string.Format("{0}make-move?room={1}&player-number={2}&x={3}&y={4}", GameConfiguration.ServerUrl, OnlineRoomNumber, OnlinePlayerNumber, point.X, point.Y);
        using (WWW www = new WWW(url))
        {
            yield return www;
            if (www.isDone)
            {
                Debug.Log("MakeMove to url: " + url);
            }
        }
    }

    public IEnumerator Undo(int roomNumber)
    {
        //OnlineRoomInfo.EndTurn();
        using (WWW www = new WWW(GameConfiguration.ServerUrl + "undo?room=" + roomNumber))
        {
            // make sure players can't just spam this button to create desync
            // since undo button can be pressed even if it's not the player's turn (?)
            // or we can make it so that the other party has to agree to undo
            yield return www;
        }
    }

    public static void ResetGame()
    {
        OnlinePlayerNumber = PlayerNumber.Neither;
        OnlineRoomNumber = -1;
        OnlineRoomInfo = new Room();
    }
}
