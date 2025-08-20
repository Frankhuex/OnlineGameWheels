using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Room
{
    public string id;
    public List<Player> playerList;
    public int maxPlayers;
    public bool started;

    public Room(string id, int maxPlayers)
    {
        this.id = id;
        this.maxPlayers = maxPlayers;
        playerList = new List<Player>();
        started = false;
    }
    public Room() { }
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}
