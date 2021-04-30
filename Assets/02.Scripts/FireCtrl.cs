using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FireCtrl : MonoBehaviour
{
    public Transform firePos;
    public GameObject fireballPrefab;

    [HideInInspector]
    public new AudioSource audio;
    public AudioClip fireSFX;

    private Animator anim;

    void Start()
    {
        audio = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(fireballPrefab, firePos.position, this.transform.rotation);
            audio.PlayOneShot(fireSFX, 0.8f);
        }
    }

}
