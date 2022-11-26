public class Room {
    public GameState GameState { get; set; }
    public RoomSummary RoomSummary { get; set; }

    public Room()
    {
        GameState = new GameState();
        RoomSummary = new RoomSummary();
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

    public string P1()
    {
        return RoomSummary.P1;
    }

    public string P2()
    {
        return RoomSummary.P2;
    }

    public void SetP1(string name)
    {
        RoomSummary.P1 = name;
    }

    public void SetP2(string name)
    {
        RoomSummary.P2 = name;
    }

    public bool IsBothPlayersPresent()
    {
        return !string.IsNullOrEmpty(P1()) && !string.IsNullOrEmpty(P2());
    }
}
