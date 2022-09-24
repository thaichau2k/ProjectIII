using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
  public static NetworkManager instance;
  public Dictionary<string, RoomInfo> cachedRoomList;

  private void Awake()
  {
    DontDestroyOnLoad(gameObject);

    if (instance != null && instance != this)
      gameObject.SetActive(false);
    else instance = this;

    cachedRoomList = new Dictionary<string, RoomInfo>();
  }

  public void Connect(string _nickName)
  {
    if (_nickName != "")
    {
      PhotonNetwork.NickName = _nickName;
      PhotonNetwork.AutomaticallySyncScene = true;
      PhotonNetwork.ConnectUsingSettings();
      Debug.Log("Photon Connect " + _nickName);
    }
  }

  public override void OnConnectedToMaster()
  {
    Debug.Log("Connected to the" + PhotonNetwork.CloudRegion + " server!");
    GameManager.instance.ChangeScene("HomeScene");
    JoinLobby();
  }

  public override void OnCreatedRoom()
  {
    Debug.Log("Created Room: " + PhotonNetwork.CurrentRoom.Name);
    PhotonNetwork.LoadLevel("RoomScene");
  }

  public override void OnDisconnected(DisconnectCause cause)
  {
    base.OnDisconnected(cause);
    Debug.Log("Disconnected cause: " + cause);
  }

  public override void OnJoinedLobby()
  {
    cachedRoomList.Clear();
    HomeUI.instance.ClearRoomListView();
  }

  public override void OnLeftLobby()
  {
    cachedRoomList.Clear();
    HomeUI.instance.ClearRoomListView();
  }

  public override void OnRoomListUpdate(List<RoomInfo> roomList)
  {
    HomeUI.instance.ClearRoomListView();

    UpdateCachedRoomList(roomList);
    HomeUI.instance.UpdateRoomListView();
  }

  private void UpdateCachedRoomList(List<RoomInfo> roomList)
  {
    foreach (RoomInfo info in roomList)
    {
      // Remove room from cached room list if it got closed, became invisible or was marked as removed
      if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
      {
        if (cachedRoomList.ContainsKey(info.Name))
        {
          cachedRoomList.Remove(info.Name);
        }

        continue;
      }

      // Update cached room info
      if (cachedRoomList.ContainsKey(info.Name))
      {
        cachedRoomList[info.Name] = info;
      }
      // Add new room info to cache
      else
      {
        cachedRoomList.Add(info.Name, info);
      }
    }
  }

  public void CreateRoom(string roomName)
  {
    Debug.Log("Room Created!!!");
    PhotonNetwork.CreateRoom(roomName, new RoomOptions()
    {
      MaxPlayers = 2,
      BroadcastPropsChangeToAll = true
    }, TypedLobby.Default);
  }

  public void JoinRoom(string roomName)
  {
    Debug.Log("Room Joined!!!");
    PhotonNetwork.LoadLevel("RoomScene");
    PhotonNetwork.JoinRoom(roomName);
  }

  public void JoinLobby()
  {
    Debug.Log("Lobby Joined!!!");
    PhotonNetwork.JoinLobby();
  }

  public void LeaveRoom()
  {
    Debug.Log("Left Room!!!");
    PhotonNetwork.LeaveRoom();
  }

  public void Disconnect()
  {
    Debug.Log("Disconnect!!!");
    PhotonNetwork.Disconnect();
  }
}