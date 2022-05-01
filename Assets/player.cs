using ExitGames.Client.Photon.StructWrapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Reflection;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun.Demo.Cockpit;
using System.Linq;

public class player : MonoBehaviourPunCallbacks, IPunObservable
{
    public float speed, mousex, mousey, sensitivity = 1, jumpForce, afkTimer, extraTimerEnd = 35f;
    public float maxVelocityChange = 10.0f;
    public Rigidbody rb;
    public Animator anim, camAnim, handsCamAnim, hpVignette;

    public GameObject weaponProp, holdingWeapon, head, cam, hands, handsCam, body, canvas, hpBar, shieldBar, gameManager, eventObj, headHp, coinsText;
    public Text hpText, shieldText, topplayers, topplayer;
    public int hp, shield, lastHit, kills;
    public GameObject[] weapons, spawns, players, extrasKeys;

    public string[] extras = new string[3] { "", "", "" };

    string currentExtra;
    float hpTimer, extraTimer, speedMultiplier = 1;

    private void Start()
    {
        if (!this.photonView.IsMine)
        {
            canvas.SetActive(false);
            cam.SetActive(false);
            handsCam.SetActive(false);
            body.layer = 0;
            hands.layer = 0;
            headHp.layer = 0;
            foreach (Transform child in hands.GetComponentInChildren<Transform>())
            {
                child.gameObject.layer = 0;
                foreach (Transform child1 in child.GetComponentInChildren<Transform>())
                {
                    child1.gameObject.layer = 0;
                }
            }
            foreach (GameObject weapon in weapons)
            {
                weapon.GetComponent<weapon>().enabled = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            gameManager = GameObject.Find("gameManager");
            gameManager.GetComponent<gameManager>().eventObj = eventObj;
            spawns = GameObject.FindGameObjectsWithTag("Respawn");
        }
    }

    void Update()
    {
        if (!this.photonView.IsMine) return;
        //head rot
        mousex += Input.GetAxis("Mouse X") * sensitivity;
        mousey += Input.GetAxis("Mouse Y") * sensitivity;

        if (mousex > 359) mousex = 0;
        else if (mousex < 0) mousex = 359;
        if (mousey > 90) mousey = 90;
        else if (mousey < -90) mousey = -90;

        head.transform.localRotation = Quaternion.Euler(-mousey, 0, 0);
        transform.rotation = Quaternion.Euler(0, mousex, 0);

        //moving
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) anim.SetBool("walk", true);
        else anim.SetBool("walk", false);

        if (camAnim.GetBool("zoom") == true)
        {
            camAnim.SetBool("walk", false);
            handsCamAnim.SetBool("walk", false);
            speed = 2 * speedMultiplier;
        }
        else
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                camAnim.SetBool("walk", true);
                handsCamAnim.SetBool("walk", true);
                speed = 5 * speedMultiplier;
            }
            else
            {
                camAnim.SetBool("walk", false);
                handsCamAnim.SetBool("walk", false);
                speed = 4 * speedMultiplier;
            }
        }

        Vector3 pos = transform.right * (Input.GetAxis("Horizontal") * speed) + transform.forward * (Input.GetAxis("Vertical") * speed);
        Vector3 newPos = new Vector3(pos.x, rb.velocity.y, pos.z);
        rb.velocity = newPos;

        if (Physics.Raycast(transform.position, -transform.up, 0.2f))
        {
            anim.SetBool("jump", false);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
        }
        else anim.SetBool("jump", true);

        anim.speed = speed / 5;

        //fight
        Debug.DrawRay(cam.transform.position, cam.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 4f))
        {
            if (hit.collider.tag == "weapon")
            {
                if (hit.collider.transform.parent != null) weaponProp = hit.collider.transform.parent.gameObject;
                else weaponProp = hit.collider.transform.gameObject;
                weaponProp.GetComponent<Outline>().enabled = true;
                if (Input.GetKeyDown(KeyCode.E) && camAnim.GetBool("zoom") == false)
                {
                    DropWeapon();

                    holdingWeapon = weaponProp;
                    holdingWeapon.GetComponent<PhotonView>().TransferOwnership(this.photonView.Owner);
                    holdingWeapon.transform.parent = transform;
                    holdingWeapon.transform.position = transform.position;
                    this.photonView.RPC("ShowHide", RpcTarget.AllBuffered, holdingWeapon.GetComponent<PhotonView>().ViewID, false);
                    if (holdingWeapon.GetComponent<weaponProp>().spawner != null) this.photonView.RPC("DeleteSpawnerProp", RpcTarget.AllBuffered, holdingWeapon.GetComponent<weaponProp>().spawner.GetComponent<PhotonView>().ViewID, holdingWeapon.GetComponent<PhotonView>().ViewID);

                    foreach (GameObject weapon in weapons)
                    {
                        bool value = false;
                        if (weapon.name == holdingWeapon.name.Split('(')[0].Split(' ')[0]) value = true;
                        this.photonView.RPC("ShowHide", RpcTarget.AllBuffered, weapon.GetComponent<PhotonView>().ViewID, value);
                    }
                }
            }
            else
            {
                if (weaponProp != null) weaponProp.GetComponent<Outline>().enabled = false;
                weaponProp = null;
            }
        }
        else
        {
            if (weaponProp != null) weaponProp.GetComponent<Outline>().enabled = false;
            weaponProp = null;
        }

        if (Input.GetKeyDown(KeyCode.G) && camAnim.GetBool("zoom") == false)
        {
            DropWeapon();
        }

        //extras
        if (extraTimer == 0)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (PlayerPrefs.GetInt("speedExtra") > 0)
                {
                    currentExtra = "speed";
                    extraTimer = 0.1f;
                }
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                if (PlayerPrefs.GetInt("jumpExtra") > 0)
                {
                    currentExtra = "jump";
                    extraTimer = 0.1f;
                }
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (PlayerPrefs.GetInt("damageExtra") > 0)
                {
                    currentExtra = "damage";
                    extraTimer = 0.1f;
                }
            }
        }

        if (PlayerPrefs.GetInt("speedExtra") > 0) extrasKeys[0].SetActive(true);
        else extrasKeys[0].SetActive(false);

        if (PlayerPrefs.GetInt("jumpExtra") > 0) extrasKeys[1].SetActive(true);
        else extrasKeys[1].SetActive(false);

        if (PlayerPrefs.GetInt("damageExtra") > 0) extrasKeys[2].SetActive(true);
        else extrasKeys[2].SetActive(false);

        //hp
        hpText.text = "" + hp;
        shieldText.text = "" + shield;
        hpBar.GetComponent<RectTransform>().sizeDelta = new Vector2(hp, 20);
        shieldBar.GetComponent<RectTransform>().sizeDelta = new Vector2(shield, 20);

        if (hp <= 25) hpVignette.SetBool("pulsing", true);
        else hpVignette.SetBool("pulsing", false);

        if (hp <= 0)
        {
            DropWeapon();
            transform.position = spawns[Random.Range(0, spawns.Length)].transform.position;
            hp = 100;
            shield = 25;
            this.photonView.RPC("AddKill", RpcTarget.All, lastHit);
        }

        if (hp > 100) hp = 100;
        if (shield > 100) shield = 100;

        //top
        //sorting players
        players = GameObject.FindGameObjectsWithTag("Player");

        //sort by alphabetical order
        bool needSort1 = true;
        for (int i = 0; (i < players.Length) && needSort1; i++)
        {
            needSort1 = false;
            for (int j = 0; j < players.Length - 1; j++)
            {
                if (string.Compare(players[j + 1].GetComponent<PhotonView>().Owner.NickName, players[j].GetComponent<PhotonView>().Owner.NickName) == 1)
                {
                    GameObject t = players[j];
                    players[j] = players[j + 1];
                    players[j + 1] = t;
                    needSort1 = true;
                }
            }
        }

        //sort by kills
        bool needSort = true;
        for (int i = 0; (i < players.Length) && needSort; i++)
        {
            needSort = false;
            for (int j = 0; j < players.Length - 1; j++)
            {
                if (players[j + 1].GetComponent<player>().kills > players[j].GetComponent<player>().kills)
                {
                    GameObject t = players[j];
                    players[j] = players[j + 1];
                    players[j + 1] = t;
                    needSort = true;
                }
            }
        }

        topplayers.text = "";
        for (int i = 0; i < Mathf.Clamp(players.Length, 0, 10); i++)
        {
            string nickname = players[i].GetComponent<PhotonView>().Owner.NickName;
            if (players[i] == gameObject) nickname = "<color=#762424>" + nickname + "</color>";
            topplayers.text += nickname + " (" + players[i].GetComponent<player>().kills + ")\n";
        }

        string name = players[0].GetComponent<PhotonView>().Owner.NickName;
        if (players[0] == gameObject) name = "<color=#762424>" + name + "</color>";
        topplayer.text = "BEST KILLER: " + name + " (" + players[0].GetComponent<player>().kills + ")";

        foreach (GameObject player in players)
        {
            //hp
            player.GetComponentInChildren<TextMeshPro>().text = "hp: " + player.GetComponent<player>().hp + "\nshield: " + player.GetComponent<player>().shield + "\nkills: " + player.GetComponent<player>().kills + "\n" + player.GetComponent<PhotonView>().Owner.NickName;
            player.GetComponentInChildren<TextMeshPro>().gameObject.transform.LookAt(cam.transform.position);
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0 && Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0 && Input.GetKey(KeyCode.Space) == false)
        {
            afkTimer += 0.1f;
            if (afkTimer >= 600)
            {
                gameManager.GetComponent<gameManager>().Leave();
                afkTimer = 0;
            }
        }
        else afkTimer = 0;

        hpTimer += 0.1f;
        if (hpTimer >= 5)
        {
            if (hp < 25) hp += 4;
            else if (hp < 50) hp += 3;
            else if (hp < 75) hp += 2;
            else if (hp < 101) hp += 1;

            hpTimer = 0;
        }

        //extras 
        if (extraTimer != 0)
        {
            if (extraTimer == 0.1f)
            {
                if (currentExtra == "speed") speedMultiplier = 2;
                else if (currentExtra == "jump") jumpForce = 8;
                else if (currentExtra == "damage") foreach (GameObject weapon in weapons) weapon.GetComponent<weapon>().damageMultiplier = 2;
            }

            extraTimer += 0.1f;
            if (extraTimer >= extraTimerEnd)
            {
                extraTimer = 0;
                speedMultiplier = 1;
                jumpForce = 5;
                foreach (GameObject weapon in weapons) weapon.GetComponent<weapon>().damageMultiplier = 1;
            }
        }
    }

    [PunRPC]
    public void ShowHide(int id, bool value)
    {
        GameObject obj = PhotonView.Find(id).gameObject;
        obj.SetActive(value);
    }

    [PunRPC]
    public void DeleteSpawnerProp(int idSpawner, int id)
    {
        GameObject spawner = PhotonView.Find(idSpawner).gameObject;
        spawner.GetComponent<propSpawner>().prop = null;

        GameObject obj = PhotonView.Find(id).gameObject;
        obj.GetComponent<weaponProp>().spawner = null;
    }

    [PunRPC]
    public void AddKill(int id)
    {
        int killsOld = kills;
        Debug.Log(kills);
        if (id != 0 || id != -10) PhotonView.Find(id).gameObject.GetComponent<player>().kills += 1;
        Debug.Log(PlayerPrefs.GetInt("coins"));
        if (killsOld != kills)
        {
            int plusCoins = Random.Range(1, 5) * 5;
            PlayerPrefs.SetInt("coins", PlayerPrefs.GetInt("coins") + plusCoins);
            Debug.Log(this.photonView.Owner.NickName);
        }
        Debug.Log(PlayerPrefs.GetInt("coins"));
        Debug.Log(kills);
    }

    [PunRPC]
    public void GetDamage(int id, int damage, int lastHitId)
    {
        GameObject player = PhotonView.Find(id).gameObject;
        if (player.GetComponent<player>().shield > 0)
        {
            if (player.GetComponent<player>().shield > damage / 2)
            {
                player.GetComponent<player>().shield -= damage / 2;
                player.GetComponent<player>().hp -= damage / 2;
            }
            else
            {
                int shieldDamage = damage / 2 - (damage / 2 - player.GetComponent<player>().shield);
                player.GetComponent<player>().shield -= shieldDamage;
                player.GetComponent<player>().hp -= (damage - shieldDamage);
            }
        }
        else player.GetComponent<player>().hp -= damage;

        player.GetComponent<player>().lastHit = lastHitId;
        
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "hp")
        {
            if (hp < 100)
            {
                hp += 50;
                PhotonNetwork.Destroy(col.gameObject);
            }
        }
        else if (col.tag == "shield")
        {
            if (shield < 100)
            {
                shield += 25;
                PhotonNetwork.Destroy(col.gameObject);
            }
        }
        else if (col.tag == "arrow")
        {
            GetDamage(this.photonView.ViewID, 30, col.GetComponent<PhotonView>().ViewID);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(hp + "/" + shield + "/" + kills);
        }
        else if (stream.IsReading)
        {
            string str = (string)stream.ReceiveNext();
            hp = System.Int32.Parse(str.Split('/')[0]);
            shield = System.Int32.Parse(str.Split('/')[1]);
            kills = System.Int32.Parse(str.Split('/')[2]);
        }
    }

    public void DropWeapon()
    {
        if (holdingWeapon != null)
        {
            this.photonView.RPC("ShowHide", RpcTarget.AllBuffered, holdingWeapon.GetComponent<PhotonView>().ViewID, true);
            holdingWeapon.transform.parent = null;
            holdingWeapon.transform.position = transform.position + transform.forward * 0.5f + transform.up * 2;
            holdingWeapon.transform.LookAt(transform.position + transform.forward * 1 + transform.up * 2);
            holdingWeapon.GetComponent<Rigidbody>().AddForce(transform.forward * 2, ForceMode.Impulse);
            holdingWeapon = null;
        }

        foreach (GameObject weapon in weapons)
        {
            if (weapon.activeInHierarchy) this.photonView.RPC("ShowHide", RpcTarget.AllBuffered, weapon.GetComponent<PhotonView>().ViewID, false);
        }
    }
}
