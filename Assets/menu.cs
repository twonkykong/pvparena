using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class menu : MonoBehaviourPunCallbacks
{
    public Text debug, coins;
    public GameObject connecting, buttons;

    private void Update()
    {
        debug.text = "region: " + PhotonNetwork.CloudRegion + "\nnickname: " + PhotonNetwork.NickName + "\nplayers: " + PhotonNetwork.CountOfPlayers;
        coins.text = "coins: " + PlayerPrefs.GetInt("coins");
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PhotonNetwork.NickName = "Player " + Random.Range(1, 101);
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Play()
    {
        if (PhotonNetwork.CountOfRooms > 0) PhotonNetwork.JoinRandomRoom();
        else PhotonNetwork.CreateRoom("Room " + PhotonNetwork.CountOfRooms + 1, new Photon.Realtime.RoomOptions { MaxPlayers = 99, IsVisible = true });
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("created");
        PhotonNetwork.LoadLevel("game");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("joined room " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnConnected()
    {
        Debug.Log("connected " + PhotonNetwork.NickName);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("joined lobby");
        connecting.SetActive(false);
        buttons.SetActive(true);
    }

    public void ChangeNickname(InputField nickname)
    {
        if (nickname.text == "") return;
        PhotonNetwork.NickName = nickname.text;
        nickname.text = "";
    }

    public void Buy(string item)
    {
        int cost = 50;
        if (item == "speed") cost = 100;
        else if (item == "damage") cost = 150;
        if (PlayerPrefs.GetInt("coins") >= cost)
        {
            PlayerPrefs.SetInt(item, PlayerPrefs.GetInt(item) + 1);
            PlayerPrefs.SetInt("coins", PlayerPrefs.GetInt("coins") - cost);
        }
    }
}
