using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class UI_Controller : MonoBehaviour
{
    public static UI_Controller instance;

    public Slider tempSlider;
    public TMP_Text DeathMessage;
    public GameObject DeathScreen;
    public TMP_Text Health_text;
    public TMP_Text Kills_text;
    public TMP_Text Death_Text;
    public TMP_Text Ping_Text;
    public TMP_Text Timer_Text;
    public GameObject leaderBord;
    public LeaderBoardPlayer leaderBoardPlayerDisplay;
    public GameObject EndScreen;
    public GameObject OptionsScreen;


    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        OptionsScreen.SetActive(false);
    }
    void Update()
    {
        PingDisplay();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }
        if (OptionsScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ShowHideOptions()
    {
        if (!OptionsScreen.activeInHierarchy)
        {
            OptionsScreen.SetActive(true);
        }
        else
        {
            OptionsScreen.SetActive(false);
        }
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void PingDisplay()
    {
        if (PhotonNetwork.GetPing() > 100)
        {
            Ping_Text.color = Color.red;
        }
        else if (PhotonNetwork.GetPing() > 50)
        {
            Ping_Text.color = Color.yellow;
        }
        else
        {
            Ping_Text.color = Color.blue;
        }
        Ping_Text.text = "Ping: " + PhotonNetwork.GetPing().ToString();
    }
}