using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class bomb : MonoBehaviourPunCallbacks
{
    public int ownerId;

    private void OnCollisionEnter(Collision collision)
    {
        if (!this.photonView.IsMine) return;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5);
        foreach (Collider col in colliders)
        {
            if (col.GetComponent<Rigidbody>() != null)
            {
                if (col.tag == "Player") col.GetComponent<player>().GetComponent<PhotonView>().RPC("GetDamage", RpcTarget.All, col.GetComponent<PhotonView>().ViewID, 75/2, ownerId);
                col.GetComponent<PhotonView>().TransferOwnership(this.photonView.Owner);
                col.GetComponent<Rigidbody>().AddExplosionForce(5, transform.position, 5);
            }
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
