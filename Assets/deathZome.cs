using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class deathZome : MonoBehaviourPunCallbacks
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") other.GetComponent<player>().GetComponent<PhotonView>().RPC("GetDamage", RpcTarget.All, other.GetComponent<PhotonView>().ViewID, 200, -10);
        else
        {
            other.GetComponent<PhotonView>().TransferOwnership(this.photonView.Owner);
            PhotonView.Destroy(other);
        }
    }
}
