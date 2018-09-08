using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfiguration {
    public static bool IsDebugModeWithOnlyBlackPieces = false;
    public static bool IsOnlineGame = true;
    public static bool IsWaitingOnPlayerEntryForm = IsOnlineGame;
    public static readonly int BOARD_SIZE = 15;
}
