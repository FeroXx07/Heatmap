using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovementSniffer : MonoBehaviour
{
    GameObject player;

    public float registerTime;

    private float timer;

    Action<uint>callback;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    void Update()
    {
        timer += Time.deltaTime;

        if(timer > registerTime)
        {
            timer = 0f;
            RegisterPosition();
        }
        
    }

    void RegisterPosition()
    {

        Vector3 position = player.transform.position;

        Debug.Log("Registering player position: " + position);


        Movement movement = new Movement();

        movement.SessionId = 0;
        movement.TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        movement.PostionX = (int)position.x;
        movement.PostionY = (int)position.y;
        movement.PostionZ = (int)position.z;

        string json = JsonUtility.ToJson(movement);

        PHP_Sender.instance.SendData(json,callback);
    }
}
