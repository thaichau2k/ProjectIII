using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreboardItem : MonoBehaviour
{
  [SerializeField] private TMP_Text _username;
  [SerializeField] private TMP_Text _winCnt;
  [SerializeField] private TMP_Text _winrate;

  public void SetUpItem(string username, int winCnt, int total)
  {
    this._username.text = username;
    this._winCnt.text = winCnt.ToString();

    if (total != 0)
    {
      double x = ((double)winCnt / (double)total) * 100;
      this._winrate.text = System.Math.Round(x) + "%";
    }
    else this._winrate.text = "...%";
  }
}
