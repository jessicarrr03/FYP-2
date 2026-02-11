using System;

[Serializable]
public class Question
{
    public string QuestionId;
    public string Stage;
    public string Text;
    public string A, B, C, D;
    public char Correct;
    public string ImageKey;
}
