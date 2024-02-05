using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player_Spawner : MonoBehaviour
{
    public static Player_Spawner instance;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject DeathEffact;
    [SerializeField] float TimeToRespawnPlayer = 5;
    GameObject player;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = Spawn_Manager.instance.GetSpawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public void Die(string demager)
    {
        UI_Controller.instance.DeathMessage.text = "You Killed by " + demager;

        Match_Manager.instance.UpdateStateSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);//Give Death

        if (player != null)
        {
            StartCoroutine(DieCo());
        }
    }

    IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(DeathEffact.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        UI_Controller.instance.DeathScreen.SetActive(true);

        yield return new WaitForSeconds(TimeToRespawnPlayer);

        UI_Controller.instance.DeathScreen.SetActive(false);
        SpawnPlayer();
    }
}
