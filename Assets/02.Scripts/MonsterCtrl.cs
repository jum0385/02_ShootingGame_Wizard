using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterCtrl : MonoBehaviour
{
    private Transform monsterTr;
    private Transform playerTr;

    private NavMeshAgent agent;

    public enum STATE { IDLE, TRACE, ATTACK, DIE };

    public STATE state = STATE.IDLE;

    [Range(10, 50)]
    public float traceDist = 10.0f;
    public float attackDist = 2.5f;

    public bool isDie = false;

    private Animator anim;

    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashDie = Animator.StringToHash("Die");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");

    // private float initHp = 100.0f;
    private float currHp = 100.0f;


    // Awake < OnEnable < Start
    //OnEnable : 스크립트가 활성화될 때마다 호출

    void OnEnable()
    {
        state = STATE.IDLE;
        isDie = false;
        currHp = 100.0f;
        GetComponent<CapsuleCollider>().enabled = true;
        StartCoroutine(CheckState());
        StartCoroutine(MonsterAction());
    }

    void Awake()
    {
        monsterTr = GetComponent<Transform>();

        playerTr = GameObject.FindGameObjectWithTag("PLAYER")?.GetComponent<Transform>();

        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();


        // player animator \ Apply Root Motion 체크
        anim.applyRootMotion = true;
    }

    IEnumerator CheckState()
    {
        while (!isDie)
        {
            if (state == STATE.DIE)
            {
                yield break;
            }

            float distance = Vector3.Distance(monsterTr.position, playerTr.position);

            if (distance <= attackDist)
            {
                state = STATE.ATTACK;
            }
            else if (distance <= traceDist)
            {
                state = STATE.TRACE;
            }
            else
            {
                state = STATE.IDLE;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (state)
            {
                case STATE.IDLE:
                    agent.isStopped = true;
                    anim.applyRootMotion = true;
                    anim.SetBool(hashTrace, false);
                    break;

                case STATE.TRACE:
                    agent.SetDestination(playerTr.position);
                    agent.isStopped = false;
                    anim.applyRootMotion = false;
                    anim.SetBool(hashTrace, true);
                    anim.SetBool(hashAttack, false);
                    break;

                case STATE.ATTACK:
                    anim.applyRootMotion = true;
                    anim.SetBool(hashAttack, true);
                    break;

                case STATE.DIE:
                    agent.isStopped = true;
                    anim.applyRootMotion = true;
                    anim.SetTrigger(hashDie);
                    GetComponent<CapsuleCollider>().enabled = false;
                    isDie = true;
                    break;
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator DieAction()
    {
        Debug.Log("Monster Die");
        yield return new WaitForSeconds(7.0f);
        this.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.CompareTag("FIREBALL"))
        {
            anim.SetTrigger(hashHit);

            currHp -= 20.0f;
            if (currHp <= 0)
            {
                state = STATE.DIE;
                StartCoroutine(DieAction());
            }
        }
    }

    // Player Die
    public void MonsterWin()
    {
        StopAllCoroutines();
        agent.isStopped = true;

        //todo 승리 애니메이션
        anim.SetTrigger(hashPlayerDie);

    }


}
