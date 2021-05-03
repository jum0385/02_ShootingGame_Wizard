using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    private float v;
    private float h;
    private float r;

    [Header("이동 및 회전 속도")]
    public float moveSpeed = 8.0f;
    public float turnSpeed = 0.0f;
    private float jumpPower = 5000.0f;

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
    public static bool playerDie = true;


    [Header("플레이어UI - 닉네임, 체력")]
    public TMP_Text userIdText;
    public Image hpBar;


    // [Header("UI - A팀 : B팀")]
    private TMP_Text teamA_Text;
    private TMP_Text teamB_Text;
    private int teamA_score;
    private int teamB_score;

    private TMP_Text result;

    public List<Transform> playerPoints = new List<Transform>();


    [Header("플레이어 옷 텍스쳐")]
    public Texture[] textures;
    public new SkinnedMeshRenderer renderer;




    IEnumerator Start()
    {
        turnSpeed = 0.0f;

        tr = GetComponent<Transform>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
        renderer = GetComponentInChildren<SkinnedMeshRenderer>();

        // 팀 스코어 UI
        teamA_Text = GameObject.FindWithTag("SCORE_A").GetComponent<TMP_Text>();
        teamB_Text = GameObject.FindWithTag("SCORE_B").GetComponent<TMP_Text>();

        // 결과 나타내는 UI
        result = GameObject.FindWithTag("RESULT").GetComponent<TMP_Text>();
        result.enabled = false;

        // 플레이어 스폰 위치 및 방향
        GameObject.Find("PlayerSpawnPointGroup").GetComponentsInChildren<Transform>(playerPoints);
        Vector3 pos = playerPoints[pv.ViewID / 1000].position;
        Quaternion rot = playerPoints[pv.ViewID / 1000].rotation;
        tr.position = pos;
        tr.rotation = rot;

        // player animator \ Apply Root Motion 언체크

        // 팀 나누기
        pv.RPC("SetTeam", RpcTarget.AllViaServer);

        // 팀 스코어 초기화
        SetRoomInfo();

        yield return new WaitForSeconds(0.5f);

        // UI에 닉네임 넣기
        if (this.gameObject.layer == LayerMask.NameToLayer("PLAYER_B"))  // B팀이면 파란색
        {
            userIdText.text = $"<color=#00F4FF>{pv.Owner.NickName}</color>";
        }
        else if (this.gameObject.layer == LayerMask.NameToLayer("PLAYER_A"))  // A팀이면 분홍색
        {
            userIdText.text = $"<color=#FF00A6>{pv.Owner.NickName}</color>";
        }

        if (pv.IsMine)
        {
            Camera.main.GetComponent<SmoothFollow>().target = transform.Find("CamPivot").transform;
            // 델리게이트
            GameManager.Result_handler += SetResult;
        }
        else
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }

        GameManager.instance.CheckFullRoom();


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
            pv.RPC("UpdateHp", RpcTarget.AllViaServer);
            hpBar.fillAmount = currHp / initHp;
            if (currHp <= 0 && !playerDie)
            {
                PlayerDie();
                // StartCoroutine(DieAction());
            }
        }
    }

    [PunRPC]
    void UpdateHp()
    {
        hpBar.fillAmount = currHp / initHp;
    }

    void PlayerDie()
    {
        // Player Die 애니메이션
        anim.SetTrigger(hashDie);

        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        playerDie = true;

        // 팀스코어 감소
        if (this.gameObject.layer == LayerMask.NameToLayer("PLAYER_B"))
        {
            teamB_score = int.Parse(teamB_Text.text);
            teamB_score--;
            teamB_Text.text = $"{teamB_score}";

            GameManager.instance.isCheckScore = true; //!
        }
        else if (this.gameObject.layer == LayerMask.NameToLayer("PLAYER_A"))
        {
            teamA_score = int.Parse(teamA_Text.text);
            teamA_score--;
            teamA_Text.text = $"{teamA_score}";

            GameManager.instance.isCheckScore = true; //!
        }

        // GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");
        // foreach (GameObject monster in monsters)
        // {
        //     monster.SendMessage("MonsterWin", SendMessageOptions.DontRequireReceiver);
        // }
    }

    // UI에 결과 띄우기
    void SetResult(int winnerLayer)
    {
        if (pv.IsMine)
        {
            if (gameObject.layer == winnerLayer)
            {
                result.text = "YOU WIN!!!";
            }
            else
            {
                result.text = "YOU LOSE...";
            }
        }

        result.enabled = true;
        if (pv.IsMine)
        {
            // 5초후 로비로 나가기
            Invoke("ExitRoom", 5.0f);
        }
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

    // 팀나누기
    [PunRPC]
    void SetTeam()
    {
        // B팀
        if ((pv.ViewID / 1000) % 2 == 0)
        {
            gameObject.layer = 11;
            renderer.material.mainTexture = textures[1];
            // int temp = int.Parse(teamB_Text.text);
            // temp ++;
            // teamB_Text.text = $"{temp}";
        }
        // A팀
        else
        {
            gameObject.layer = 10;
            renderer.material.mainTexture = textures[0];
            // int temp = int.Parse(teamA_Text.text);
            // temp ++;
            // teamA_Text.text = $"{temp}";
        }
    }

    // 기본 세팅 N:N
    void SetRoomInfo()
    {
        int temp = (PhotonNetwork.CurrentRoom.PlayerCount) / 2;
        teamA_Text.text = $"{temp}";
        teamB_Text.text = $"{temp}";

        // 


        // Room currentRoom =  PhotonNetwork.CurrentRoom;
        // roomNameText.text = currentRoom.Name;
        // connectInfoText.text = $"{currentRoom.PlayerCount}/{currentRoom.MaxPlayers}";
    }

    // 클론을 지우는 등 cleanUp 작업
    public void ExitRoom()
    {
        // PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }

    // // CleanUp 끝난 후에 호출되는 콜백
    // // 이미 로비에 있는 상태이므로 씬만 바꿔주면 됨
    // public override void OnLeftRoom()
    // {
    //     // Lobby 씬으로 되돌아 가기...
    //     SceneManager.LoadScene("Lobby");
    // }





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