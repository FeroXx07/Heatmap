using System;
using Server;
using Unity.VisualScripting;
using UnityEngine;

public class MovementSniffer : MonoBehaviour
{
    public bool showDebugLogs = false;
    GameObject player;
    public float registerTime;
    private float timer;
    Action<uint> callback;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
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
        if (showDebugLogs) Debug.Log("Registering player position: " + position);
        Movement movement = new Movement();
        movement.SessionId = 0;
        movement.TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        movement.PositionX = (int)position.x;
        movement.PositionY = (int)position.y;
        movement.PositionZ = (int)position.z;
        string json = JsonUtility.ToJson(movement);
        //PHP_Sender.instance.SendData(json, callback);
    }
}