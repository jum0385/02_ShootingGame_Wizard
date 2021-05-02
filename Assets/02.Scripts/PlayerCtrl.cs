using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using Photon.Pun;
using TMPro;

public class PlayerCtrl : MonoBehaviour, IPunObservable
{
    private float v;
    private float h;
    private float r;

    [Header("이동 및 회전 속도")]
    public float moveSpeed = 8.0f;
    public float turnSpeed = 0.0f;
    public float jumpPower = 5.0f;

    private float turnSpeedValue = 200.0f;

    private Transform tr;
    private Animator anim;
    private Rigidbody rigid;

    private bool isDie = false;
    private bool isJumping = false;

    private readonly int hashRunF = Animator.StringToHash("RunF");
    private readonly int hashRunR = Animator.StringToHash("RunR");
    private readonly int hashIdle = Animator.StringToHash("IsIdle");
    private readonly int hashDie = Animator.StringToHash("Die");

    public float initHp = 100.0f;
    public float currHp = 100.0f;

    //

    private PhotonView pv;
    public bool playerDie = false;

    
    public TMP_Text userIdText;


    IEnumerator Start()
    {
        turnSpeed = 0.0f;

        tr = GetComponent<Transform>();
        anim = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();

        pv = GetComponent<PhotonView>();

        // player animator \ Apply Root Motion 언체크

        // UI에 닉네임 넣기
        userIdText.text = pv.Owner.NickName;

        if (pv.IsMine)
        {
            Camera.main.GetComponent<SmoothFollow>().target = transform.Find("CamPivot").transform;
        }
        else
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }

        yield return new WaitForSeconds(0.5f);
        turnSpeed = turnSpeedValue;
    }

    // 키 입력
    void Update()
    {
        v = Input.GetAxis("Vertical");
        h = Input.GetAxis("Horizontal");
        r = Input.GetAxis("Mouse X");

        if (Input.GetMouseButtonDown(1) && !playerDie)    // 우측 버튼 클릭 시 -> Jump
        {
            isJumping = true;
        }
    }

    // 물리적 처리
    void FixedUpdate()
    {
        
        if (pv.IsMine && !playerDie)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed * v);
            transform.Rotate(Vector3.up * Time.deltaTime * 150.0f * h);

            StartCoroutine(PlayerAnimation());
            Jump();

        }
        // Rotation();

    }

    IEnumerator PlayerAnimation()
    {
        anim.SetFloat(hashRunF, 0);
        anim.SetFloat(hashRunR, 0);
        anim.SetBool(hashIdle, false);

        while (!isDie)
        {

            anim.SetFloat(hashRunF, v);
            anim.SetFloat(hashRunR, h);


            yield return new WaitForSeconds(0.2f);
        }

    }

    void Jump()
    {
        if (!isJumping)
        {
            return;
        }
        Debug.Log("Jump!!");
        // rigid.MovePosition(transform.position + Vector3.up);
        rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);

        isJumping = false;
        // tr.Translate(Vector3.up * Time.deltaTime * jumpValue , Space.Self);
        // rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.CompareTag("FIREBALL"))
        {
            // anim.SetTrigger(hashHit);

            currHp -= 20.0f;
            if (currHp <= 0 && !playerDie)
            {
                PlayerDie();
                // StartCoroutine(DieAction());
            }
        }
    }

    void PlayerDie()
    {
        // Player Die 애니메이션
        anim.SetTrigger(hashDie);

        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        playerDie = true;

        // GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");
        // foreach (GameObject monster in monsters)
        // {
        //     monster.SendMessage("MonsterWin", SendMessageOptions.DontRequireReceiver);
        // }
    }





    // 네트워크를 통해서 수신받을 변수
    Vector3 receivePos = Vector3.zero;
    Quaternion receiveRot = Quaternion.identity;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 데이터를 보낸다
        if (stream.IsWriting) // PhotonView.IsMine == ture
        {
            stream.SendNext(transform.position); // 위치
            stream.SendNext(transform.rotation); // 회전값

        }
        // 내 탱크의 복사본들이 받는 부분
        else
        {
            receivePos = (Vector3)stream.ReceiveNext();
            receiveRot = (Quaternion)stream.ReceiveNext();
        }
    }





}



// if (v > 0.0f)
// {
//     anim.SetFloat(hashRunF, v);
// }
// else if (v < 0.0f)
// {
//     anim.SetFloat(hashRunF, v);
// }
// if (h > 0.0f)
// {
//     anim.SetFloat(hashRunR, h);
// }
// else if (h < 0.0f)
// {
//     anim.SetFloat(hashRunR, h);
// }
// else
// {
//     anim.SetBool(hashIdle, true);
// }