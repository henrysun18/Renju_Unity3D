using UnityEngine;

public class GameConfiguration {
    //for testing illegal move calculator
    public static bool IsDebugModeWithSamePieceUntilUndo = false;

    //Sets camera to be in portrait mode and prevent screen timeout
    public static bool IsAndroidGame = false;

    //determines whether to show OnlineRoomSelection, and restrict player to only one colour
    public static bool IsOnlineGame;
    public static string ServerUrl;

    public static readonly int BOARD_SIZE = 15;

    public static Camera OrientCameraBasedOnPlatform()
    {
        if (IsAndroidGame)
        {
            GameObject.Find(GameConstants.CAMERA_PC).SetActive(false);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            AdjustMainCameraToFitBoardOnAnyAspectRatio();
        }
        else
        {
            GameObject.Find(GameConstants.CAMERA_ANDROID).SetActive(false);
        }

        GameObject.Find(GameConstants.CANVAS).GetComponent<Canvas>().worldCamera = Camera.main;

        return Camera.main;
    }

    private static void AdjustMainCameraToFitBoardOnAnyAspectRatio()
    {
        Camera mainCamera = Camera.main;

        float vFovDegOld = mainCamera.fieldOfView; //should be set to 60
        float hFovDeg = 36; //want the device to be wide enough to see entire board, goal is at least 36 horizontal fov
        float deviceAspectRatio = mainCamera.aspect;
        float vFovDegNew = 2.0f * Mathf.Atan(Mathf.Tan(0.5f * hFovDeg * Mathf.Deg2Rad) / deviceAspectRatio) * Mathf.Rad2Deg;

        if (vFovDegNew > vFovDegOld)
        {
            //only allow vertical fov to be increased to accommodate lower aspect ratios like 2:1, otherwise undo button is cut off such as in 4:3
            mainCamera.fieldOfView = vFovDegNew;
        }
    }
}
