using System;
using System.Collections;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Networking;

namespace Server
{
    public class PHP_Sender : MonoBehaviour
    {
        public bool showDebugLogs = false;
        public bool useLocalHost = false; // Using Xampp's apache to create a localhost server and be able to debug PHP scripts in VS Code using XDebug (PHP debugger kernel)
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

            InitUser();
        }

        private void OnDisable()
        {
            instance = null;
        }
        #endregion
        
        #region Sender
        private const string apiUrl = "https://citmalumnes.upc.es/~brandonam/addData.php/POST";
        private const string localHostUrl = "http://localhost/delivery3/addUser.php";
        
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
                // uint newId = uint.Parse(string.Concat(request.downloadHandler.text.Where(Char.IsDigit)));
                // callback?.Invoke(newId);
            }
            else
            {
                Debug.LogError("HTTP Request failed with error: " + request.error);
            }
        }
        #endregion

        #region User
        public UserSO user;
        public User userData;

        private void InitUser()
        {
            userData.Id = UInt64.MaxValue;
            userData.Name = user.Name;
            userData.Sex = user.Sex;
            userData.Age = user.Age;
            userData.Country = user.Country;
            userData.DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            string json = userData.ToJson();
            Debug.LogWarning(json);

            SendData(useLocalHost ? localHostUrl : apiUrl, json, null);
        }
        #endregion
    }
}

