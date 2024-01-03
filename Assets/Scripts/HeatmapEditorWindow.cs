using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class HeatmapEditorWindow : EditorWindow
{
    private readonly string[] _queryTypes = { "DamagePositionNormalized", "PlayerDamagePositionNormalized", "EnemyDamagePositionNormalized" };
    private int _selectedQueryTypeIndex = 0;
    private readonly string[] _granularityTypes = { "ROUND", "FLOOR"};
    private int _selectedGranularityTypeIndex = 0;
    private string _query = "";
    private Color _cubeColor = Color.red;

    private readonly string _localHostAPI ="http://localhost/delivery3/getQuery.php"; // Replace with your actual local API URL
    private readonly string _globalAPI = "https://citmalumnes.upc.es/~brandonam/getQuery.php/POST";
    private bool _useLocalHost = false; 
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
        
        _useLocalHost = EditorGUILayout.Toggle("Local Host:", _useLocalHost);
        
        GUILayout.Label("Query:", EditorStyles.boldLabel);
        
        // Granularity type dropdown
        _selectedGranularityTypeIndex = EditorGUILayout.Popup("Granularity Type:", _selectedGranularityTypeIndex, _granularityTypes);
        _queryHandeler.granularityType =(QueryHandeler.GranularityType) _selectedGranularityTypeIndex;
        
        // Query type dropdown
        _selectedQueryTypeIndex = EditorGUILayout.Popup("Query Type:", _selectedQueryTypeIndex, _queryTypes);
        _query = _queryHandeler.GetQueryType(_queryTypes[_selectedQueryTypeIndex]);
        
        // Cube color selection
        _cubeColor = EditorGUILayout.ColorField("Query Color:", _cubeColor);
        
        // disable button if query in progress
        EditorGUI.BeginDisabledGroup(_query != null && _webRequest != null && !_webRequest.isDone);
        // Send Query
        if (GUILayout.Button("Send Query"))
        {
            RequestQuery(_query);
        }
        EditorGUI.EndDisabledGroup();
    }
   
    public void RequestQuery(string query)
    {
        try
        {
            _webRequest = UnityWebRequest.PostWwwForm(_useLocalHost ? _localHostAPI : _globalAPI, query);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(query);
            _webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
            _webRequest.downloadHandler = new DownloadHandlerBuffer();
            _webRequest.SetRequestHeader("Content-Type", "application/json");
            _webRequest.SendWebRequest();
            OnQueryRequested?.Invoke(query);
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
            OnQueryFailed?.Invoke(_query);
        }
        else
        {
            Debug.Log($"Response for {_query}: {_webRequest.downloadHandler.text}");
            // Handle the response
            OnQueryDone?.Invoke(_query);
        }

        // Cleanup
        _webRequest.Dispose();
        _webRequest = null;

        EditorApplication.update -= EditorUpdate;
    }
}
