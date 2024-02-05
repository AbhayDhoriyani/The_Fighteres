using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    [SerializeField] TMP_Text buttonText;

    RoomInfo info;

    public void setButtonInfo(RoomInfo inputInfo)
    {
        info = inputInfo;
        buttonText.text = info.Name;
    }

    public void OpenRoom()
    {
        Luncher.instance.JoinRoom(info);
    }
}
