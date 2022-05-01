using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon.StructWrapping;

public class weapon : MonoBehaviourPunCallbacks
{
    public bool shoot, holdToShoot, _throw, holdHit;
    bool holding;
    public GameObject bullet, shootPower, cam, handsCam, parent;
    public float distance = 4f, timer, shootTimer, hitTimer;
    public int damageMultiplier = 1, damage;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (timer == 0)
            {
                if (shoot == false)
                {
                    if (holdHit)
                    {
                        holding = true;
                        GetComponent<Animator>().SetBool("hit", true);
                        cam.GetComponent<Animator>().SetBool("zoom", true);
                        handsCam.GetComponent<Animator>().SetBool("zoom", true);
                    }
                    else if (holdToShoot)
                    {
                        shootPower.SetActive(true);
                        shootTimer = 0.1f;
                        GetComponent<Animator>().SetBool("zoom", true);
                        cam.GetComponent<Animator>().SetBool("zoom", true);
                        handsCam.GetComponent<Animator>().SetBool("zoom", true);

                    }
                    else
                    {
                        GetComponent<Animator>().SetBool("hit" + Random.Range(1, 3), true);
                        hitTimer = 0.1f;
                        RaycastHit hit;
                        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, distance))
                        {
                            GameObject prop = hit.collider.gameObject;
                            if (prop.GetComponent<PhotonView>() != null && prop.GetComponent<Rigidbody>() != null)
                            {
                                if (prop.tag == "Player")
                                {
                                    parent.GetComponent<player>().GetComponent<PhotonView>().RPC("GetDamage", RpcTarget.All, prop.GetComponent<PhotonView>().ViewID, damage * damageMultiplier, parent.GetComponent<PhotonView>().ViewID);
                                }
                                else
                                {
                                    while (prop.transform.parent != null) prop = prop.transform.parent.gameObject;
                                    prop.GetComponent<PhotonView>().TransferOwnership(this.photonView.Owner);
                                    if (prop.GetComponent<Rigidbody>() != null) prop.GetComponent<Rigidbody>().AddForce(cam.transform.forward * 10f, ForceMode.Impulse);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (holdToShoot)
                    {
                        shootPower.SetActive(true);
                        GetComponent<Animator>().SetBool("zoom", true);
                        cam.GetComponent<Animator>().SetBool("zoom", true);
                        handsCam.GetComponent<Animator>().SetBool("zoom", true);
                        shootTimer = 0.1f;
                    }
                }
                if (!holdToShoot) timer = 0.1f;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (holdHit)
            {
                holding = false;
                GetComponent<Animator>().SetBool("hit", false);
                GetComponent<Animator>().SetBool("zoom", false);
                cam.GetComponent<Animator>().SetBool("zoom", false);
            }

            if (shootTimer != 0)
            {
                shootPower.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 12);
                shootPower.SetActive(false);
                GetComponent<Animator>().SetBool("zoom", false);
                cam.GetComponent<Animator>().SetBool("zoom", false);
                handsCam.GetComponent<Animator>().SetBool("zoom", false);

                if (shoot == false)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, distance))
                    {
                        GameObject prop = hit.collider.gameObject;
                        if (prop.GetComponent<PhotonView>() != null && prop.GetComponent<Rigidbody>() != null)
                        {
                            if (prop.tag == "Player")
                            {
                                parent.GetComponent<player>().GetComponent<PhotonView>().RPC("GetDamage", RpcTarget.All, prop.GetComponent<PhotonView>().ViewID, Mathf.RoundToInt(shootTimer / 5 * 40) * damageMultiplier, parent.GetComponent<PhotonView>().ViewID);
                            }
                            else
                            {
                                while (prop.transform.parent != null) prop = prop.transform.parent.gameObject;
                                prop.GetComponent<PhotonView>().TransferOwnership(this.photonView.Owner);
                                if (prop.GetComponent<Rigidbody>() != null) prop.GetComponent<Rigidbody>().AddForce(cam.transform.forward * shootTimer / 5 * 10, ForceMode.Impulse);
                            }
                        }
                    }
                }
                else
                {
                    GameObject g = PhotonNetwork.Instantiate(bullet.name, cam.transform.position + cam.transform.forward, cam.transform.rotation);
                    g.GetComponent<Rigidbody>().AddForce(cam.transform.forward * shootTimer * 6, ForceMode.Impulse);
                    if (g.GetComponent<bullet>() != null) g.GetComponent<bullet>().damage = Mathf.RoundToInt(g.GetComponent<bullet>().damageMax / 5 * shootTimer * damageMultiplier);

                    if (_throw)
                    {
                        if (g.GetComponent<bomb>() != null) g.GetComponent<bomb>().ownerId = parent.GetComponent<PhotonView>().ViewID;
                        parent.GetComponent<player>().GetComponent<PhotonView>().RPC("ShowHide", RpcTarget.AllBuffered, this.photonView.ViewID, false);
                        PhotonNetwork.Destroy(parent.GetComponent<player>().holdingWeapon);
                    }
                }

                shootTimer = 0;
                timer = 0.1f;
            }
        }

        if (holding)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, distance))
            {
                if (hit.collider.tag == "Player")
                {
                    parent.GetComponent<player>().GetComponent<PhotonView>().RPC("GetDamage", RpcTarget.All, hit.collider.GetComponent<PhotonView>().ViewID, 2 * damageMultiplier, parent.GetComponent<PhotonView>().ViewID);
                }
                
            }
        }
    }

    private void FixedUpdate()
    {
        if (timer != 0)
        {
            timer += 0.1f;
            if (timer >= 2.5f)
            {
                timer = 0;
            }
        }

        if (shootTimer != 0 && shootTimer < 5)
        {
            shootTimer += 0.1f;
            shootPower.GetComponent<RectTransform>().sizeDelta = new Vector2(shootTimer / 5 * 100, 12);
        }

        if (hitTimer != 0)
        {
            hitTimer += 0.1f;
            if (hitTimer >= 0.2f)
            {
                GetComponent<Animator>().SetBool("hit1", false);
                GetComponent<Animator>().SetBool("hit2", false);
                hitTimer = 0;
            }
        }
    }
}
