using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;

public class PlayerListEntry : MonoBehaviour
{
  [Header("UI References")]
  public Text PlayerNameText;
  public Button PlayerReadyButton;
  public Image PlayerReadyImage;

  private int ownerId;
  private bool isPlayerReady;

  #region UNITY

  public void Start()
  {
    if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
    {
      PlayerReadyButton.gameObject.SetActive(false);
    }
    else
    {
      Hashtable initialProps = new Hashtable() { { "isPlayerReady", isPlayerReady } };
      PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
      PhotonNetwork.LocalPlayer.SetScore(0);

      PlayerReadyButton.onClick.AddListener(() =>
      {
        isPlayerReady = !isPlayerReady;
        SetPlayerReady(isPlayerReady);

        Hashtable props = new Hashtable() { { "isPlayerReady", isPlayerReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        if (PhotonNetwork.IsMasterClient)
          FindObjectOfType<RoomUI>().LocalPlayerPropertiesUpdated();
      });
    }
  }

  #endregion

  public void Initialize(int playerId, string playerName)
  {
    ownerId = playerId;
    PlayerNameText.text = playerName;
  }

  public void SetPlayerReady(bool playerReady)
  {
    PlayerReadyButton.GetComponentInChildren<Text>().text = playerReady ? "Ready!" : "Ready?";
    PlayerReadyImage.enabled = playerReady;
  }
}