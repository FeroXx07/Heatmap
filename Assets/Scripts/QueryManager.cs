using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class QueryManager : MonoBehaviour
{
    public UnityEvent<string> OnQueryRequested = new();
    public UnityEvent<string> OnQueryFailed = new();
    public UnityEvent<string> OnQueryDone = new();
    
    public bool showDebugLogs = false;
    public bool useLocalHost = false; // Using Xampp's apache to create a localhost server and be able to debug PHP scripts in VS Code using XDebug (PHP debugger kernel)
    public string globalAPI = "https://citmalumnes.upc.es/~brandonam/getQuery.php/POST";
    public string localHostAPI = "http://localhost/delivery3/getQuery.php";

    private void Start()
    {
        //SendQuery(useLocalHost ? localHostAPI : globalAPI, predefinedQueryOne);
    }

    public void RequestQuery(string query)
    {
        SendQuery(useLocalHost ? localHostAPI : globalAPI, query);
    }

    private void SendQuery(string url, string query)
    {
        StartCoroutine(SendWebRequest(url, query));
    }

    private IEnumerator SendWebRequest(string url, string query)
    {
        OnQueryRequested?.Invoke(query);
        
        UnityWebRequest request = new UnityWebRequest(url);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(query);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        //Debug.Log("Json is -->    " + json.ToString());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log($"{file} data sent successfully!");
            Debug.Log($"Response for {query}: {request.downloadHandler.text}");
            OnQueryDone?.Invoke(request.downloadHandler.text);
        }
        else
        {
            OnQueryFailed?.Invoke($"HTTP Request failed with error: {request.error}");
            Debug.LogError($"HTTP Request failed with error: {request.error}");
        }
    }
}
