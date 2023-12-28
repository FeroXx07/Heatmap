using System;
using System.Collections;
using System.Linq;
using _Scripts;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Networking;

namespace Server
{
    public class PHP_Sender : GenericSingleton<PHP_Sender>
    {
        public bool showDebugLogs = false;
        public bool useLocalHost = false; // Using Xampp's apache to create a localhost server and be able to debug PHP scripts in VS Code using XDebug (PHP debugger kernel)
        public string apiUrlAddData = "https://citmalumnes.upc.es/~brandonam/addData.php/POST";
        public string localHostUrlAddData = "http://localhost/delivery3/addData.php"; 
        #region Sender
        public void SendData(string url, string jsonData, Action<uint> callback)
        {
            StartCoroutine(SendWebRequest(url, jsonData, callback));
        }

        private IEnumerator SendWebRequest(string url, string json, Action<uint> callback)
        {
            UnityWebRequest request = new UnityWebRequest(url);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            //Debug.Log("Json is -->    " + json.ToString());

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log($"{file} data sent successfully!");
                Debug.Log($"Response for {json}: {request.downloadHandler.text}");
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
}

