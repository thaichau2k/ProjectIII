using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
  [SerializeField] private GameObject playerPrefab;
  [SerializeField] private float minX;
  [SerializeField] private float maxX;

  private void Start()
  {
    Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), -4, 0);
    GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
  }
}
