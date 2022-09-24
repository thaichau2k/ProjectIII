using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  public static GameManager instance;

  private void Awake()
  {
    DontDestroyOnLoad(gameObject);

    if (instance == null) instance = this;
    else if (instance != this)
    {
      Destroy(instance.gameObject);
      instance = this;
    }
  }

  public void ChangeScene(string _sceneName)
  {
    SceneManager.LoadSceneAsync(_sceneName);
  }

  public static Color GetColor(int colorChoice)
  {
    switch (colorChoice)
    {
      case 0: return Color.red;
      case 1: return Color.green;
    }

    return Color.black;
  }
}
