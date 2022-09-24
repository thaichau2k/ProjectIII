using System;

[Serializable]
public class UserModel
{
  string _username;
  int _winCount;
  int _totalGames;

  public UserModel() { }

  public UserModel(string username, int winCount, int totalGames)
  {
    this._username = username;
    this._winCount = winCount;
    this._totalGames = totalGames;
  }

  public string getUsername()
  {
    return _username;
  }

  public void setUsername(string username)
  {
    this._username = username;
  }

  public int getWinCount()
  {
    return _winCount;
  }

  public void setWinCount(int winCount)
  {
    this._winCount = winCount;
  }

  public int getTotalGames()
  {
    return _totalGames;
  }

  public void setTotalGames(int total)
  {
    this._totalGames = total;
  }
}
