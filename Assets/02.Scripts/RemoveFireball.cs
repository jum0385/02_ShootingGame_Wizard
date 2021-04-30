using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveFireball : MonoBehaviour
{

    public GameObject sparkEffect;

    void OnCollisionEnter(Collision coll)
    {
        ContactPoint cont = coll.GetContact(0);

        // 법선 벡터
        Vector3 normal = cont.normal;
        // 법선 벡터를 쿼터니언 타입으로 변환
        Quaternion rot = Quaternion.LookRotation(-normal);

        // 스파크 이펙트 발생(생성)
        // Instantiate(생성객체, 좌표, 회전각도);
        GameObject spark = Instantiate(sparkEffect, cont.point, rot);
        Destroy(spark, 0.3f);

        Destroy(this.gameObject);
    }
}
