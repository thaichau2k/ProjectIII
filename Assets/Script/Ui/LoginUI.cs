using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Auth;

public class LoginUI : MonoBehaviour
{
  public static LoginUI instance;
  [SerializeField] private TMP_Text warningTxt;

  [Header("Login")]
  [SerializeField] private Button toRegisterBtn;
  [SerializeField] private GameObject loginUI;
  [SerializeField] private TMP_InputField email;
  [SerializeField] private TMP_InputField pass;
  [SerializeField] private Button loginBtn;

  [Header("Register")]
  [SerializeField] private Button backBtn;
  [SerializeField] private GameObject registerUI;
  [SerializeField] private TMP_InputField emailRegister;
  [SerializeField] private TMP_InputField passRegister;
  [SerializeField] private TMP_InputField username;
  [SerializeField] private TMP_InputField passVerify;
  [SerializeField] private Button registerBtn;

  private void Awake()
  {
    if (instance == null) instance = this;
    else if (instance != this)
    {
      Destroy(instance.gameObject);
      instance = this;
    }
  }

  private void ClearUi()
  {
    loginUI.SetActive(false);
    registerUI.SetActive(false);
    ClearOutput();
  }

  public void LoginScreen()
  {
    ClearUi();
    loginUI.SetActive(true);
  }

  public void RegisterScreen()
  {
    ClearUi();
    registerUI.SetActive(true);
  }

  private void Start()
  {
    LoginScreen();
    RegisterEvent();
  }

  private void RegisterEvent()
  {
    toRegisterBtn.onClick.AddListener(() => { RegisterScreen(); });
    backBtn.onClick.AddListener(() => { LoginScreen(); });
    loginBtn.onClick.AddListener(() =>
    {
      StartCoroutine(Login(email.text, pass.text));
    });

    registerBtn.onClick.AddListener(() =>
    {
      StartCoroutine(Register(emailRegister.text, passRegister.text, username.text));
    });
  }

  public void ClearOutput()
  {
    warningTxt.text = "";
  }

  private IEnumerator Login(string _email, string _password)
  {
    Credential credential = EmailAuthProvider.GetCredential(_email, _password);

    var loginTask = FirebaseManager.instance._auth.SignInWithCredentialAsync(credential);

    yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

    if (loginTask.Exception != null)
    {
      //Error Handle
      FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
      AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

      string message = "Login Failed!";
      switch (errorCode)
      {
        case AuthError.MissingEmail:
          message = "Missing Email";
          break;
        case AuthError.MissingPassword:
          message = "Missing Password";
          break;
        case AuthError.WrongPassword:
          message = "Wrong Password";
          break;
        case AuthError.InvalidEmail:
          message = "Invalid Email";
          break;
        case AuthError.UserNotFound:
          message = "Account does not exist";
          break;
      }
      warningTxt.text = message;
    }
    else
    {
      FirebaseManager.instance._user = loginTask.Result;
      Debug.LogFormat("User signed in successfully: {0} ({1})", FirebaseManager.instance._user.DisplayName, FirebaseManager.instance._user.Email);
      warningTxt.text = "";

      //Change Scene here!
      StartCoroutine(UserManager.instance.GetUserData());

      yield return new WaitForSeconds(0.5f);

      NetworkManager.instance.Connect(FirebaseManager.instance._user.DisplayName);
    }
  }

  private IEnumerator Register(string _email, string _password, string _username)
  {
    if (_username == "")
      warningTxt.text = "Missing Username";
    else if (passRegister.text != passVerify.text)
      warningTxt.text = "Password Does Not Match!";
    else
    {
      var RegisterTask = FirebaseManager.instance._auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

      yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

      if (RegisterTask.Exception != null)
      {
        //Error Handle
        FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        string message = "Register Failed!";
        switch (errorCode)
        {
          case AuthError.MissingEmail:
            message = "Missing Email";
            break;
          case AuthError.MissingPassword:
            message = "Missing Password";
            break;
          case AuthError.WeakPassword:
            message = "Weak Password";
            break;
          case AuthError.EmailAlreadyInUse:
            message = "Email Already In Use";
            break;
          case AuthError.InvalidEmail:
            message = "Invalid Email";
            break;
        }
        warningTxt.text = message;
      }
      else
      {
        FirebaseManager.instance._user = RegisterTask.Result;

        if (FirebaseManager.instance._user != null)
        {
          UserProfile profile = new UserProfile { DisplayName = _username };

          var ProfileTask = FirebaseManager.instance._user.UpdateUserProfileAsync(profile);

          yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

          if (ProfileTask.Exception != null)
          {
            FirebaseManager.instance._user.DeleteAsync();
            //Error Handle
            FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
          }
          else
          {
            //username updated!
            FirebaseManager.instance.UpdateUsername(_username, true);
            FirebaseManager.instance.UpdateUserWinCnt(0);
            FirebaseManager.instance.UpdateUserTotalGames(0);
            StartCoroutine(UserManager.instance.GetUserData());

            yield return new WaitForSeconds(0.5f);

            NetworkManager.instance.Connect(FirebaseManager.instance._user.DisplayName);
          }
        }
      }
    }
  }
}
