using Newtonsoft.Json;

public class GameState {

    public bool IsBlacksTurn { get; set; }
    public UndoStatesDto UndoStates { get; set; }

    public Point OpponentsLastMove { get; set; }
}
