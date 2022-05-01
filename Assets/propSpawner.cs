using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Photon.Pun;

public class propSpawner : MonoBehaviourPunCallbacks
{
    public GameObject prop, prefab;
    public float timer;

    private void FixedUpdate()
    {
        if (!this.photonView.IsMine) return;
        if (prop == null)
        {
            if (timer == 0)
            {
                timer = 0.1f;
            }
        }

        if (timer != 0)
        {
            timer += 0.1f;
            if (timer > 15)
            {
                prop = PhotonNetwork.Instantiate(prefab.name, transform.position + Vector3.up, Quaternion.identity);
                if (prop.tag == "weapon") prop.GetComponent<weaponProp>().spawner = gameObject;
                timer = 0;
            }
        }
    }
}
