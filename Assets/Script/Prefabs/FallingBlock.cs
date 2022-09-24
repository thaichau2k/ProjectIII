using UnityEngine;
using Photon.Pun;

public class FallingBlock : MonoBehaviour
{
  private bool isDestroyed;
  float visisbleHeightThreshold;
  private PhotonView photonView;

#pragma warning disable 0109
  private new Rigidbody rigidbody;
#pragma warning restore 0109

  public void Awake()
  {
    photonView = GetComponent<PhotonView>();

    rigidbody = GetComponent<Rigidbody>();

    if (photonView.InstantiationData != null)
    {
      rigidbody.AddForce((Vector3)photonView.InstantiationData[0], ForceMode.Impulse);
      transform.localScale = (float)photonView.InstantiationData[1] * Vector2.one;
    }

    visisbleHeightThreshold = -Camera.main.orthographicSize - transform.localScale.y;
  }

  public void Update()
  {
    if (!photonView.IsMine)
      return;

    if (transform.position.y < visisbleHeightThreshold) PhotonNetwork.Destroy(gameObject);
  }

  public void OnCollisionEnter(Collision collision)
  {
    if (isDestroyed)
    {
      return;
    }

    if (collision.gameObject.CompareTag("Player"))
    {
      if (photonView.IsMine)
      {
        PhotonView playerView = collision.gameObject.GetComponent<PhotonView>();
        playerView.RPC("HandleGetHit", RpcTarget.All, playerView.Owner.ActorNumber);

        DestroyBlockGlobally();
      }
    }
  }


  private void DestroyBlockGlobally()
  {
    isDestroyed = true;

    PhotonNetwork.Destroy(gameObject);
  }

  private void DestroyBlockLocally()
  {
    isDestroyed = true;

    GetComponent<Renderer>().enabled = false;
  }
}

