using Gamekit3D;
using Gamekit3D.Message;
using System;
using Server;
using UnityEngine;

public class PlayerHitSniffer : MonoBehaviour, IMessageReceiver
{
    public bool showDebugLogs = false;
    public bool useLocalHost = false; 
    public GameObject player;
    private Damageable playerDamageable;
    private UserSessionHandler _userSessionHandler;

    private bool canInvoke = true;
    private float timer = 0;
    private readonly float timeToReset = 1;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        _userSessionHandler = GetComponent<UserSessionHandler>();
    }

    private void Start()
    {
        playerDamageable = player.GetComponent<Damageable>();
        player.GetComponent<PlayerController>().OnVolumeDie.AddListener(VolumeDeath);
        playerDamageable.onDamageMessageReceivers.Add(this);
    }

    private void OnDisable()
    {
        player.GetComponent<PlayerController>().OnVolumeDie.RemoveListener(VolumeDeath);
    }

    private void Update()
    {
        if (!canInvoke)
        {
            timer -= Time.deltaTime;
            if (timer <= 0.0f)
            {
                canInvoke = true;
            }
        }
    }

    public void OnReceiveMessage(MessageType type, object sender, object msg)
    {
        switch (type)
        {
            case MessageType.DAMAGED:
                {
                    Damageable.DamageMessage damageData = (Damageable.DamageMessage)msg;
                    string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    SendHit(damageData, timeStamp);
                }
                break;
            case MessageType.DEAD:
                {
                    Damageable.DamageMessage damageData = (Damageable.DamageMessage)msg;
                    string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    SendHit(damageData,timeStamp, true);
                }
                break;
        }
    }

    void SendHit(Damageable.DamageMessage damageData, string timeStamp, bool isDeath = false)
    {
        // Create struct
        Hit hit = new Hit
        {
            HitId = UInt64.MaxValue,
            SessionId = _userSessionHandler.session.SessionId,
            PositionX = (int)player.transform.position.x,
            PositionY = (int)player.transform.position.y,
            PositionZ = (int)player.transform.position.z,
            TimeStamp = timeStamp,
            AttackType = $"{damageData.damager}",
            Damage = damageData.amount,
            Hitter = $"{damageData.damager.gameObject.name}",
            Hitted = $"Player",
            SourcePositionX = (int)damageData.damageSource.x,
            SourcePositionY = (int)damageData.damageSource.y,
            SourcePositionZ = (int)damageData.damageSource.z,
            Mortal = (short)(isDeath ? 1:0)
        };
        // Send player hit data to PHP sender
        string json = hit.ToJson();
        if (showDebugLogs) Debug.Log(json);
        PHP_Sender.Instance.SendData(useLocalHost ? PHP_Sender.Instance.localHostUrlAddData : PHP_Sender.Instance.apiUrlAddData,json, DataAddedSuccessfully);
    }

    // void SendDeath(Damageable.DamageMessage damageData, string timeStamp)
    // {
    //     Death death = new Death
    //     {
    //         DeathId = UInt64.MaxValue,
    //         SessionId = _userSessionHandler.session.SessionId,
    //         PositionX = (int)player.transform.position.x,
    //         PositionY = (int)player.transform.position.y,
    //         PositionZ = (int)player.transform.position.z,
    //         TimeStamp = timeStamp,
    //         DeathType = "PlayerDeath"
    //     };
    //     string json = death.ToJson();
    //     if (showDebugLogs) Debug.Log(json);
    //     PHP_Sender.Instance.SendData(useLocalHost ? PHP_Sender.Instance.localHostUrlAddData : PHP_Sender.Instance.apiUrlAddData,json, DataAddedSuccessfully);
    // }

    void VolumeDeath(GameObject player)
    {
        if (canInvoke)
        {
            canInvoke = false;
            timer = timeToReset;
        }
        else
        {
            return;
        }
        // Create struct
        Hit hit = new Hit
        {
            HitId = UInt64.MaxValue,
            SessionId = _userSessionHandler.session.SessionId,
            PositionX = (int)player.transform.position.x,
            PositionY = (int)player.transform.position.y,
            PositionZ = (int)player.transform.position.z,
            TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            AttackType = $"VolumeDamage",
            Damage = 0,
            Hitter = "VolumeDamage",
            Hitted = $"Player",
            SourcePositionX = (int)player.transform.position.x,
            SourcePositionY = (int)player.transform.position.x,
            SourcePositionZ = (int)player.transform.position.x,
            Mortal = 1,
        };
        // Send player hit data to PHP sender
        string json = hit.ToJson();
        if (showDebugLogs) Debug.Log(json);
        PHP_Sender.Instance.SendData(useLocalHost ? PHP_Sender.Instance.localHostUrlAddData : PHP_Sender.Instance.apiUrlAddData,json, DataAddedSuccessfully);
    }

    private void DataAddedSuccessfully(uint id)
    {
        Debug.Log("Data added successfully");
    }
}
