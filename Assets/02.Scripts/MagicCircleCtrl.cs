using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCircleCtrl : MonoBehaviour
{

    private float time = 3.0f;

    public GameObject magicCircle;


    IEnumerator Start()
    {
        yield return new WaitForSeconds(time);
        Destroy(magicCircle);
    }

}
