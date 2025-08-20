using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player
{
    public string userId;
    public string name;
    public bool active;
    public bool ready;
    public bool host;
    public bool roomId;

    public Player(string userId, string name, bool active, bool ready, bool host, bool roomId)  
    {
        this.userId = userId;
        this.name = name;
        this.active = active;
        this.ready = ready;
        this.host = host;
        this.roomId = roomId;
    }
    public Player() { }
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}
