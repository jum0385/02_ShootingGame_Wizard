using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public List<Transform> points = new List<Transform>();
    public GameObject monsterPrefab;

    public float createTime = 3.0f;
    public bool isGameOver = false;

    public List<GameObject> monsterPool = new List<GameObject>();
    public int maxPool = 4;                 //!

    private Transform playerTr;

    
    public GameObject magicCirclePrefab;

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


}
