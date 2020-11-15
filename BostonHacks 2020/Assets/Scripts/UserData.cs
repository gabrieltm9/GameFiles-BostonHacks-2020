using System;

[Serializable]
public class UserData 
{
    public UserData() { }

    public string id;

    public string email;
    public string name;

    public int energyScore = 0;
    public int waterScore = 0;
    public int wasteScore = 0;
    public int totalScore = 0;
}
