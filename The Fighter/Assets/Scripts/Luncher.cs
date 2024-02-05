using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

public class Luncher : MonoBehaviourPunCallbacks
{
    public static Luncher instance;

    [SerializeField] GameObject RoomTestButton;

    [Header("Main Screen")]
    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject menuButtons;
    [SerializeField] GameObject errorScreen;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text loading_Text;

    [Header("Login")]
    [SerializeField] GameObject nameInputPanel;
    [SerializeField] TMP_InputField nameInputField;

    [Header("Room Screen")]
    [SerializeField] byte maxPlayers;
    [SerializeField] GameObject roomScreen;
    [SerializeField] GameObject createRoomScreen;
    [SerializeField] GameObject roomFindScreen;
    [SerializeField] GameObject StartButton;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text roomNameText, PlayerNameLable;
    [SerializeField] RoomButton theRoomButton;
    List<RoomButton> allRoomButtons = new List<RoomButton>();
    List<TMP_Text> allPlayersName = new List<TMP_Text>();

    [Space]
    [SerializeField] string[] gameSceanNames;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        CloseScreens();
        if (!PhotonNetwork.IsConnected)
        {
            loadingScreen.SetActive(true);
            loading_Text.text = "Connecting to Network....";
            PhotonNetwork.ConnectUsingSettings();
        }

#if UNITY_EDITOR
        RoomTestButton.SetActive(true);
#endif
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        loading_Text.text = "Joining Lobby....";
    }

    public override void OnJoinedLobby()
    {
        CloseScreens();
        loadingScreen.SetActive(false);
        menuButtons.SetActive(true);

        if (!PlayerPrefs.HasKey("PlayerName"))
        {
            CloseScreens();
            nameInputPanel.SetActive(true);
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
            CloseScreens();
            menuButtons.SetActive(true);
        }
    }

    void CloseScreens()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomFindScreen.SetActive(false);
        nameInputPanel.SetActive(false);
    }

    public void OpenRoomScreen()
    {
        CloseScreens();
        createRoomScreen.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInputField.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayers;
            PhotonNetwork.CreateRoom(roomNameInputField.text);

            CloseScreens();
            loading_Text.text = "Creating Room....";
            loadingScreen.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseScreens();
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        roomScreen.SetActive(true);

        ListAllPlayer();
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    void ListAllPlayer()
    {
        foreach (TMP_Text playerName in allPlayersName)
        {
            Destroy(playerName.gameObject);
        }
        allPlayersName.Clear();

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerText = Instantiate(PlayerNameLable, PlayerNameLable.transform.parent);
            newPlayerText.text = players[i].NickName;
            newPlayerText.gameObject.SetActive(true);
            allPlayersName.Add(newPlayerText);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerText = Instantiate(PlayerNameLable, PlayerNameLable.transform.parent);
        newPlayerText.text = newPlayer.NickName;
        newPlayerText.gameObject.SetActive(true);
        allPlayersName.Add(newPlayerText);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayer();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        loading_Text.text = "Leaveing Room...";
        loadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseScreens();
        menuButtons.SetActive(true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseScreens();
        errorText.text = "Fail to creating room : " + message + "try again.....";
        errorScreen.SetActive(true);
    }

    public void LeaveErrorScreen()
    {
        CloseScreens();
        menuButtons.SetActive(true);
    }

    public void OpenRoomBrowser()
    {
        CloseScreens();
        roomFindScreen.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseScreens();
        menuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton roomButton in allRoomButtons)
        {
            Destroy(roomButton.gameObject);
        }
        allRoomButtons.Clear();

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                newButton.setButtonInfo(roomList[i]);
                newButton.gameObject.SetActive(true);

                allRoomButtons.Add(newButton);
            }
        }
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);

        CloseScreens();
        loading_Text.text = "Joining Room...";
        loadingScreen.SetActive(true);
    }

    public void SetNickName()
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            PhotonNetwork.NickName = nameInputField.text + " #" + Random.Range(0, 1000).ToString();
            PlayerPrefs.SetString("PlayerName", PhotonNetwork.NickName);
            CloseScreens();
            menuButtons.SetActive(true);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(gameSceanNames[Random.Range(0, gameSceanNames.Length)]);
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    //For Testing 
    public void QuickJoin()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 7;
        PhotonNetwork.CreateRoom("Test", options);
        CloseScreens();
        loading_Text.text = "Creating Room...";
        loadingScreen.SetActive(true);
    }
}