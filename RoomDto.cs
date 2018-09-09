public class RoomDto {
    public GameStateDto GameState { get; set; }
    public PlayerInfoDto PlayerInfo { get; set; }

    public bool IsBlacksTurn()
    {
        return GameState.IsBlacksTurn;
    }

    public void EndTurn()
    {
        GameState.IsBlacksTurn = !GameState.IsBlacksTurn;
    }

    public bool IsUndoButtonPressedByBlack()
    {
        return GameState.UndoStates.IsUndoButtonPressedByBlack;
    }

    public bool IsUndoButtonPressedByWhite()
    {
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
        return GameState.OpponentsLastMove;
    }

    public void SetOpponentsLastMove(Point move)
    {
        GameState.OpponentsLastMove = move;
    }

    public string Player1Name()
    {
        return PlayerInfo.Player1;
    }

    public string Player2Name()
    {
        return PlayerInfo.Player2;
    }

    public void SetPlayer1Name(string name)
    {
        PlayerInfo.Player1 = name;
    }

    public void SetPlayer2Name(string name)
    {
        PlayerInfo.Player2 = name;
    }
}
