using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

public class RoomDto {

    public string Player1 { get; set; }
    public string Player2 { get; set; }

    public bool IsBlacksTurn { get; set; }
    public bool IsWhitesTurn { get; set; }
    public Point OpponentsLastMove { get; set; }
}
