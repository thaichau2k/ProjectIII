using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomInfoItem : MonoBehaviour
{
  [SerializeField] private TMP_Text roomName;
  [SerializeField] private Button selectRoomBtn;

  private void Start()
  {
    selectRoomBtn.onClick.AddListener(() =>
    {
      if (roomName.text != "" && roomName.text != null)
        NetworkManager.instance.JoinRoom(roomName.text);
      else Debug.Log("Room name Not Found!!!");
    });
  }

  public void SetRoomName(string _roomName)
  {
    this.roomName.text = _roomName;
  }
}
