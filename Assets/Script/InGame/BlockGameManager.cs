using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System;
using TMPro;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

public static class ThreadSafeRandom
{
  [ThreadStatic] private static System.Random Local;

  public static System.Random ThisThreadsRandom
  {
    get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
  }
}

static class MyExtensions
{
  public static void Shuffle<T>(this IList<T> list)
  {
    int n = list.Count;
    while (n > 1)
    {
      n--;
      int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
      T value = list[k];
      list[k] = list[n];
      list[n] = value;
    }
  }
}

public class BlockGameManager : MonoBehaviourPunCallbacks
{
  public static BlockGameManager instance;
  private DatabaseReference db;
  public PhotonView view;

  [SerializeField] private GameObject scorePanel;

  [Header("Game Over")]
  [SerializeField] private GameObject gameOverScreen;
  [SerializeField] private Text gameOverTxt;
  private bool isGameOver;

  [Header("Quiz Popup")]
  [SerializeField] private GameObject quizPopup;
  [SerializeField] private TMP_Text question;
  [SerializeField] private TMP_Text[] choicesTxt;
  [SerializeField] private Button[] choicesBtn;
  [SerializeField] private Image progressBar;
  private List<QuizModel> quizList = new List<QuizModel>();
  private string currentQuiz = "";
  private int hitPlayerActorNumber = -1;
  public bool isQuiz;

  private void Awake()
  {
    if (instance == null) instance = this;
    else if (instance != this)
    {
      Destroy(instance.gameObject);
      instance = this;
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    Init();
    SetQuizList();
    RegisterEvent();
  }

  int wrongChoice = 0;
  bool isCoroutine = false;
  // Update is called once per frame
  void Update()
  {
    if (isGameOver)
    {
      StartCoroutine(HidePopupQuiz(0.5f));
    }

    if (quizPopup.activeSelf && !isCoroutine)
    {
      if (wrongChoice == 2)
      {
        StartCoroutine(HidePopupQuiz(1.0f));
        isCoroutine = true;
      }
      else
      {
        progressBar.fillAmount -= 1.0f / 10.0f * Time.deltaTime;
        if (progressBar.fillAmount <= 0)
        {
          StartCoroutine(HidePopupQuiz(0.25f));
          isCoroutine = true;
        }
      }
    }
  }

  private void Init()
  {
    gameOverScreen.SetActive(false);
    quizPopup.SetActive(false);
    scorePanel.SetActive(true);
    isGameOver = false;

    //update number of games user played
    UserManager.instance.setTotalGames(UserManager.instance.GetTotalGames() + 1);
    FirebaseManager.instance.UpdateUserTotalGames(UserManager.instance.GetTotalGames());
  }

  private void SetQuizList()
  {
    if (PhotonNetwork.IsMasterClient)
    {
      db = FirebaseManager.instance.dbReference;
      StartCoroutine(GetQuizList());
    }
  }

  private void RegisterEvent()
  {
    choicesBtn[0].onClick.AddListener(() => { view.RPC("ChooseAnswer", RpcTarget.All, 0); });
    choicesBtn[1].onClick.AddListener(() => { view.RPC("ChooseAnswer", RpcTarget.All, 1); });
    choicesBtn[2].onClick.AddListener(() => { view.RPC("ChooseAnswer", RpcTarget.All, 2); });
    choicesBtn[3].onClick.AddListener(() => { view.RPC("ChooseAnswer", RpcTarget.All, 3); });
  }

  public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
  {
    object quiz;
    if (propertiesThatChanged.TryGetValue("Quiz", out quiz))
    {
      currentQuiz = quiz.ToString();
    }
  }

  [PunRPC]
  private void ChooseAnswer(int i, PhotonMessageInfo info)
  {
    string[] quizComponent = currentQuiz.Split('#');

    if (choicesTxt[i].text == quizComponent[1])
    {
      foreach (Button btn in choicesBtn) btn.interactable = false;
      choicesBtn[i].GetComponent<Image>().color = Color.green;

      //Remove quiz popup
      StartCoroutine(HidePopupQuiz(1));

      //Handle Score
      if (info.Sender.ActorNumber == hitPlayerActorNumber)
        info.Sender.AddScore(1);
      else info.Sender.AddScore(2);
    }
    else
    {
      if (info.Sender == PhotonNetwork.LocalPlayer)
        foreach (Button btn in choicesBtn) btn.interactable = false;

      choicesBtn[i].GetComponent<Image>().color = Color.red;
      choicesBtn[i].interactable = false;
      wrongChoice++;
    }
  }

  public void StorePlayerGetHit(int actorNumber)
  {
    hitPlayerActorNumber = actorNumber;
  }

  public void OnPopupQuiz()
  {
    if (!isGameOver)
    {
      showQuiz();
      progressBar.fillAmount = 1;
      ShowPopupQuiz();
    }
  }

  private void ShowPopupQuiz()
  {
    BlockSpawner.instance.isSpawn = false;
    quizPopup.SetActive(true);
    isQuiz = true;
  }

  private IEnumerator HidePopupQuiz(float seconds)
  {
    yield return new WaitForSeconds(seconds);

    if (!isGameOver)
      BlockSpawner.instance.isSpawn = true;
    quizPopup.SetActive(false);
    isQuiz = false;
    ResetQuizPopup();
  }

  private IEnumerator GetQuizList()
  {
    var DBTask = db.Child("question_bank").GetValueAsync();

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

      foreach (DataSnapshot childSnapshot in snapshot.Children)
      {
        QuizModel quizItem = new QuizModel();

        quizItem.setQuiz(childSnapshot.Child("quiz").Value.ToString());
        quizItem.setAnswer(childSnapshot.Child("answer").Value.ToString());
        quizItem.setChoices(childSnapshot.Child("choices").Value.ToString());
        quizList.Add(quizItem);
      }

      quizList.Shuffle();
      HandleSendQuizToAllPlayer();
    }
  }

