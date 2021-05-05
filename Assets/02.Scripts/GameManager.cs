using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance = null;

    public List<Transform> points = new List<Transform>();
    public GameObject monsterPrefab;

    public float createTime = 3.0f;
    public bool isGameOver = false;

    public List<GameObject> monsterPool = new List<GameObject>();
    private int maxPool = 1;                 //골렘 수

    private Transform playerTr;

    public GameObject magicCirclePrefab;

    //
    private PhotonView pv;

    private GameObject monsterTemp;

    public bool isCheckScore = false;
    public bool isGameEnd = false;


    [Header("UI - A팀 : B팀")]
    public TMP_Text teamA_Text;
    public TMP_Text teamB_Text;
    private int teamA_score;
    private int teamB_score;


    //델리게이트
    public delegate void Callback(int winnerLayer);
    public static event Callback Result_handler;

    public List<Transform> playerPoints = new List<Transform>();

    [Header("UI - A팀 : B팀")]
    public Image waitingImg;
    public TMP_Text waitText;
    public TMP_Text startText;





    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        pv = GetComponent<PhotonView>();

        GameObject.Find("PlayerSpawnPointGroup").GetComponentsInChildren<Transform>(playerPoints);
        Vector3 pos = playerPoints[PhotonNetwork.CurrentRoom.PlayerCount - 1].position;
        Quaternion rot = playerPoints[PhotonNetwork.CurrentRoom.PlayerCount - 1].rotation;

        startText.enabled = false;

        // 플레이어 생성
        GameObject playerTemp = PhotonNetwork.Instantiate("Player", pos, rot, 0);
    }




    void CreatePool()
    {
        for (int i = 0; i < maxPool; i++)
        {
            GameObject monster = PhotonNetwork.Instantiate("Monster", Vector3.zero, Quaternion.identity, 0);
            // GameObject monster = Instantiate<GameObject>(monsterPrefab);
            monster.name = $"Monster_{i:00}";
            monster.SetActive(false);

            monsterPool.Add(monster);
        }
    }

    void Start()
    {
        playerTr = GameObject.FindGameObjectWithTag("PLAYER")?.GetComponent<Transform>();
        GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>(points);

        monsterPrefab = Resources.Load<GameObject>("monster");

        // todo 몬스터 생성
        // if (PhotonNetwork.IsMasterClient)
        // {
        //     Debug.Log($"{PhotonNetwork.NickName} : 방장");
        //     CreatePool();   // monster pool 생성
        //     StartCoroutine(GetMonsterInPool());
        // }

    }

    void Update()
    {
        if (isCheckScore)
        {
            StartCoroutine(CheckScore());
        }
    }

    IEnumerator CheckScore()
    {
        while (!isGameEnd)
        {
            teamA_score = int.Parse(teamA_Text.text);
            teamB_score = int.Parse(teamB_Text.text);

            if ((teamA_score == 0) || (teamB_score == 0))
            {
                // 어떤 팀이 이겼는지 확인
                if (teamA_score == 0)    // B팀이 이김
                {
                    Result_handler(11);
                }
                else                    // A팀이 이김
                {
                    Result_handler(10);
                }

                isGameEnd = true;
            }
            yield return new WaitForSeconds(0.2f);

        }
    }


    IEnumerator GetMonsterInPool()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(createTime);

            foreach (GameObject monster in monsterPool)
            {
                if (monster.activeSelf == false)
                {
                    monsterTemp = monster;
                    pv.RPC("ActiveMonster", RpcTarget.AllViaServer);
                    //!
                    CreateMagicCircle(monster);
                    break;
                }
            }

        }
    }

    [PunRPC]
    public void ActiveMonster()
    {
        int idx = Random.Range(1, points.Count);
        monsterTemp.transform.position = points[idx].position;
        monsterTemp.transform.LookAt(playerTr.position);
        monsterTemp.SetActive(true);

        Debug.Log($"{PhotonNetwork.NickName} : monster active");

    }

    void CreateMagicCircle(GameObject monster)
    {
        // Instantiate( 생성할 객체, 위치 값, 회전 값 )
        Vector3 height = new Vector3(0, 0.1f, 0);
        Vector3 circlePositoin = monster.transform.position;
        GameObject circle = Instantiate(magicCirclePrefab, monster.transform.position + height, monster.transform.rotation, monster.transform);
        // yield return new WaitForSeconds(3.0f);
        Destroy(circle, 3.0f);
    }

    // 풀방인지 체크하고
    // 풀방이면 게임시작
    public void CheckFullRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            waitText.enabled = false;
            waitingImg.enabled = false;
            startText.enabled = true;

            PlayerCtrl.playerDie = false;

            Invoke("RemoveStartText", 2.0f);
        }
    }

    void RemoveStartText()
    {
        startText.enabled = false;
    }



}
