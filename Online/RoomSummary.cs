public class RoomSummary
{
    public string P1 { get; set; }
    public string P2 { get; set; }

    public bool IsBothPlayersPresent()
    {
        return !string.IsNullOrEmpty(P1) && !string.IsNullOrEmpty(P2);
    }
}
