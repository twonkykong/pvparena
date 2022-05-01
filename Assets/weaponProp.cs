using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class weaponProp : MonoBehaviourPunCallbacks
{
    public GameObject spawner;
    float timer;

    private void OnDisable()
    {
        timer = 0.1f;
    }

    private void FixedUpdate()
    {
        if (timer != 0)
        {
            timer += 0.1f;
            if (timer > 50)
            {
                if (this.photonView.IsMine) PhotonView.Destroy(gameObject);
            }
        }
    }
}
