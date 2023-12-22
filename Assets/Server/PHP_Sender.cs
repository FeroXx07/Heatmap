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
    string apiUrl = "https://citmalumnes.upc.es/~allenbn/";
    //public void SendData(User user, Action<uint> callback)
    //{
    //    string json = JsonUtility.ToJson(user);
    //    StartCoroutine(SendWebRequest(apiUrl, "addUsers.php", json, callback));
    //}
    //public void SendData(Session user, Action<uint> callback)
    //{
    //    string json = JsonUtility.ToJson(user);
    //    StartCoroutine(SendWebRequest(apiUrl, "addSession.php", json, callback));
    //}
    //public void SendData(Monetization user, Action<uint> callback)
    //{
    //    string json = JsonUtility.ToJson(user);
    //    StartCoroutine(SendWebRequest(apiUrl, "addMonetization.php", json, callback));
    //}

    public IEnumerator SendWebRequest(string url, string file, string json, Action<uint> callback)
    {
        UnityWebRequest request = new UnityWebRequest(url + file, "POST");
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        //Debug.Log("Json is -->    " + json.ToString());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log($"{file} data sent successfully!");
            //Debug.Log("Response: \n" + request.downloadHandler.text);
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
