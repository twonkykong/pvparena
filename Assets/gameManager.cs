using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class gameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab, eventObj;

    private void Start()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 10, 0), Quaternion.identity);

        if (this.photonView.IsMine)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("propSpawner"))
            {
                obj.GetComponent<PhotonView>().TransferOwnership(this.photonView.Owner);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        eventObj.GetComponentInChildren<Text>().text = newPlayer.NickName + " joined room";
        eventObj.GetComponent<Animation>().Stop();
        eventObj.GetComponent<Animation>().Play();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        eventObj.GetComponentInChildren<Text>().text = otherPlayer.NickName + " left room";
        eventObj.GetComponent<Animation>().Stop();
        eventObj.GetComponent<Animation>().Play();
    }

    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("123");
        SceneManager.LoadScene("menu");
    }
}
