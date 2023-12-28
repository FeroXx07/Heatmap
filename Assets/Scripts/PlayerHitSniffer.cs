using Gamekit3D;
using Gamekit3D.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using UnityEngine;

public class PlayerHitSniffer : MonoBehaviour, IMessageReceiver
{
    public bool showDebugLogs = false;
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

                    // Create struct
                    Hit hit = new Hit();    
                    hit.PostionX = (int)player.transform.position.x;
                    hit.PostionY = (int)player.transform.position.y;
                    hit.PostionZ = (int)player.transform.position.z;
                    hit.TimeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    hit.AttackType = $"{damageData.damager.gameObject.name + " attack"}";
                    hit.Damage = damageData.amount;
                    hit.Hitter = $"{damageData.damager.gameObject.name}";
                    hit.Hitted = $"Player";
                    hit.SourcePositionX = (int)damageData.damageSource.x;
                    hit.SourcePositionY = (int)damageData.damageSource.y;
                    hit.SourcePositionZ = (int)damageData.damageSource.z;

                    // Send player hit data to PHP sender
                    string json = JsonUtility.ToJson(hit);
                    //PHP_Sender.instance.SendData(json, null);
                }
                break;
            case MessageType.DEAD:
                {
                    Damageable.DamageMessage damageData = (Damageable.DamageMessage)msg;
                    // Send player death data to PHP sender
                }
                break;
        }
    }
}
