using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bowRope : MonoBehaviour
{
    public GameObject[] rope;

    private void FixedUpdate()
    {
        GetComponent<LineRenderer>().SetPositions(new Vector3[3] { rope[0].transform.position, rope[1].transform.position, rope[2].transform.position });
    }
}
