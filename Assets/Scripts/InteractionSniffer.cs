using Gamekit3D;
using Gamekit3D.Message;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSniffer : MonoBehaviour, IMessageReceiver
{
    public bool showDebugLogs = false;
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

        GameObject[] enemyBoxes = GameObject.FindGameObjectsWithTag("Destructible");

        foreach (GameObject box in enemyBoxes)
        {
            Damageable enemyDamageable = box.GetComponent<Damageable>();
            enemyDamageable.onDamageMessageReceivers.Add(this);
        }
    }


    public void OnSniffInteraction(InteractableType type, GameObject go)
    {
        string message = "Interaction Type: " + type + " Sender: " + go;

        //messages.Add(message);

       Debug.Log(message);

        switch (type)
        {
            case InteractableType.NONE:
                break;
            case InteractableType.GET_KEY:
                break;
            case InteractableType.USE_KEY:
                break;
            case InteractableType.DAMAGE_BOX: //wip
                break;
            case InteractableType.BREAK_BOX: //wip
                break;
            case InteractableType.OPEN_DOOR:
                break;
            case InteractableType.SWITCH:
                break;
            case InteractableType.PRESSURE_PLATE:
                break;
            case InteractableType.GET_TREASURE:
                break;
            case InteractableType.HEAL:
                break;
            case InteractableType.SHOW_INFO_TEXT:
                break;
        }
    }

    public void OnReceiveMessage(MessageType type, object sender, object msg)
    {
        Damageable senderScr = sender as Damageable;
        if (senderScr == null) return;
        
        string message = "Type: " + type + " Sender: " + senderScr + " Msg: " + msg;

        //messages.Add(message);

        //Debug.Log(message);

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

    public void SayTest()
    {
        Debug.Log("test function");
    }
}
