using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
{
    public const string CANVAS = "Canvas";
    public const string CAMERA_ANDROID = "CameraAndroid";
    public const string CAMERA_PC = "CameraPC";

    public const string BLACK_STONE = "BlackStone";
    public const string WHITE_STONE = "WhiteStone";
    public const string MOVE_NUMBER_TEXT = "MoveNumberText";
    public const string BLACK_USER_INTERFACE = "BlackUserInterface";
    public const string WHITE_USER_INTERFACE = "WhiteUserInterface";
    public const string OFFICE_PROPS = "OfficeProps";
    public const string BLACK_PROPS = "BlackProps";
    public const string WHITE_PROPS = "WhiteProps";
    public const string PROP = "Prop";

    public const string P1_LABEL_GAMEOBJECT = "P1Label";
    public const string P2_LABEL_GAMEOBJECT = "P2Label";
    public const string ONLINE_MATCHMAKING_FORM = "OnlineMatchmakingForm";

    public static Quaternion QuaternionTowardsBlack = Quaternion.Euler(90, 0, 90);
    public static Quaternion IllegalMoveQuaternion = Quaternion.Euler(0, -90, 0);
    public static Quaternion QuaternionTowardsWhite = Quaternion.Euler(90, 0, -90);
}
