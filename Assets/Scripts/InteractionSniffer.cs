using Gamekit3D;
using Gamekit3D.Message;
using System;
using System.Collections.Generic;
using Server;
using UnityEngine;

public class InteractionSniffer : MonoBehaviour, IMessageReceiver
{
    public bool showDebugLogs = false;
    public bool useLocalHost = false; 
    private UserSessionHandler _userSessionHandler;

    public enum InteractableType
    {
        NONE = 0,
        GET_KEY = 1,
        USE_KEY = 2,
        BREAK_BOX = 3,
        OPEN_DOOR = 4,
        SWITCH = 5,
        PRESSURE_PLATE = 6,
        GET_TREASURE = 7,
        HEAL = 8,
        SHOW_INFO_TEXT = 9,
        DAMAGE_BOX = 10,
        FINISH = 11
    }

    public static Action<InteractableType, GameObject> OnInteraction;
    public List<GameObject> interactables;
    //public List<string> messages = new List<string>();

    private void OnEnable()
    {
        OnInteraction += OnSniffInteraction;
    }

    private void Awake()
    {
        //GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        //foreach (GameObject go in allGameObjects)
        //{
        //    // Check if the GameObject has the specified script attached
        //    if (go.GetComponent("InteractOnTrigger") != null && !go.transform.parent.name.Contains("Example"))
        //    {
        //        // Add the GameObject to the list
        //        interactables.Add(go);
        //    }
        //}
        _userSessionHandler = GetComponent<UserSessionHandler>();

        GameObject[] enemyBoxes = GameObject.FindGameObjectsWithTag("Destructible");

        foreach (GameObject box in enemyBoxes)
        {
            Damageable enemyDamageable = box.GetComponent<Damageable>();
            enemyDamageable.onDamageMessageReceivers.Add(this);
        }
    }


    private void OnSniffInteraction(InteractableType type, GameObject go)
    {
       string message = $"Interaction Type: {type}, Sender: {go}";
       Debug.Log(message);
        
        switch (type)
        {
            case InteractableType.NONE:
                break;
            case InteractableType.GET_KEY:
                SendInteraction(type, go);
                break;
            case InteractableType.USE_KEY:
                SendInteraction(type, go);
                break;
            case InteractableType.DAMAGE_BOX: //wip
                SendInteraction(type, go);
                break;
            case InteractableType.BREAK_BOX: //wip
                SendInteraction(type, go);
                break;
            case InteractableType.OPEN_DOOR:
                SendInteraction(type, go);
                break;
            case InteractableType.SWITCH:
                SendInteraction(type, go);
                break;
            case InteractableType.PRESSURE_PLATE:
                SendInteraction(type, go);
                break;
            case InteractableType.GET_TREASURE:
                SendInteraction(type, go);
                break;
            case InteractableType.HEAL:
                SendInteraction(type, go);
                break;
            case InteractableType.FINISH:
                SendInteraction(type, go);
                break;
            case InteractableType.SHOW_INFO_TEXT:
                break;
        }
    }

    public void OnReceiveMessage(MessageType type, object sender, object msg)
    {
        Damageable senderScr = sender as Damageable;
        if (senderScr == null) return;
        
        string message = $"Interaction Type: {type}, Sender: {sender}, Msg: {msg}";
        Debug.Log(message);

        switch (type)
        {
            case MessageType.DAMAGED:
                {
                    OnSniffInteraction(InteractableType.DAMAGE_BOX, senderScr.gameObject);
                }
                break;
            case MessageType.DEAD:
                {
                    OnSniffInteraction(InteractableType.BREAK_BOX, senderScr.gameObject);
                }
                break;
        }
    }

    void SendInteraction(InteractableType type, GameObject go)
    {
        // Create struct
        Interaction interaction = new Interaction
        {
            InteractionId = UInt64.MaxValue,
            SessionId = _userSessionHandler.session.SessionId,
            PositionX = (int)go.transform.position.x,
            PositionY = (int)go.transform.position.y,
            PositionZ = (int)go.transform.position.z,
            TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            InteractionType = type.ToString()
        };
        // Send player hit data to PHP sender
        string json = interaction.ToJson();
        if (showDebugLogs) Debug.Log(json);
        PHP_Sender.Instance.SendData(useLocalHost ? PHP_Sender.Instance.localHostUrlAddData : PHP_Sender.Instance.apiUrlAddData,json, DataAddedSuccessfully);
    }
    
    private void DataAddedSuccessfully(uint id)
    {
        Debug.Log("Data added successfully");
    }
}
