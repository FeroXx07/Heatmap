using System;
using Gamekit3D;
using Gamekit3D.Message;
using Server;
using UnityEngine;

public class EnemyHitSniffer : MonoBehaviour, IMessageReceiver
{
    public bool showDebugLogs = false;
    public bool useLocalHost = false; 
    public GameObject[] enemies;
    private UserSessionHandler _userSessionHandler;
    private void Awake()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        _userSessionHandler = GetComponent<UserSessionHandler>();
    }

    private void Start()
    {
        foreach (GameObject enemy in enemies)
        {
            Damageable enemyDamageable = enemy.GetComponent<Damageable>();
            enemyDamageable.onDamageMessageReceivers.Add(this);
        }
    }

    public void OnReceiveMessage(MessageType type, object sender, object msg)
    {
        MonoBehaviour senderObj = sender as MonoBehaviour;
        
        if (senderObj == null)
        {
            Debug.LogError("Cast failed");
            return;
        }
        
        switch (type)
        {
            case MessageType.DAMAGED:
            {
                Damageable.DamageMessage damageData = (Damageable.DamageMessage)msg;
                string timeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                SendHit(senderObj.gameObject, damageData, timeStamp);
            }
                break;
            case MessageType.DEAD:
            {
                Damageable.DamageMessage damageData = (Damageable.DamageMessage)msg;
                string timeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                SendHit(senderObj.gameObject, damageData, timeStamp);
                SendDeath(senderObj.gameObject, damageData, timeStamp);
            }
                break;
        }
    }
    
    void SendHit(GameObject enemy, Damageable.DamageMessage damageData, string timeStamp)
    {
        // Create struct
        Hit hit = new Hit
        {
            HitId = UInt64.MaxValue,
            SessionId = _userSessionHandler.session.SessionId,
            PositionX = (int)enemy.transform.position.x,
            PositionY = (int)enemy.transform.position.y,
            PositionZ = (int)enemy.transform.position.z,
            TimeStamp = timeStamp,
            AttackType = $"{damageData.damager}",
            Damage = damageData.amount,
            Hitter = $"{damageData.damager.gameObject.name}",
            Hitted = enemy.name,
            SourcePositionX = (int)damageData.damageSource.x,
            SourcePositionY = (int)damageData.damageSource.y,
            SourcePositionZ = (int)damageData.damageSource.z
        };
        // Send player hit data to PHP sender
        string json = hit.ToJson();
        if (showDebugLogs) Debug.Log(json);
        PHP_Sender.Instance.SendData(useLocalHost ? PHP_Sender.Instance.localHostUrlAddData : PHP_Sender.Instance.apiUrlAddData,json, DataAddedSuccessfully);
    }

    void SendDeath(GameObject enemy, Damageable.DamageMessage damageData, string timeStamp)
    {
        Death death = new Death
        {
            DeathId = UInt64.MaxValue,
            SessionId = _userSessionHandler.session.SessionId,
            PositionX = (int)enemy.transform.position.x,
            PositionY = (int)enemy.transform.position.y,
            PositionZ = (int)enemy.transform.position.z,
            TimeStamp = timeStamp,
            DeathType = "EnemyDeath"
        };
        string json = death.ToJson();
        if (showDebugLogs) Debug.Log(json);
        PHP_Sender.Instance.SendData(useLocalHost ? PHP_Sender.Instance.localHostUrlAddData : PHP_Sender.Instance.apiUrlAddData,json, DataAddedSuccessfully);
    }

    private void DataAddedSuccessfully(uint id)
    {
        Debug.Log("Data added successfully");
    }
}