using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows;

public class PHP_Sender : MonoBehaviour
{
    #region Singleton
    public static PHP_Sender instance { get; private set; }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void OnDisable()
    {
        instance = null;
    }
    #endregion

    #region Sender
    string apiUrl = "https://citmalumnes.upc.es/~brandonam/addData.php/POST";

    public void SendData(string jsonData, Action<uint> callback)
    {
        StartCoroutine(SendWebRequest(apiUrl, jsonData, callback));
    }
    public IEnumerator SendWebRequest(string url, string json, Action<uint> callback)
    {
        UnityWebRequest request = new UnityWebRequest(apiUrl);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        //Debug.Log("Json is -->    " + json.ToString());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log($"{file} data sent successfully!");
            Debug.Log("Response: \n" + request.downloadHandler.text);
            uint newId = uint.Parse(string.Concat(request.downloadHandler.text.Where(Char.IsDigit)));
            callback?.Invoke(newId);
        }
        else
        {
            Debug.LogError("HTTP Request failed with error: " + request.error);
        }
    }
    #endregion
}

[System.Serializable]
public struct Hit
{
   public UInt64 Id;
   public UInt64 SessionId;
   public int PostionX;
   public int PostionY;
   public int PostionZ;
   public string TimeStamp;
   public string AttackType;
   public int Damage;
   public string Hitter;
   public string Hitted;
   public int SourcePositionX;
   public int SourcePositionY;
   public int SourcePositionZ;
}

[System.Serializable]
public struct Death
{
    public UInt64 Id;
    public UInt64 SessionId;
    public int PostionX;
    public int PostionY;
    public int PostionZ;
    public string TimeStamp;
    public string DeathType;
}

[System.Serializable]
public struct Interaction
{
    public UInt64 Id;
    public UInt64 SessionId;
    public int PostionX;
    public int PostionY;
    public int PostionZ;
    public string TimeStamp;
    public string interactionType;
}

[System.Serializable]
public struct Kill
{
   public UInt64 Id;
   public UInt64 SessionId;
   public int PostionX;
   public int PostionY;
   public int PostionZ;
   public string TimeStamp;
   public string enemyType;
}

[System.Serializable]
public struct Movement
{
   public UInt64 Id;
   public UInt64 SessionId;
   public int PostionX;
   public int PostionY;
   public int PostionZ;
   public string TimeStamp;
}