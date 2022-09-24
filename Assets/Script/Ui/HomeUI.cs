using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class HomeUI : MonoBehaviour
{
  public static HomeUI instance;
  private DatabaseReference db;
  [SerializeField] private Button signOutBtn;

  [Header("User Profile")]
  [SerializeField] private Button profileBtn;
  [SerializeField] private GameObject profileLayer;
  [SerializeField] private TMP_InputField username;
  [SerializeField] private TMP_Text winrate;
  [SerializeField] private Button updateNameBtn;

  [Header("Lobby")]
  [SerializeField] private Button lobbyBtn;
  [SerializeField] private GameObject lobbyLayer;
  [SerializeField] private Button createRoomBtn; //just link to game scene for database right now
  [SerializeField] private TMP_InputField roomName;
  [SerializeField] private GameObject roomItemPrefab;
  [SerializeField] private Transform roomItemHolder;
  private Dictionary<string, GameObject> roomListEntries;


  [Header("Leaderboard")]
  [SerializeField] private Button LeaderboardBtn;
  [SerializeField] private GameObject leaderboardLayer;
  [SerializeField] private List<GameObject> playerInfoItem;
  private List<UserModel> userList;

  private void Awake()
  {
    if (instance == null) instance = this;
    else if (instance != this)
    {
      Destroy(instance.gameObject);
      instance = this;
    }

    roomListEntries = new Dictionary<string, GameObject>();
  }

  void Start()
  {
    db = FirebaseManager.instance.dbReference;
    GetUserData();
    LobbyScreen();
    RegisterEvent();
  }

  private void ClearUi()
  {
    lobbyLayer.SetActive(false);
    profileLayer.SetActive(false);
    leaderboardLayer.SetActive(false);
  }

  private void LobbyScreen()
  {
    ClearUi();
    lobbyLayer.SetActive(true);
  }

  private void ProfileScreen()
  {
    ClearUi();
    GetUserData();
    profileLayer.SetActive(true);
  }

  private void LeaderboardScreen()
  {
    ClearUi();
    ResetLeaderboard();
    leaderboardLayer.SetActive(true);
  }

  private void RegisterEvent()
  {
    lobbyBtn.onClick.AddListener(() => { LobbyScreen(); });
    profileBtn.onClick.AddListener(() => { ProfileScreen(); });
    LeaderboardBtn.onClick.AddListener(() => { LeaderboardScreen(); });

    signOutBtn.onClick.AddListener(() =>
    {
      FirebaseManager.instance.SignOut();
      NetworkManager.instance.Disconnect();
      UserManager.instance.BackToLoginScene();
    });

    username.onValueChanged.AddListener(name =>
    {
      updateNameBtn.interactable = Validate(name);
    });

    updateNameBtn.onClick.AddListener(() =>
    {
      FirebaseManager.instance.UpdateUsername(username.text, false);
      updateNameBtn.interactable = false;
      UserManager.instance.SetUsername(username.text);
    });

    createRoomBtn.onClick.AddListener(() =>
    {
      if (roomName.text != "" && roomName.text != null)
        NetworkManager.instance.CreateRoom(roomName.text);
    });
  }

  #region User Profile

  public void GetUserData()
  {
    username.text = UserManager.instance.GetUsername();
    winrate.text = "Win rate: " + (double)UserManager.instance.GetWin() +
                   "/" + (double)UserManager.instance.GetTotalGames();
  }

  private bool Validate(string name)
  {
    if (name == "") return false;
    if (name == FirebaseManager.instance._user.DisplayName) return false;

    return true;
  }

  #endregion

  #region Leaderboard

  public void ResetLeaderboard()
  {
    userList = new List<UserModel>();
    StartCoroutine(ILoadScoreboardData());
  }

  private IEnumerator ILoadScoreboardData()
  {
    int index = 0;
    var DBTask = db.Child("users").OrderByChild("number_of_wins").LimitToLast(10).GetValueAsync();

    yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

    if (DBTask.Exception != null)
      Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
    else
    {
      //Data has been retrieved
      DataSnapshot snapshot = DBTask.Result;

      //Loop through every users UID
      foreach (DataSnapshot childSnapshot in snapshot.Children)
      {
        UserModel user = new UserModel();
        user.setTotalGames(int.Parse(childSnapshot.Child("games_played").Value.ToString()));
        user.setWinCount(int.Parse(childSnapshot.Child("number_of_wins").Value.ToString()));
        user.setUsername(childSnapshot.Child("username").Value.ToString());

        userList.Add(user);
      }

      foreach (var item in userList)
      {
        var scoreboardItemController = playerInfoItem[index].GetComponent<ScoreboardItem>();
        scoreboardItemController.SetUpItem(item.getUsername(), item.getWinCount(), item.getTotalGames());

        index++;
      }
    }
  }

  #endregion

  #region  Lobby

  public void UpdateRoomListView()
  {
    foreach (RoomInfo info in NetworkManager.instance.cachedRoomList.Values)
    {
      GameObject entry = Instantiate(roomItemPrefab);
      entry.transform.SetParent(roomItemHolder.transform);
      entry.transform.localScale = Vector3.one;
      entry.GetComponent<RoomInfoItem>().SetRoomName(info.Name);

      roomListEntries.Add(info.Name, entry);
    }
  }

  public void ClearRoomListView()
  {
    foreach (GameObject entry in roomListEntries.Values)
    {
      Destroy(entry.gameObject);
    }

    roomListEntries.Clear();
  }

  #endregion
}


