using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

public class UserManager : MonoBehaviour
{
  public static UserManager instance;

  private string username;
  private int winCount;
  private int totalGames;

  private void Awake()
  {
    DontDestroyOnLoad(gameObject);

    if (instance == null) instance = this;
    else if (instance != this)
    {
      Destroy(instance.gameObject);
      instance = this;
    }
  }

  public string GetUsername()
  {
    return this.username;
  }

  public void SetUsername(string _username)
  {
    this.username = _username;
  }

  public int GetWin()
  {
    return this.winCount;
  }

  public void SetWin(int _win)
  {
    this.winCount = _win;
  }

  public int GetTotalGames()
  {
    return this.totalGames;
  }

  public void setTotalGames(int _total)
  {
    this.totalGames = _total;
  }

  public void BackToLoginScene()
  {
    username = "";
    winCount = 0;
    totalGames = 0;
  }

  public IEnumerator GetUserData()
  {
    var DBTask = FirebaseManager.instance.dbReference.Child("users").Child(FirebaseManager.instance._user.UserId).GetValueAsync();

    yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

    if (DBTask.Exception != null)
      Debug.LogError(message: $"Failed to register task with {DBTask.Exception}");
    else if (DBTask.Result.Value == null)
    {
      //No data exists yet
      Debug.LogError("null?");
    }
    else
    {
      //Data has been retrieved
      DataSnapshot snapshot = DBTask.Result;

      username = snapshot.Child("username").Value.ToString();
      winCount = int.Parse(snapshot.Child("number_of_wins").Value.ToString());
      totalGames = int.Parse(snapshot.Child("games_played").Value.ToString());
    }
  }
}
