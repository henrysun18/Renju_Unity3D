using Assets.Scripts.Online;
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
    public GameObject UndoRequestSentModal;
    public GameObject UndoRequestReceivedModal;

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
        return RestAPIUtil.GetRequest(url, (response) =>
        {
            Debug.Log("CheckIsMyTurn url: " + url);
            if (response.Equals("true"))
            {
                StartCoroutine(GetOpponentsMostRecentMove());
            }
        });
    }

    IEnumerator GetOpponentsMostRecentMove() //only usage is in IsMyTurn()
    {
        string url = GameConfiguration.ServerUrl + "most-recent-move?room=" + OnlineRoomNumber;
        return RestAPIUtil.GetRequest(url, (response) =>
        {
            Point mostRecentMove = JsonConvert.DeserializeObject<Point>(response);
            Debug.Log("GetOpponentsMostRecentMove url: " + url + " which is " + response);
            if (mostRecentMove.Equals(GameConstants.UNDO_REQUEST))
            {
                // if opponent clicks undo, we need to open a pop-up to accept or reject. So the baton is passed to us. 
                if (!UndoRequestReceivedModal.activeInHierarchy)
                {
                    UndoRequestReceivedModal.SetActive(true);
                    RenjuBoard.EndTurn();
                }
            }
            else if (mostRecentMove.Equals(GameConstants.UNDO_REQUEST_ACCEPTED))
            {
                // if opponent accepts undo request, Close the "waiting" pop-up, then undo, before finally passing the baton back to us
                if (UndoRequestSentModal.activeInHierarchy)
                {
                    // don't accidentally undo too many times
                    UndoRequestSentModal.SetActive(false);
                    Undo();
                    RenjuBoard.EndTurn();
                }
            }
            else if (mostRecentMove.Equals(GameConstants.UNDO_REQUEST_REJECTED))
            {
                // if opponent rejects undo request, Close the "waiting" pop-up, and now it's simply our turn again
                if (UndoRequestSentModal.activeInHierarchy)
                {
                    UndoRequestSentModal.SetActive(false);
                    RenjuBoard.EndTurn();
                }
            }
            else
            {
                RenjuBoard.AttemptToPlaceStone(mostRecentMove);
            }
        });
    }

    public IEnumerator MakeMove(Point point)
    {
        Debug.Log("making move with updated renjuboard.isblacksturn " + RenjuBoard.IsBlacksTurn);
        // notify server that we made a move, so server can then in turn notify the other player to make the next move
        string url = string.Format("{0}make-move?room={1}&player-number={2}&x={3}&y={4}", GameConfiguration.ServerUrl, OnlineRoomNumber, OnlinePlayerNumber, point.X, point.Y);
        return RestAPIUtil.GetRequest(url, (response) =>
        {
            Debug.Log("MakeMove to url: " + url);
        });
    }

    private void Undo()
    {
        // need to undo opponents turn as well as our most recent turn
        RenjuBoard.UndoOneMove();
        RenjuBoard.UndoOneMove();
    }

    public void UndoRequest()
    {
        // mock move being made on the client side, so we can continue checking for opponent "moves" a.k.a. the undo response
        RenjuBoard.EndTurn();
        UndoRequestSentModal.SetActive(true);
        StartCoroutine(MakeMove(GameConstants.UNDO_REQUEST));
    }

    public void UndoRequestAccept()
    {
        RenjuBoard.EndTurn();
        UndoRequestReceivedModal.SetActive(false);
        Undo(); // removes the 2 most recent moves on the client-side, before letting server and thus the opponent know to do the same
        StartCoroutine(MakeMove(GameConstants.UNDO_REQUEST_ACCEPTED));
    }

    public void UndoRequestReject()
    {
        RenjuBoard.EndTurn();
        UndoRequestReceivedModal.SetActive(false);
        StartCoroutine(MakeMove(GameConstants.UNDO_REQUEST_REJECTED));
    }

    public static void ResetGame()
    {
        OnlinePlayerNumber = PlayerNumber.Neither;
        OnlineRoomNumber = -1;
        OnlineRoomInfo = new Room();
    }
}
