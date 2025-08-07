using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingMenu;
    [SerializeField] private GameObject connectedMenu;
    [SerializeField] private GameObject joinRoomMenu;
    [SerializeField] private GameObject lobby;
    
    

    [SerializeField] private Button buttonSetting;
    [SerializeField] private Button buttonCancelSetting;
    [SerializeField] private Button buttonSaveSetting;
    [SerializeField] private Button buttonStartAndConnect;
    [SerializeField] private Button buttonDisconnect;
    [SerializeField] private Button buttonCreateRoom;
    [SerializeField] private Button buttonOpenJoinRoomMenu;
    [SerializeField] private Button buttonJoinRoom;
    [SerializeField] private Button buttonCancelJoinRoom;
    [SerializeField] private Button buttonLeaveRoom;
    [SerializeField] private Button buttonReady;
    [SerializeField] private Button buttonHostStart;


    [SerializeField] private TMP_InputField urlInput;
    [SerializeField] private TMP_InputField roomIdInput;
    [SerializeField] private TMP_Text textConnected;
    [SerializeField] private TMP_Text textRoomId;
    [SerializeField] private GameObject lobbyScrollViewContent;
    [SerializeField] private GameObject lobbyListItemBase;

    public void OpenSettingMenu()
    {
        mainMenu.SetActive(false);
        settingMenu.SetActive(true);
        urlInput.text = WebSocketClient.Instance.url;
        Debug.Log("URL: " + WebSocketClient.Instance.url);
    }

    public void CloseSettingMenuAndCancel()
    {
        settingMenu.SetActive(false);
        mainMenu.SetActive(true);
        Debug.Log("URL: " + WebSocketClient.Instance.url);
    }

    public void CloseSettingMenuAndSave()
    {
        string url = urlInput.text;
        WebSocketClient.Instance.url = url;
        settingMenu.SetActive(false);
        mainMenu.SetActive(true);
        Debug.Log("URL: " + WebSocketClient.Instance.url);
    }

    public async Task HandleStartButton()
    {
        Action callback = null;
        callback = () =>
        {
            mainMenu.SetActive(false);
            connectedMenu.SetActive(true);
            textConnected.text = "Connected to\n" + WebSocketClient.Instance.url;

            WebSocketClient.Instance.OnConnected -= callback;
            Debug.Log("Connected to WebSocket server: " + WebSocketClient.Instance.url);
        };
        WebSocketClient.Instance.OnConnected += callback;
        await WebSocketClient.Instance.ConnectAsync();
    }

    public async Task HandleDisconnectButton()
    {
        await WebSocketClient.Instance.CloseAsync();
        connectedMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OpenJoinRoomMenu()
    {
        connectedMenu.SetActive(false);
        joinRoomMenu.SetActive(true);
        roomIdInput.text = "";
        Debug.Log("Join Room Menu Opened");
    }

    public void CloseJoinRoomMenu()
    {
        joinRoomMenu.SetActive(false);
        connectedMenu.SetActive(true);
        Debug.Log("Join Room Menu Closed");
    }

    public async Task HandleCreateRoomButton()
    {
        await RoomManager.Instance.CreateRoom();
        RoomManager.Instance.OnRoomRefreshed += async (Room room) =>
        {
            connectedMenu.SetActive(false);
            lobby.SetActive(true);
            await RefreshLobbyList(room);
        };
    }

    public async Task HandleJoinRoomButton()
    {
        string roomId = roomIdInput.text;
        await RoomManager.Instance.JoinRoom(roomId);
        
        Debug.Log("Join Room Button Pressed");
        RoomManager.Instance.OnRoomRefreshed += async (Room room) =>
        {
            joinRoomMenu.SetActive(false);
            lobby.SetActive(true);
            await RefreshLobbyList(room);
        };
    }

    public async Task HandleLeaveRoomButton()
    {
        Debug.Log("Leave Room Button Pressed");
        var tcs = new TaskCompletionSource<bool>();
        await RoomManager.Instance.LeaveRoom();

        Action<string> callback = null;
        callback = (string text) =>
        {
            try
            {
                Message<string> message = JsonUtility.FromJson<Message<string>>(text);
                if (message.msgType == Message<string>.MsgType.ROOM_LEFT)
                {
                    tcs.SetResult(true);
                    lobby.SetActive(false);
                    connectedMenu.SetActive(true);
                    textRoomId.text = "";
                    Debug.Log("Left Room");
                    WebSocketClient.Instance.OnMessageReceived -= callback;
                }
            }
            catch (Exception ex)
            {
                tcs.SetResult(false);
                Debug.LogError($"Error processing leave room message: {ex.Message}");
            }

        };
        WebSocketClient.Instance.OnMessageReceived += callback;
        await tcs.Task;
        
        Debug.Log("Leave Room Button Finished");
        
    }


    public async Task RefreshLobbyList(Room room)
    {
        if (room == null) return;
        textRoomId.text = "Room ID:\n" + room.id;
        foreach (Transform child in lobbyScrollViewContent.transform)
        {
            if (child != lobbyListItemBase.transform)
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < room.playerList.Count; i++)
        {
            Player player = room.playerList[i];
            GameObject lobbyListItem = Instantiate(lobbyListItemBase, lobbyScrollViewContent.transform);
            lobbyListItem.SetActive(true);
            TMP_Text[] texts = lobbyListItem.GetComponentsInChildren<TMP_Text>();
            Debug.Log(player.ToString());
            texts[0].text = (i + 1).ToString();
            texts[1].text = player.name;
            texts[2].text = player.host ? "Host" : "Player";
            texts[3].text = player.ready ? "Ready" : "Not Ready";
        }
        buttonReady.GetComponentInChildren<TMP_Text>().text = RoomManager.Instance.player.ready ? "Ready: Yes" : "Ready: No";
        buttonHostStart.gameObject.SetActive(RoomManager.Instance.player.host);
    }

    public async Task HandleReadyButton()
    {
        if (RoomManager.Instance.room == null || RoomManager.Instance.player == null)
        {
            Debug.LogError("Cannot ready, not in a room or player not found.");
            return;
        }
        bool ready = !RoomManager.Instance.player.ready;

        await RoomManager.Instance.Ready(ready);
    }




    // Start is called before the first frame update
    void Start()
    {
        mainMenu.SetActive(true);

        foreach (GameObject menu in new GameObject[] { settingMenu, connectedMenu, joinRoomMenu, lobby })
        {
            menu.SetActive(false);
        }

        buttonSetting.onClick.AddListener(OpenSettingMenu);
        buttonCancelSetting.onClick.AddListener(CloseSettingMenuAndCancel);
        buttonSaveSetting.onClick.AddListener(CloseSettingMenuAndSave);
        buttonOpenJoinRoomMenu.onClick.AddListener(OpenJoinRoomMenu);
        buttonCancelJoinRoom.onClick.AddListener(CloseJoinRoomMenu);

        buttonStartAndConnect.onClick.AddListener(async () =>
        {
            await HandleStartButton();
        });
        buttonDisconnect.onClick.AddListener(async () =>
        {
            await HandleDisconnectButton();
        });
        buttonCreateRoom.onClick.AddListener(async () =>
        {
            await HandleCreateRoomButton();
        });
        buttonJoinRoom.onClick.AddListener(async () =>
        {
            await HandleJoinRoomButton();
        });
        buttonLeaveRoom.onClick.AddListener(async () =>
        {
            await HandleLeaveRoomButton();
        });
        buttonReady.onClick.AddListener(async () =>
        {
            await HandleReadyButton();
        });
        
    }


}
