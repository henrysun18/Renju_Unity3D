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

    private RenjuBoard RenjuBoard;

    public void Init(RenjuBoard renjuBoard)
    {
        this.RenjuBoard = renjuBoard;
    }

    void Start()
    {
        if (GameConfiguration.IsOnlineGame && OnlineRoomNumber >= 0)
        {
            InvokeRepeating("CheckForOpponentAction", 0.2f, 1f); //refresh every 1s
        }
    }

    void CheckForOpponentAction()
    {
        //Black will call this in the beginning to verify with server if he can start (IsMyTurn() is the only method that can modify OnlineRoomInfo...IsBlacksTurn)
        //White will call IsMyTurn as soon as both players are in the room
        if (OnlinePlayerNumber == PlayerNumber.One && OnlineRoomInfo.IsWhitesTurn()
            || OnlinePlayerNumber == PlayerNumber.Two && OnlineRoomInfo.IsBlacksTurn())
        {
            StartCoroutine(IsMyTurn());
        }
    } 

    public IEnumerator IsMyTurn()
    {
        using (WWW www = new WWW(GameConfiguration.ServerUrl + "is-my-turn?room=" + OnlineRoomNumber + "&player-number=" + OnlinePlayerNumber))
        {
            if (www.isDone)
            {
                if (www.text.Equals("true"))
                {
                    OnlineRoomInfo.GameState.IsBlacksTurn = OnlinePlayerNumber == PlayerNumber.One;
                    StartCoroutine(GetOpponentsMostRecentMove());
                }
                yield return www;
            }
        }
    }

    IEnumerator GetOpponentsMostRecentMove() //only usage is in IsMyTurn()
    {
        using (WWW www = new WWW(GameConfiguration.ServerUrl + "most-recent-move?room=" + OnlineRoomNumber))
        {
            if (www.isDone)
            {
                Point mostRecentMove = JsonConvert.DeserializeObject<Point>(www.text);
                if (mostRecentMove.X == -1 && mostRecentMove.Y == -1)
                {
                    //opponent pressed undo
                }

                RenjuBoard.AttemptToPlaceStone(mostRecentMove);
                yield return www;
            }
        }
    }

    public IEnumerator MakeMove(Point point)
    {
        OnlineRoomInfo.GameState.IsBlacksTurn = !OnlineRoomInfo.GameState.IsBlacksTurn;
        using (WWW www = new WWW(string.Format("{0}make-move?room={1}&player-number={2}&x={3}&y={4}", GameConfiguration.ServerUrl, OnlineRoomNumber, OnlinePlayerNumber, point.X, point.Y)))
        {
            if (www.isDone)
            {
                yield return www;
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
