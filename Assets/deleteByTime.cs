using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class deleteByTime : MonoBehaviourPun
{
    private void OnCollisionEnter(Collision collision)
    {
        if (this.photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }
}
