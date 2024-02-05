using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Match_Manager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static Match_Manager instance;
    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayer,
        UpdateState
    }

    [SerializeField] List<PlayerInfo> allPlayerInfos = new List<PlayerInfo>();
    int index;
    List<LeaderBoardPlayer> lBoardPlayers = new List<LeaderBoardPlayer>();

    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    public int KillsToWin = 10;
    public Transform mapCameraPoint;
    public GameState state = GameState.Waiting;
    public float waitAfterEnding = 5f;
    public float matchLength = 300f;
    float currentMatchTime;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
            state = GameState.Playing;
            SetUpTimer();
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Tab) || state == GameState.Ending)
        {
            ShowLeaderBord();
        }
        else
        {
            UI_Controller.instance.leaderBord.SetActive(false);
        }

        if (currentMatchTime > 0 && state == GameState.Playing)
        {
            currentMatchTime -= Time.deltaTime;
            if (currentMatchTime <= 0)
            {
                currentMatchTime = 0;
                state = GameState.Ending;
                ListPlayerSend();
                StateCheck();
            }
            UpdateTimerDisplay();
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;
            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayer:
                    ListPlayerReceive(data);
                    break;
                case EventCodes.UpdateState:
                    UpdateStateReceive(data);
                    break;
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string userName)
    {
        object[] package = new object[4];
        package[0] = userName;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;


        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }
    public void NewPlayerReceive(object[] dataReceived)
    {
        PlayerInfo player = new PlayerInfo((string)dataReceived[0], (int)dataReceived[1], (int)dataReceived[2], (int)dataReceived[3]);
        allPlayerInfos.Add(player);
        ListPlayerSend();
    }

    public void ListPlayerSend()
    {
        object[] package = new object[allPlayerInfos.Count + 1];

        package[0] = state;

        for (int i = 0; i < allPlayerInfos.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = allPlayerInfos[i].name;
            piece[1] = allPlayerInfos[i].actor;
            piece[2] = allPlayerInfos[i].kill;
            piece[3] = allPlayerInfos[i].death;

            package[i + 1] = piece;
        }
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }
    public void ListPlayerReceive(object[] dataRecevied)
    {
        allPlayerInfos.Clear();
        state = (GameState)dataRecevied[0];

        for (int i = 1; i < dataRecevied.Length; i++)
        {
            object[] piece = (object[])dataRecevied[i];

            PlayerInfo player = new PlayerInfo((string)piece[0], (int)piece[1], (int)piece[2], (int)piece[3]);
            allPlayerInfos.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i - 1;
            }
        }
        StateCheck();
    }

    public void UpdateStateSend(int actorSending, int stateToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorSending, stateToUpdate, amountToChange };
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateState,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }
    public void UpdateStateReceive(object[] dataRecevied)
    {
        int actor = (int)dataRecevied[0];
        int stateType = (int)dataRecevied[1];
        int amount = (int)dataRecevied[2];

        for (int i = 0; i < dataRecevied.Length; i++)
        {
            if (allPlayerInfos[i].actor == actor)
            {
                switch (stateType)
                {
                    case 0:           //Kills
                        allPlayerInfos[i].kill += amount;
                        break;
                    case 1:          //Death
                        allPlayerInfos[i].death += amount;
                        break;
                }

                if (i == index)
                {
                    UpdateStateDesplay();
                }
                if (UI_Controller.instance.leaderBord.activeInHierarchy)
                {
                    ShowLeaderBord();
                }
                break;
            }
        }
        ScoreCheck();
    }

    void UpdateStateDesplay()
    {
        if (allPlayerInfos.Count > index)
        {
            UI_Controller.instance.Kills_text.text = "Kills : " + allPlayerInfos[index].kill;
            UI_Controller.instance.Death_Text.text = "Deaths : " + allPlayerInfos[index].death;
        }
        else
        {
            UI_Controller.instance.Kills_text.text = "Kills : 0";
            UI_Controller.instance.Death_Text.text = "Deaths : 0";
        }
    }

    void ShowLeaderBord()
    {
        UI_Controller.instance.leaderBord.SetActive(true);

        foreach (LeaderBoardPlayer lp in lBoardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lBoardPlayers.Clear();

        UI_Controller.instance.leaderBoardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = sortPlayers(allPlayerInfos);

        foreach (PlayerInfo player in sorted)
        {
            LeaderBoardPlayer newPlayerDisplay = Instantiate(UI_Controller.instance.leaderBoardPlayerDisplay, UI_Controller.instance.leaderBoardPlayerDisplay.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kill, player.death);
            newPlayerDisplay.gameObject.SetActive(true);
            lBoardPlayers.Add(newPlayerDisplay);
        }
    }

    List<PlayerInfo> sortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();
        while (sorted.Count < players.Count)
        {
            int highest = -1;
            PlayerInfo selectedPlayer = players[0];
            foreach (PlayerInfo player in players)
            {
                if (!sorted.Contains(player))
                {
                    if (player.kill > highest)
                    {
                        selectedPlayer = player;
                        highest = player.kill;
                    }
                }
            }
            sorted.Add(selectedPlayer);
        }
        return sorted;
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    void ScoreCheck()
    {
        bool winnerFound = false;
        foreach (PlayerInfo player in allPlayerInfos)
        {
            if (player.kill >= KillsToWin && KillsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayerSend();
            }
        }
    }

    void StateCheck()
    {
        if (state == GameState.Ending)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        state = GameState.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        UI_Controller.instance.EndScreen.SetActive(true);
        ShowLeaderBord();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = mapCameraPoint.position;
        Camera.main.transform.rotation = mapCameraPoint.rotation;

        StartCoroutine(EndCo());
    }

    private IEnumerator EndCo()
    {
        yield return new WaitForSeconds(waitAfterEnding);
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void SetUpTimer()
    {
        if (matchLength > 0)
        {
            currentMatchTime = matchLength;
            UpdateTimerDisplay();
        }
    }

    public void UpdateTimerDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);

        UI_Controller.instance.Timer_Text.text = timeToDisplay.Minutes.ToString("00") + ":" + timeToDisplay.Seconds.ToString("00");
    }
}

[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kill, death;

    public PlayerInfo(string _name, int _actor, int _kill, int _death)
    {
        name = _name;
        actor = _actor;
        kill = _kill;
        death = _death;
    }
}