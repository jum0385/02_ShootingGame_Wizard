using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(AudioSource))]
public class FireCtrl : MonoBehaviour
{
    public Transform firePos;
    public GameObject fireballPrefab;

    [HideInInspector]
    public new AudioSource audio;
    public AudioClip fireSFX;

    private Animator anim;

    private PhotonView pv;
    private PlayerCtrl pCtrl;

    void Start()
    {
        audio = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        pCtrl = GetComponent<PlayerCtrl>();

        pv = GetComponent<PhotonView>();

    }

    void Update()
    {
        if (pv.IsMine)
        {
            if (Input.GetMouseButtonDown(0) && !pCtrl.playerDie)
            {
                pv.RPC("Fire", RpcTarget.AllViaServer);
            }

        }
    }

    [PunRPC]
    void Fire()
    {
        GameObject fireObj = Instantiate(fireballPrefab, firePos.position, this.transform.rotation);
        if (this.gameObject.layer == 10)
        {
            fireObj.layer = 10;
        }
        else
        {
            fireObj.layer = 11;
        }
        audio.PlayOneShot(fireSFX, 0.8f);

    }

}
