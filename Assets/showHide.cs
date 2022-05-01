using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showHide : MonoBehaviour
{
    public GameObject[] show, hide;

    public void Pressed()
    {
        foreach (GameObject obj in show) obj.SetActive(true);
        foreach (GameObject obj in hide) obj.SetActive(false);
    }
}
