using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class HeatmapEditorWindow : EditorWindow
{
    #region Query properties
    private readonly string[] _queryTypes = { "DamagePositionNormalized", "PlayerDamagePositionNormalized", "EnemyDamagePositionNormalized" };
    private int _selectedQueryTypeIndex = 0;
    private readonly string[] _granularityTypes = { "ROUND", "FLOOR"};
    private int _selectedGranularityTypeIndex = 0;
    private string _query = "";
    
    private readonly string _localHostAPI ="http://localhost/delivery3/getQuery.php";
    private readonly string _globalAPI = "https://citmalumnes.upc.es/~brandonam/getQuery.php/POST";
    private bool _useLocalHost = false; 
    private UnityWebRequest _webRequest;

    public delegate void QueryEvent(string query);
    public static event QueryEvent OnQueryRequested;
    public static event QueryEvent OnQueryDone;
    public static event QueryEvent OnQueryFailed;
    
    private int _selectedQueryIndex = 0;
    private string[] _availableQueries;
    
    #endregion

    #region  draw properties

    enum HeatmapType
    {
        SHADER,
        CUBES
    }

    private HeatmapType _heatmapType;
    private Color _minColor;
    private Color _maxColor;
    private Material _material;
    
    private GameObject _prefab;
    private float _objectSize;
    #endregion    
    
    private QueryHandeler _queryHandler = new QueryHandeler();
    private HeatmapDrawer _heatmapDrawer = new HeatmapDrawer();
    private QueryStructureOne _currentQueryStructure;

    [MenuItem("Tools/Heatmap Editor")]
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
        _queryHandler.granularityType =(QueryHandeler.GranularityType) _selectedGranularityTypeIndex;
        
        // Query type dropdown
        _selectedQueryTypeIndex = EditorGUILayout.Popup("Query Type:", _selectedQueryTypeIndex, _queryTypes);
        _query = _queryHandler.GetQueryType(_queryTypes[_selectedQueryTypeIndex]);
        
        // disable button if query in progress
        EditorGUI.BeginDisabledGroup(_query != null && _webRequest != null && !_webRequest.isDone);
        // Send Query
        if (GUILayout.Button("Send Query"))
        {
            RequestQuery(_query);
        }
        EditorGUI.EndDisabledGroup();
        
        GUILayout.Label("Display options:", EditorStyles.boldLabel);
        
        _heatmapType = (HeatmapType) EditorGUILayout.EnumPopup("Heatmap type",_heatmapType);
        
        DisplayHeatMapTypes(_heatmapType);
    }

    void DisplayHeatMapTypes(HeatmapType type)
    {
        switch (type)
        {
            case HeatmapType.SHADER:
                _minColor = EditorGUILayout.ColorField("Min Color", _minColor);
                _maxColor = EditorGUILayout.ColorField("Max Color", _maxColor);
                _material = EditorGUILayout.ObjectField("Shader Material", _material, typeof(Material), false) as Material;
                break;
            case HeatmapType.CUBES:
                _prefab = EditorGUILayout.ObjectField("Prefab:",_prefab,typeof(GameObject),false)as GameObject;
                _objectSize = EditorGUILayout.FloatField("Size", _objectSize);
                break;
        }
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
            OnQueryFailed?.Invoke(_webRequest.error);
        }
        else
        {
            Debug.Log($"Response for {_query}: {_webRequest.downloadHandler.text}");
            OnQueryDone?.Invoke(_webRequest.downloadHandler.text);
        }

        _webRequest.Dispose();
        _webRequest = null;

        EditorApplication.update -= EditorUpdate;
    }

    private void OnEnable()
    {
        OnQueryDone += _queryHandler.ProcessQueryReceived;
    }

    private void OnDisable()
    {
        OnQueryDone -= _queryHandler.ProcessQueryReceived;
    }

    void DisplayData()
    {
        
    }
}
