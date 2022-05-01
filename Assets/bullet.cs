using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class bullet : MonoBehaviourPunCallbacks
{
    public int damageMax, damage;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            if (this.photonView.IsMine) collision.collider.GetComponent<PhotonView>().RPC("GetDamage", RpcTarget.All, collision.collider.GetComponent<PhotonView>().ViewID, damage);
        }
    }
}
