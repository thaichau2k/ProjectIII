using System;

[Serializable]
public class QuizModel
{
  string _quiz;
  string _answer;
  string _choices;

  public QuizModel() { }

  public QuizModel(string quiz, string answer, string choices)
  {
    this._quiz = quiz;
    this._answer = answer;
    this._choices = choices;
  }

  public string getQuiz()
  {
    return _quiz;
  }

  public void setQuiz(string quiz)
  {
    this._quiz = quiz;
  }

  public string getAnswer()
  {
    return _answer;
  }

  public void setAnswer(string answer)
  {
    this._answer = answer;
  }

  public string getChoices()
  {
    return _choices;
  }

  public void setChoices(string choices)
  {
    this._choices = choices;
  }
}
