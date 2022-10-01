using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Room : MonoBehaviour
{

    public int ROOM_ID;
    public string ROOM_Key;
    public int TURN_TIME;
    public string PLAYER_STATUS;
    public int JOINED_PLAYER_ID;
    public int ROOM_OWNER;
}
