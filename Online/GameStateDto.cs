using Newtonsoft.Json;

public class GameStateDto {

    public bool IsBlacksTurn { get; set; }
    public UndoStatesDto UndoStates { get; set; }

    public Point OpponentsLastMove { get; set; }
}
