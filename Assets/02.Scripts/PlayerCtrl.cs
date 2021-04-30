using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
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


    IEnumerator Start()
    {
        turnSpeed = 0.0f;

        tr = GetComponent<Transform>();
        anim = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();

        // player animator \ Apply Root Motion 언체크


        yield return new WaitForSeconds(0.5f);
        turnSpeed = turnSpeedValue;
    }

    // 키 입력
    void Update()
    {
        v = Input.GetAxis("Vertical");
        h = Input.GetAxis("Horizontal");
        r = Input.GetAxis("Mouse X");

        if (Input.GetMouseButtonDown(1))    // 우측 버튼 클릭 시 -> Jump
        {
            isJumping = true;
        }
    }

    // 물리적 처리
    void FixedUpdate()
    {
        Vector3 dir = (Vector3.forward * v) + (Vector3.right * h);
        tr.Translate(dir.normalized * Time.deltaTime * moveSpeed, Space.Self);
        tr.Rotate(Vector3.up * Time.smoothDeltaTime * turnSpeed * r);

        // 회전 버벅거림
        /*
        Vector3 rot = Vector3.right * r;
        Vector3 endPosition = tr.position + rot;
        tr.position = Vector3.Slerp(tr.position, endPosition, Time.fixedDeltaTime  * turnSpeed);
        */


        StartCoroutine(PlayerAnimation());
        Jump();
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


    private void OnTriggerEnter(Collider coll)
    {
        if((currHp > 0.0f) && (coll.CompareTag("PUNCH")))
        {
            currHp -= 15.0f;
            Debug.Log("Player got hit!!!");
            if (currHp <= 0.0f)
            {
                PlayerDie();
            }
        }
    }

    void PlayerDie()
    {
        // Player Die 애니메이션
        anim.SetTrigger(hashDie);

        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");
        foreach (GameObject monster in monsters)
        {
            monster.SendMessage("MonsterWin", SendMessageOptions.DontRequireReceiver);
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