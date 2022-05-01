using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrow : MonoBehaviour
{
    private void FixedUpdate()
    {
        GetComponent<Rigidbody>().rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity/2);
    }
}
