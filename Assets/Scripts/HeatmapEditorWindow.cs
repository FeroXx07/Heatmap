using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class HeatmapEditorWindow : EditorWindow
{
    private string[] queryTypes = { "DamagePositionNormalized", "PlayerDamagePositionNormalized", "EnemyDamagePositionNormalized" };
    private int selectedQueryTypeIndex = 0;
    private string[] granularityTypes = { "ROUND", "FLOOR"};
    private int selectedGranularityTypeIndex = 0;
    private string query = "";
    private Color cubeColor = Color.red;

    private string localHostAPI ="http://localhost/delivery3/getQuery.php"; // Replace with your actual local API URL
    private string globalAPI = "https://citmalumnes.upc.es/~brandonam/getQuery.php/POST";
    private bool useLocalHost = false; 
    private UnityWebRequest _webRequest;

    public delegate void QueryEvent(string query);
    public static event QueryEvent OnQueryRequested;
    public static event QueryEvent OnQueryDone;
    public static event QueryEvent OnQueryFailed;
    
    private QueryHandeler _queryHandeler = new QueryHandeler();
    

    [MenuItem("Window/Heatmap Editor")]
    public static void ShowWindow()
    {
        GetWindow<HeatmapEditorWindow>("Heatmap Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Connection: ", EditorStyles.boldLabel);
        
        useLocalHost = EditorGUILayout.Toggle("Local Host:", useLocalHost);
        
        GUILayout.Label("Query:", EditorStyles.boldLabel);
        
        // Granularity type dropdown
        selectedGranularityTypeIndex = EditorGUILayout.Popup("Granularity Type:", selectedGranularityTypeIndex, granularityTypes);
        _queryHandeler.granularityType =(QueryHandeler.GranularityType) selectedGranularityTypeIndex;
        
        // Query type dropdown
        selectedQueryTypeIndex = EditorGUILayout.Popup("Query Type:", selectedQueryTypeIndex, queryTypes);
        query = _queryHandeler.GetQueryType(queryTypes[selectedQueryTypeIndex]);
        
        // Cube color selection
        cubeColor = EditorGUILayout.ColorField("Query Color:", cubeColor);
        
        // disable button if query in progress
        EditorGUI.BeginDisabledGroup(query != null && _webRequest != null && !_webRequest.isDone);
        // Send Query
        if (GUILayout.Button("Send Query"))
        {
            RequestQuery(query);
        }
        EditorGUI.EndDisabledGroup();
    }
   
    public void RequestQuery(string query)
    {
        try
        {
            _webRequest = UnityWebRequest.PostWwwForm(useLocalHost ? localHostAPI : globalAPI, query);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(query);
            _webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
            _webRequest.downloadHandler = new DownloadHandlerBuffer();
            _webRequest.SetRequestHeader("Content-Type", "application/json");
            _webRequest.SendWebRequest();
            EditorApplication.update += EditorUpdate;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void EditorUpdate()
    {
        if (!_webRequest.isDone)
            return;
        if (_webRequest.isNetworkError || _webRequest.isHttpError)
        {
            Debug.LogError($"HTTP Request failed with error: {_webRequest.error}");
        }
        else
        {
            Debug.Log($"Response for {query}: {_webRequest.downloadHandler.text}");
            // Handle the response
        }

        // Cleanup
        _webRequest.Dispose();
        _webRequest = null;

        EditorApplication.update -= EditorUpdate;
    }
}
