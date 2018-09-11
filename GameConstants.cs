using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConstants {
    public const string BLACK_STONE = "BlackStone";
    public const string WHITE_STONE = "WhiteStone";
    public const string P1_LABEL_GAMEOBJECT = "P1Label";
    public const string P2_LABEL_GAMEOBJECT = "P2Label";
    public const string UNDO_BUTTON_BLACK = "UndoButtonBlack";
    public const string UNDO_BUTTON_WHITE = "UndoButtonWhite";
    public const string BLACK_WIN_MESSAGE = "BlackWinMessage";
    public const string WHITE_WIN_MESSAGE = "WhiteWinMessage";
    public const string ONLINE_MATCHMAKING_FORM = "OnlineMatchmakingForm";

    public static Quaternion QuaternionTowardsBlack = Quaternion.Euler(90, 0, 90);
    public static Quaternion QuaternionTowardsWhite = Quaternion.Euler(90, 0, -90);
}
