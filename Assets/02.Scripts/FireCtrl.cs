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

    void Start()
    {
        audio = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

        pv = GetComponent<PhotonView>();

    }

    void Update()
    {
        if (pv.IsMine)
        {
            if (Input.GetMouseButtonDown(0))
            {
                pv.RPC("Fire", RpcTarget.AllViaServer);
            }

        }
    }

    [PunRPC]
    void Fire()
    {
        Instantiate(fireballPrefab, firePos.position, this.transform.rotation);
        audio.PlayOneShot(fireSFX, 0.8f);

    }

}
