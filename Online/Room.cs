public class Room {
    public GameState GameState { get; set; }
    public RoomSummary RoomSummary { get; set; }

    public bool IsBlacksTurn()
    {
        return GameState != null && GameState.IsBlacksTurn;
    }

    public void EndTurn()
    {
        GameState.IsBlacksTurn = !GameState.IsBlacksTurn;
    }

    public bool IsUndoButtonPressedByBlack()
    {
        if (GameState == null || GameState.UndoStates == null)
        {
            return false;
        }
        return GameState.UndoStates.IsUndoButtonPressedByBlack;
    }

    public bool IsUndoButtonPressedByWhite()
    {
        if (GameState == null || GameState.UndoStates == null)
        {
            return false;
        }
        return GameState.UndoStates.IsUndoButtonPressedByWhite;
    }

    public void SetUndoButtonPressedByBlack(bool IsUndoButtonPressed)
    {
        GameState.UndoStates.IsUndoButtonPressedByBlack = IsUndoButtonPressed;
    }

    public void SetUndoButtonPressedByWhite(bool IsUndoButtonPressed)
    {
        GameState.UndoStates.IsUndoButtonPressedByWhite = IsUndoButtonPressed;
    }

    public Point OpponentsLastMove()
    {
        if (GameState == null)
        {
            return Point.At(-1, -1);
        }
        return GameState.OpponentsLastMove;
    }

    public void SetOpponentsLastMove(Point move)
    {
        GameState.OpponentsLastMove = move;
    }

    public string Player1Name()
    {
        return RoomSummary.P1;
    }

    public string Player2Name()
    {
        return RoomSummary.P2;
    }

    public void SetPlayer1Name(string name)
    {
        RoomSummary.P1 = name;
    }

    public void SetPlayer2Name(string name)
    {
        RoomSummary.P2 = name;
    }
}
