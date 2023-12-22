using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSniffer : MonoBehaviour
{
    GameObject player;

    public float registerTime;

    private float timer;

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

    }
}
