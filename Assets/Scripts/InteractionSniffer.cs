using Gamekit3D;
using Gamekit3D.Message;
using System;
using System.Collections.Generic;
using UnityEngine;


public class InteractionSniffer : MonoBehaviour
{
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
        SHOW_INFO_TEXT = 9
    }

    static public Action<InteractableType, GameObject> OnInteraction;
    public List<GameObject> interactables;
    //public List<string> messages = new List<string>();

    private void OnEnable()
    {
        OnInteraction += OnSniffInteraction;
    }

    private void Awake()
    {
        //GameObject interactable = GameObject.find<InteractOnTrigger>().gameObject;
        //interactables.Add(interactable);

        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allGameObjects)
        {
            // Check if the GameObject has the specified script attached
            if (go.GetComponent("InteractOnTrigger") != null && !go.transform.parent.name.Contains("Example"))
            {
                // Add the GameObject to the list
                interactables.Add(go);
            }
        }
    }


    public void OnSniffInteraction(InteractableType type, GameObject go)
    {
        string message = "Type: " + type + " Sender: " + go;

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
            default:
                break;
        }
    }

    public void SayTest()
    {
        Debug.Log("test function");
    }

}
