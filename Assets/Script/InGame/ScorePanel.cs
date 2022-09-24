using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;
using TMPro;

public class ScorePanel : MonoBehaviourPunCallbacks
{
  [SerializeField] private TMP_Text otherPlayerScore;
  [SerializeField] private TMP_Text masterPlayerScore;

  public override void OnEnable()
  {
    base.OnEnable();

    foreach (Player p in PhotonNetwork.PlayerList)
    {
      if (p.IsMasterClient) masterPlayerScore.text = p.NickName + ": " + p.GetScore();
      else otherPlayerScore.text = p.NickName + ": " + p.GetScore();
    }
  }

  public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
  {
    base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

    if (targetPlayer.IsMasterClient) masterPlayerScore.text = targetPlayer.NickName + ": " + targetPlayer.GetScore();
    else otherPlayerScore.text = targetPlayer.NickName + ": " + targetPlayer.GetScore();

    if (targetPlayer.GetScore() >= 10) BlockGameManager.instance.GetComponent<PhotonView>().RPC("EndGame", RpcTarget.All, targetPlayer.ActorNumber);
  }
}