  private void HandleSendQuizToAllPlayer()
  {
    if (!PhotonNetwork.IsMasterClient) return;

    string quiz = quizList[0].getQuiz() + "#" + quizList[0].getAnswer() + "#" + quizList[0].getChoices();

    ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable { { "Quiz", quiz } };
    PhotonNetwork.CurrentRoom.SetCustomProperties(props);
  }

  private void showQuiz()
  {
    string[] quizComponent = currentQuiz.Split('#');
    question.text = quizComponent[0];
    string[] choices = quizComponent[2].Split('/');

    if (choices.Length < 4) Debug.Log("Error: Not enough data!!!");
    else
    {
      for (int i = 0; i < choices.Length; i++)
        choicesTxt[i].text = choices[i];
    }
  }

  private void ResetQuizPopup()
  {
    foreach (var btn in choicesBtn)
    {
      btn.interactable = true;
      btn.GetComponent<Image>().color = Color.white;
    }

    wrongChoice = 0;
    hitPlayerActorNumber = -1;

    if (PhotonNetwork.IsMasterClient)
    {
      quizList.Remove(quizList[0]);
      HandleSendQuizToAllPlayer();
    }
  }

  [PunRPC]
  public void EndGame(int winnerActorNumber)
  {
    isGameOver = true;
    StartCoroutine(HandleEndGame(winnerActorNumber));
  }

  private IEnumerator HandleEndGame(int winnerActorNumber)
  {
    if (PhotonNetwork.LocalPlayer.ActorNumber == winnerActorNumber)
    {
      UserManager.instance.SetWin(UserManager.instance.GetWin() + 1);
      FirebaseManager.instance.UpdateUserWinCnt(UserManager.instance.GetWin());
      gameOverTxt.text = "You win!";
      gameOverTxt.color = Color.green;
    }
    else
    {
      gameOverTxt.text = "You lose";
      gameOverTxt.color = Color.red;
    }

    yield return new WaitForSeconds(1.25f);

    gameOverScreen.SetActive(true);

    yield return new WaitForSeconds(6f);

    PhotonNetwork.LoadLevel("RoomScene");
  }

  public override void OnPlayerLeftRoom(Player otherPlayer)
  {
    isGameOver = true;
    StartCoroutine(HandleEndGame(PhotonNetwork.LocalPlayer.ActorNumber));
  }
}
