using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderBoardPlayer : MonoBehaviour
{
    [SerializeField] TMP_Text playerName_Text, Kill_Text, Death_Text;

    public void SetDetails(string name, int kills, int deaths)
    {
        playerName_Text.text = name;
        Kill_Text.text = kills.ToString("00");
        Death_Text.text = deaths.ToString("00");
    }
}
