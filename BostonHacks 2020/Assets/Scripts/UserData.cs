using System;

[Serializable]
public class UserData 
{
    public UserData() { }

    public string id;

    public string email;
    public string name;

    public int energyScore;
    public int waterScore;
    public int wasteScore;
    public int totalScore;
}
