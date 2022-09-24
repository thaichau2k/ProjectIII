using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class RoomUI : MonoBehaviourPunCallbacks
{
  public static RoomUI instance;

  [SerializeField] private Button startBtn;
  [SerializeField] private Button leaveBtn;
  [SerializeField] private TextMeshProUGUI roomName;
  [SerializeField] private GameObject roomPanel;
  public GameObject PlayerListEntryPrefab;

  private Dictionary<int, GameObject> playerListEntries;

  private void Awake()
  {
    if (instance == null) instance = this;
    else if (instance != this)
    {
      Destroy(instance.gameObject);
      instance = this;
    }
  }

  private void Start()
  {
    Init();
    RegisterEvent();
    OnJoinedRoom();
  }

  private void Init()
  {
    startBtn.gameObject.SetActive(false);
    leaveBtn.gameObject.SetActive(true);
  }

  private void RegisterEvent()
  {
    startBtn.onClick.AddListener(() =>
    {
      PhotonNetwork.CurrentRoom.IsOpen = false;
      PhotonNetwork.CurrentRoom.IsVisible = false;

      PhotonNetwork.LoadLevel("GameScene");
    });

    leaveBtn.onClick.AddListener(() =>
    {
      NetworkManager.instance.LeaveRoom();
    });
  }

  public void LocalPlayerPropertiesUpdated()
  {
    startBtn.gameObject.SetActive(CheckPlayersReady());
  }

  private bool CheckPlayersReady()
  {
    if (!PhotonNetwork.IsMasterClient)
      return false;

    foreach (Player p in PhotonNetwork.PlayerList)
    {
      object isPlayerReady;
      if (p.CustomProperties.TryGetValue("isPlayerReady", out isPlayerReady))
      {
        if (!(bool)isPlayerReady) return false;
      }
      else return false;
    }

    return true;
  }

  public override void OnJoinedRoom()
  {
    if (playerListEntries == null)
      playerListEntries = new Dictionary<int, GameObject>();

    foreach (Player p in PhotonNetwork.PlayerList)
    {
      GameObject entry = Instantiate(PlayerListEntryPrefab);
      entry.transform.SetParent(roomPanel.transform);
      entry.transform.localScale = Vector3.one;
      entry.GetComponent<PlayerListEntry>().Initialize(p.ActorNumber, p.NickName);

      object isPlayerReady;
      if (p.CustomProperties.TryGetValue("isPlayerReady", out isPlayerReady))
      {
        entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
      }

      playerListEntries.Add(p.ActorNumber, entry);
    }

    startBtn.gameObject.SetActive(CheckPlayersReady());

    Hashtable props = new Hashtable { { "PlayerLoadedLevel", false } };
    PhotonNetwork.LocalPlayer.SetCustomProperties(props);

    Debug.Log(PhotonNetwork.CurrentRoom.Name);
    roomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
  }

  public override void OnPlayerEnteredRoom(Player newPlayer)
  {
    GameObject entry = Instantiate(PlayerListEntryPrefab);
    entry.transform.SetParent(roomPanel.transform);
    entry.transform.localScale = Vector3.one;
    entry.GetComponent<PlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

    playerListEntries.Add(newPlayer.ActorNumber, entry);

    startBtn.gameObject.SetActive(CheckPlayersReady());
  }

  public override void OnPlayerLeftRoom(Player otherPlayer)
  {
    Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
    playerListEntries.Remove(otherPlayer.ActorNumber);

    startBtn.gameObject.SetActive(CheckPlayersReady());
  }

  public override void OnMasterClientSwitched(Player newMasterClient)
  {
    if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
    {
      startBtn.gameObject.SetActive(CheckPlayersReady());
    }

  }

  public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
  {
    if (playerListEntries == null)
    {
      playerListEntries = new Dictionary<int, GameObject>();
    }

    GameObject entry;
    if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
    {
      object isPlayerReady;
      if (changedProps.TryGetValue("isPlayerReady", out isPlayerReady))
      {
        entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
      }
    }

    startBtn.gameObject.SetActive(CheckPlayersReady());
  }
}
