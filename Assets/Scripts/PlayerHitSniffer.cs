using Gamekit3D;
using Gamekit3D.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitSniffer : MonoBehaviour, IMessageReceiver
{
    public GameObject player;
    private Damageable playerDamageable;
    
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        playerDamageable = player.GetComponent<Damageable>();

        playerDamageable.onDamageMessageReceivers.Add(this);
    }

    public void OnReceiveMessage(MessageType type, object sender, object msg)
    {
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
