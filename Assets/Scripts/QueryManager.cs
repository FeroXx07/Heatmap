using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

public class QueryManager : MonoBehaviour
{
    public bool showDebugLogs = false;
    public bool useLocalHost = false; // Using Xampp's apache to create a localhost server and be able to debug PHP scripts in VS Code using XDebug (PHP debugger kernel)
    public string globalAPI = "https://citmalumnes.upc.es/~brandonam/getQuery.php/POST";
    public string localHostAPI = "http://localhost/delivery3/getQuery.php";

    public string predefinedQueryOne = "SELECT PositionX, PositionZ, SUM(Damage) AS TotalDamageInPosition," +
                                       " SUM(Damage) / MAX(SUM(Damage)) OVER() AS NormalizedValue FROM Hit GROUP BY PositionX, PositionZ;";

    private class QueryStructureOne
    {
        public void InsertData(int x, int z, float v)
        {
            PositionX.Add(x);
            PositionZ.Add(z);
            NormalizedValue.Add(v);
        }
        
        public List<int> PositionX = new List<int>();
        public List<int> PositionZ = new List<int>();
        public List<float> NormalizedValue = new List<float>();
    }
    private void Start()
    {
        SendQuery(useLocalHost ? localHostAPI : globalAPI, predefinedQueryOne, OnQueryDone);
    }

    private void OnQueryDone(string result)
    {
        QueryStructureOne queryStructureOne = new QueryStructureOne();
        // Split the received string into lines
        string[] rows = result.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        // Parse the received lines into arrays
        foreach (string row in rows)
        {
            string[] rowData = row.Split('\n');
            
            int posX = int.Parse(rowData[0].Split(':')[1]);
            int posZ = int.Parse(rowData[1].Split(':')[1]);
            // Parse total damage and normalized damage
            float totalDamage = float.Parse(rowData[2].Split(':')[1]);
            float.TryParse(rowData[3].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                out float parseResult);
            float normalizedDamage = parseResult;
            queryStructureOne.InsertData(posX, posZ, normalizedDamage);
        }

        // Display retrieved data (for demonstration)
        Debug.Log("Positions:");
        for (int i = 0; i < queryStructureOne.PositionX.Count; i++)
        {
            Debug.Log($"Position X: {queryStructureOne.PositionX[i]}, Position Z: {queryStructureOne.PositionZ[i]}, Value: {queryStructureOne.NormalizedValue[i]}");
        }
    }

    private void SendQuery(string url, string query, Action<string> callback)
    {
        StartCoroutine(SendWebRequest(url, query, callback));
    }

    private IEnumerator SendWebRequest(string url, string query, Action<string> callback)
    {
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
            callback?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("HTTP Request failed with error: " + request.error);
        }
    }
}
