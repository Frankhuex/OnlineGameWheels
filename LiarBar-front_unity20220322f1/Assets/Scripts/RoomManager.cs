using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RoomManager
{
    private static RoomManager _instance;
    public static RoomManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new RoomManager();
            }
            return _instance;
        }
    }

    public Room room;
    public Player player;
    public RoomManager()
    {
        room = null;
        WebSocketClient.Instance.OnMessageReceived += (string text) => RefreshRoom(text);
    }

    public event Action<Room> OnRoomRefreshed;
    public void RefreshRoom(string text)
    {
        Debug.Log("Refreshing room: " + text);
        try
        {
            Message<Room> msg = Message<Room>.FromJson<Message<Room>>(text);
            Debug.Log("Message parsed");
            if (msg.msgType == Message<Room>.MsgType.ROOM_PLAYERS_LIST)
            {
                room = msg.data;
                player = room.playerList.Find(x => x.userId == WebSocketClient.Instance.userId);
                Debug.Log("Room ID: " + room.id);
                Debug.Log("Room refreshed: " + room.ToString());
                Debug.Log("Player joined: " + player.ToString());
                OnRoomRefreshed?.Invoke(room);
            }
            else
            {
                Debug.LogError("Not refreshing room.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error parsing message: " + ex.Message);
        }
    }

    public async Task CreateRoom()
    {
        Message<string> msg = new Message<string>(Message<string>.MsgType.CREATE_ROOM, "test");
        await WebSocketClient.Instance.SendMessageAsync(msg);
    }

    public async Task JoinRoom(string roomId)
    {
        Message<string> msg = new Message<string>(Message<string>.MsgType.JOIN_ROOM, roomId);
        await WebSocketClient.Instance.SendMessageAsync(msg);
    }

    public async Task LeaveRoom()
    {
        if (room == null)
        {
            Debug.LogError("Cannot leave room, not in a room.");
            return;
        }
        Message<string> msg = new Message<string>(Message<string>.MsgType.LEAVE_ROOM, room.id);
        await WebSocketClient.Instance.SendMessageAsync(msg);
        room = null; // Clear the room after leaving
        player = null;
    }

    public async Task Ready(bool ready)
    {
        if (room == null || player == null)
        {
            Debug.LogError("Cannot change ready status, not in a room or player not found.");
            return;
        }
        Message<bool> msg = new Message<bool>(Message<bool>.MsgType.PREPARE, ready);
        await WebSocketClient.Instance.SendMessageAsync(msg);
    }

}
