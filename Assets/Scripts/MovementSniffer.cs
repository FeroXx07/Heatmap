using System;
using Server;
using Unity.VisualScripting;
using UnityEngine;

public class MovementSniffer : MonoBehaviour
{
    public bool showDebugLogs = false;
    public bool useLocalHost = false;
    GameObject player;
    public float registerTime;
    private float timer;
    Action<uint> callback;
    UserSessionHandler sessionHandler;
    bool SessionStarted;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sessionHandler = GetComponent<UserSessionHandler>();
        sessionHandler.OnSessionStart += SessionStart;
    }

    void Update()
    {
        if (!SessionStarted) return;

        timer += Time.deltaTime;
        if (timer > registerTime)
        {
            timer = 0f;
            RegisterPosition();
        }
    }

    void RegisterPosition()
    {
        Vector3 position = player.transform.position;

        Movement movement = new Movement
        {
            MovementId = UInt64.MaxValue,
            SessionId = sessionHandler.session.SessionId,
            TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            PositionX = (int)position.x,
            PositionY = (int)position.y,
            PositionZ = (int)position.z
        };

        string json = movement.ToJson();
        if (showDebugLogs) Debug.Log(json);
        PHP_Sender.Instance.SendData(useLocalHost ? PHP_Sender.Instance.localHostUrlAddData : PHP_Sender.Instance.apiUrlAddData, json, DataAddedSuccessfully);
    }
    private void DataAddedSuccessfully(uint id)
    {
        Debug.Log("Data added successfully");
    }

    void SessionStart(UInt64 id)
    {
        SessionStarted = true;
    }
}