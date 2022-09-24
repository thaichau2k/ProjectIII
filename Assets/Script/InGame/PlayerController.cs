using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerController : MonoBehaviour
{
  private float speed = 7f;
  float screenHalfWidth;
  public PhotonView view;
  [SerializeField] private TextMeshProUGUI playerName;
  private Animator ani;
  private bool isFacingRight = true;

  private void Start()
  {
    float halfPlayerWidth = transform.localScale.x / 2;
    screenHalfWidth = Camera.main.aspect * Camera.main.orthographicSize + halfPlayerWidth;
    SetName();
    ani = GetComponent<Animator>();
  }

  private void FixedUpdate()
  {
    if (view.IsMine)
    {
      float inputX = Input.GetAxisRaw("Horizontal");
      float velocity = inputX * speed;
      transform.Translate(Vector2.right * velocity * Time.deltaTime);

      if (inputX != 0) ani.SetBool("isRunning", true);
      else ani.SetBool("isRunning", false);

      if (inputX > 0 && !isFacingRight) Flip();
      else if (inputX < 0 && isFacingRight) Flip();

      if (transform.position.x < -screenHalfWidth)
        transform.position = new Vector2(screenHalfWidth, transform.position.y);
      if (transform.position.x > screenHalfWidth)
        transform.position = new Vector2(-screenHalfWidth, transform.position.y);
    }
  }

  [PunRPC]
  public void HandleGetHit(int actorNumber)
  {
    if (!BlockGameManager.instance.isQuiz)
    {
      BlockGameManager.instance.StorePlayerGetHit(actorNumber);
      BlockGameManager.instance.OnPopupQuiz();
    }
  }

  private void SetName()
  {
    playerName.text = view.Owner.NickName;
  }

  private void Flip()
  {
    Vector3 currentScale = gameObject.transform.localScale;
    currentScale.x *= -1;
    gameObject.transform.localScale = currentScale;

    isFacingRight = !isFacingRight;
  }
}
