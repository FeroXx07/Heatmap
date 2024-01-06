using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;

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
    public static Action<string, string> OnQueryRequested;
    public static Action<string, uint> OnQueryDone;
    public static Action<string, uint> OnQueryFailed;
    #endregion

    #region  draw properties

    enum HeatmapType
    {
        SHADER,
        CUBES
    }

    [SerializeField] private HeatmapType _heatmapType;
    [SerializeField] private Color _minColor;
    [SerializeField] private Color _maxColor;
    [SerializeField] private Material _material;
    [SerializeField] private Gradient _gradient = new Gradient();
    
    [SerializeField] private GameObject _prefab;
    private float _objectSize;
    #endregion    
    
    private QueryHandeler _queryHandler;
    private HeatmapDrawer _heatmapDrawer;
    
    private QueryDataStructure _currentQueryDataStructure;
    private int _selectedQueryIndex = 0;
    private EditorCoroutine _currentHttpRequestCoroutine = null;
    
    private int _currentProcessedQueryType = 0;
    private uint _currentProccesedQueryId = UInt32.MaxValue;

    private void Awake()
    {
        _queryHandler = new();
        _heatmapDrawer = new();
    }

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
        
        // Queries available popup
        List<QueryDataStructure> listQueries = _queryHandler.GetQueryList();
        bool hasData = listQueries.Count > 0 ? true : false;
        GUILayout.Label($"Queries: {listQueries.Count}", EditorStyles.boldLabel);
        if (hasData)
        {
            List<string> availableQueries = new List<string>();
            
            foreach (var q in listQueries)
                availableQueries.Add($"{q.name}_{q.id}");
            
            _selectedQueryIndex = EditorGUILayout.Popup("Available queries to draw:", _selectedQueryIndex, availableQueries.ToArray());
            _currentQueryDataStructure = listQueries.ElementAt(_selectedQueryIndex);
            GUILayout.Label($"Select query to draw: {_currentQueryDataStructure}", EditorStyles.boldLabel);
        }
        
        // disable button if query in progress
        EditorGUI.BeginDisabledGroup(_query != null && _webRequest != null && !_webRequest.isDone);
        
        // Send Query
        if (GUILayout.Button("Send query") && _webRequest == null)
        {
            Debug.Log($"HeatmapEditorWindow: Button pressed: Send query");
            _currentHttpRequestCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(RequestQuery(_query, _queryTypes[_selectedQueryTypeIndex]));
            _currentProcessedQueryType = _selectedQueryTypeIndex;
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUI.BeginDisabledGroup(!hasData);
        if (GUILayout.Button("Clear selected query"))
        {
            Debug.Log($"HeatmapEditorWindow: Button pressed: Clear selected query");
            var data = listQueries.ElementAt(_selectedQueryIndex);
            
            _heatmapDrawer.RemoveHeatmapCube(data);
            _queryHandler.ClearQuery(data);
        }

        if (GUILayout.Button("Clear queries"))
        {
            Debug.Log($"HeatmapEditorWindow: Button pressed: Clear queries");
            _heatmapDrawer.RemoveAllHeatMapCubes();
            _queryHandler.ClearQueryList();
        }
        EditorGUI.EndDisabledGroup();
        //DisplayHeatMapDrawSlider(listQueries);
        
       
        
        GUILayout.Label("Display options:", EditorStyles.boldLabel);
        
        _heatmapType = (HeatmapType) EditorGUILayout.EnumPopup("Heatmap type",_heatmapType);
        
        DisplayHeatMapTypes(_heatmapType);
    }

    private void DisplayHeatMapDrawSlider(List<QueryDataStructure> listQueries)
    {
        if (listQueries.Count <= 0) return;
        var selectedQueryDataStructure = listQueries.ElementAt(_selectedQueryIndex);
        HeatmapCube hc = _heatmapDrawer.GetHeatMapCube(selectedQueryDataStructure);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Intensity");
        hc.Intensity  = EditorGUILayout.Slider(hc.Intensity, 1.0f, 10.0f);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Y Offset");
        hc.YOffSet  = EditorGUILayout.Slider(hc.YOffSet, 0.0f, 3.0f);
        EditorGUILayout.EndHorizontal();
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
                _gradient = EditorGUILayout.GradientField("Gradient", _gradient);
                _prefab = EditorGUILayout.ObjectField("Prefab:",_prefab,typeof(GameObject),false)as GameObject;
                _objectSize = EditorGUILayout.FloatField("Size", _objectSize);
                break;
        }
    }

    private IEnumerator RequestQuery(string query, string name)
    {
        Debug.Log($"HeatmapEditorWindow: Requesting query {name}");
        
        OnQueryRequested?.Invoke(query, name);
        _webRequest = UnityWebRequest.PostWwwForm(_useLocalHost ? _localHostAPI : _globalAPI, query);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(query);
        _webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
        _webRequest.downloadHandler = new DownloadHandlerBuffer();
        _webRequest.SetRequestHeader("Content-Type", "application/json");
        
        yield return _webRequest.SendWebRequest();
        
        if (_webRequest.result == UnityWebRequest.Result.Success)
        {
            _currentProccesedQueryId = _queryHandler.SaveNewQuery(name);
            Debug.Log($"Response for {_query}: {_webRequest.downloadHandler.text}");
            OnQueryDone?.Invoke(_webRequest.downloadHandler.text, _currentProccesedQueryId);
        }
        else
        {
            Debug.LogError($"HTTP Request failed with error: {_webRequest.error}");
            OnQueryFailed?.Invoke(_webRequest.error, _currentProccesedQueryId);
        }
        
        _webRequest.Dispose();
        _webRequest = null;
        //EditorApplication.update += EditorUpdate;
       
    }

    private void QueryDone(string result, uint id)
    {
        Debug.Log($"HeatmapEditorWindow: QueryDone Id: {id}");
        QueryDataStructure q = _queryHandler.ProcessQueryReceived(result, id);
        switch (_heatmapType)
        {
            case HeatmapType.CUBES:
                _heatmapDrawer.CreateHeatmapCube(q, _gradient, _prefab, 1.0f);
                break;
            case HeatmapType.SHADER:
                if(_material != null)
                    _heatmapDrawer.CreateHeatmapShader(q,_material,_gradient,1.0f);
                else
                {
                    Debug.Log("material not set in the editor");
                }
                break;
        }
        
    }

    private void EditorUpdate()
    {
        // if (_webRequest == null || !_webRequest.isDone)
        //     return;
        // Debug.Log($"HeatmapEditorWindow: Editor Update");
        // if (_webRequest.result == UnityWebRequest.Result.ConnectionError)
        // {
        //     Debug.LogError($"HTTP Request failed with error: {_webRequest.error}");
        //     OnQueryFailed?.Invoke(_webRequest.error, _currentProccesedQueryId);
        // }
        // else
        // {
        //     Debug.Log($"Response for {_query}: {_webRequest.downloadHandler.text}");
        //     OnQueryDone?.Invoke(_webRequest.downloadHandler.text, _currentProccesedQueryId);
        // }
        //
        // _webRequest.Dispose();
        // _webRequest = null;
        //
        // EditorApplication.update -= EditorUpdate;
    }

    private void OnEnable()
    {
        if (_queryHandler == null)
            _queryHandler = new();
        if (_heatmapDrawer == null)
            _heatmapDrawer = new();
        OnQueryDone += QueryDone;
        // EditorApplication.update += _heatmapDrawer.EditorUpdate;
    }

    private void OnDisable()
    {
        OnQueryDone -= QueryDone;
        // EditorApplication.update -= _heatmapDrawer.EditorUpdate;
        EditorCoroutineUtility.StopCoroutine(_currentHttpRequestCoroutine);
    }
    
    private void OnDestroy()
    {
        OnQueryDone -= QueryDone;
        // if (_heatmapDrawer)
        //     EditorApplication.update -= _heatmapDrawer.EditorUpdate;
        EditorCoroutineUtility.StopCoroutine(_currentHttpRequestCoroutine);
    }
}
