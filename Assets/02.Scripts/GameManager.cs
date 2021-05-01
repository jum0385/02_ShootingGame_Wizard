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
    public int maxPool = 4;                 //골렘 수

    private Transform playerTr;

    public GameObject magicCirclePrefab;

    //
    private PhotonView pv;

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

        //todo 스폰위치
        Vector3 pos = new Vector3(-17.0f, 0, -58.0f);
        // 플레이어 생성
        PhotonNetwork.Instantiate("Player",pos,Quaternion.identity, 0);

    }

    void CreatePool()
    {
        for (int i = 0; i < maxPool; i++)
        {
            GameObject monster = Instantiate<GameObject>(monsterPrefab);
            monster.name = $"Monster_{i:00}";
            monster.SetActive(false);

            monsterPool.Add(monster);
        }
    }

    void Start()
    {
        pv = GetComponent<PhotonView>();
        playerTr = GameObject.FindGameObjectWithTag("PLAYER")?.GetComponent<Transform>();
        GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>(points);

        monsterPrefab = Resources.Load<GameObject>("monster");

        CreatePool();   // monster pool 생성

        StartCoroutine(GetMonsterInPool());
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
                    int idx = Random.Range(1, points.Count);
                    monster.transform.position = points[idx].position;
                    monster.transform.LookAt(playerTr.position);
                    monster.SetActive(true);

                    //!
                    CreateMagicCircle(monster);
                    break;
                }
            }

        }
    }

    void CreateMagicCircle(GameObject monster)
    {
        // Instantiate( 생성할 객체, 위치 값, 회전 값 )
        Vector3 height = new Vector3(0, 0.1f, 0);
        Vector3 circlePositoin =  monster.transform.position;
        GameObject circle = Instantiate(magicCirclePrefab, monster.transform.position + height, monster.transform.rotation);
        // yield return new WaitForSeconds(3.0f);
        Destroy(circle, 3.0f);
    }




    //



}
