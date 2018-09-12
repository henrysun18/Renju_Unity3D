using UnityEngine;

public class GameConfiguration {
    public static bool IsDebugModeWithSamePieceUntilUndo = false;
    public static bool IsOnlineGame = false;
    public static bool IsWaitingOnPlayerEntryForm = IsOnlineGame;
    public static bool IsAndroidGame = true;
    public static readonly int BOARD_SIZE = 15;

    public static Camera OrientCameraBasedOnPlatform()
    {
        if (IsAndroidGame)
        {
            GameObject.Find(GameConstants.CAMERA_PC).SetActive(false);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        else
        {
            GameObject.Find(GameConstants.CAMERA_ANDROID).SetActive(false);
        }

        GameObject.Find(GameConstants.CANVAS).GetComponent<Canvas>().worldCamera = Camera.main;

        return Camera.main;
    }
}
