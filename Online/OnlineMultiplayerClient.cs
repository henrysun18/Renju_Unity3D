using Newtonsoft.Json;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class OnlineMultiplayerClient : MonoBehaviour
{
    public static PlayerNumber OnlinePlayerNumber = PlayerNumber.Neither;
    public static int OnlineRoomNumber = -1;
    public static Room OnlineRoomInfo = new Room();


    public IEnumerator IsMyTurn(int roomNumber, PlayerNumber playerNumber)
    {
        using (WWW www = new WWW(GameConfiguration.ServerUrl + "is-my-turn?room=" + roomNumber + "&player-number=" + playerNumber))
        {
            if (www.text.Equals("true"))
            {
                GetOpponentsMostRecentMove(roomNumber);
            }
            yield return www;
        }
    }

    public IEnumerator GetOpponentsMostRecentMove(int roomNumber)
    {
        using (WWW www = new WWW(GameConfiguration.ServerUrl + "most-recent-move?room=" + roomNumber))
        {
            Point mostRecentMove = JsonConvert.DeserializeObject<Point>(www.text);
            OnlineRoomInfo.GameState.OpponentsLastMove = mostRecentMove;
            yield return www;
        }
    }

    public Point GetOpponentsMostRecentMove()
    {
        return OnlineRoomInfo.OpponentsLastMove();
    }
}
