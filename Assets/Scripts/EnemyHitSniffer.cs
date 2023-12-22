using Gamekit3D;
using Gamekit3D.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitSniffer : MonoBehaviour, IMessageReceiver
{
    public GameObject[] enemies;
    //public List<string> messages = new List<string>();
    
    private void Awake()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
    }

    private void Start()
    {

        foreach (GameObject enemy in enemies) 
        {
            Damageable enemyDamageable = enemy.GetComponent<Damageable>(); ;
            enemyDamageable.onDamageMessageReceivers.Add(this);
        }

        
    }

    public void OnReceiveMessage(MessageType type, object sender, object msg)
    {
        string message = "Type: " + type + " Sender: " + " Msg: " + msg;

        //messages.Add(message);

        //Debug.Log(message);

        switch (type)
        {
            case MessageType.DAMAGED:
                {
                    Damageable.DamageMessage damageData = (Damageable.DamageMessage)msg;
                    //Damaged(damageData);
                }
                break;
            case MessageType.DEAD:
                {
                    Damageable.DamageMessage damageData = (Damageable.DamageMessage)msg;
                    //Die(damageData);
                }
                break;
        }
    }
}
