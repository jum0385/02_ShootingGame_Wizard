using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class PhotonManager_WaitingRoom : MonoBehaviourPunCallbacks
{
    // 플레이어를 표시할 프리팹
    public GameObject waitingPlayerPrefab;
    // 플레이어 프리팹을 차일드화 시킬 부모 객체
    public Transform TeamA;
    public Transform TeamB;



    private void Awake()
    {
        // 방장이 혼자 씬을 로딩하면, 나머지 사람들은 자동으로 싱크가 됨
        PhotonNetwork.AutomaticallySyncScene = true;

    }

    private void Start()
    {
        switch (PhotonNetwork.CurrentRoom.PlayerCount % 2)
        {
            case 0:
                Debug.Log("짝수");
                break;
            case 1:
                Debug.Log("홀수");
                break;
        }
    }


    // 새로운 유저가 들어왔을 때
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        switch (PhotonNetwork.CurrentRoom.PlayerCount % 2)
        {
            case 0:
                Debug.Log("짝수");
                break;
            case 1:
                Debug.Log("홀수");
                break;
        }
    }



}
