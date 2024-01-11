using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Serialization;

public class HeatmapEditorWindow : EditorWindow
{
    #region Query properties

    private readonly string[] _queryTypes =
    {
        "DamagePositionNormalized", "PlayerDamagePositionNormalized", "EnemyDamagePositionNormalized", "Interaction"
    };

    private readonly string[] _interactionTypes =
    {
        "NONE",
        "GET_KEY",
        "USE_KEY",
        "BREAK_BOX",
        "OPEN_DOOR",
        "SWITCH",
        "PRESSURE_PLATE",
        "GET_TREASURE",
        "HEAL",
        "SHOW_INFO_TEXT",
        "DAMAGE_BOX",
        "FINISH"
    };
  
    private int _selectedQueryTypeIndex = 0;
    private readonly string[] _granularityTypes = { "ROUND", "FLOOR" };
    private int _selectedGranularityTypeIndex = 0;
    private string _query = "";
    private readonly string _localHostAPI = "http://localhost/delivery3/getQuery.php";
    private readonly string _globalAPI = "https://citmalumnes.upc.es/~brandonam/getQuery.php/POST";
    private bool _useLocalHost = false;
    private UnityWebRequest _webRequest;
    public static Action<string, string> OnQueryRequested;
    public static Action<string, uint> OnQueryDone;
    public static Action<string, uint> OnQueryFailed;
    private int _selectedInteractionTypeIndex = 0;
    #endregion

    #region  Filters
    private readonly string[] _countries =
    {
        "All", "Spain", "Portugal", "France", "Bulgaria", "USA" //insertar al gusto
    };

    private readonly string[] _sex =
    {
        "All", "Male", "Female"
    };

    private int _selectedCountryTypeIndex = 0;
    private int _selectedMinAge = 1;
    private int _selectedMaxAge = 99;
    private int _selectedSex = 0;
    private int _selectedMinTime = 0;
    private int _selectedMaxTime = 200;
    #endregion

    #region draw properties

    public enum QueryType
    {
        SHADER,
        CUBES,
        PATH
    }


    [SerializeField] private QueryType queryType;
    [SerializeField] private Gradient _gradient = new Gradient();
    [SerializeField] private float _intensity = 1.0f;
    [SerializeField] private GameObject _prefabCube;
    [SerializeField] private GameObject _prefabPath;
    private float _objectSize;

    private List<UInt64> _playerIds = new();
    private static UInt64 _selectedPlayerId = UInt64.MaxValue;
    private List<UInt64> _sessionIds = new();
    private static UInt64 _selectedSessionId = UInt64.MaxValue;
    public bool playerQuerying = false;
    public bool sessionQuerying = false;
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
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 1.0f;
        alphaKeys[1].time = 1.0f;
        
        _gradient.alphaKeys = alphaKeys;
        
        GradientColorKey[] gradientKeys = new GradientColorKey[5];
        gradientKeys[0].color =Color.white;
        gradientKeys[0].time = 0.00f;
        gradientKeys[1].color =Color.green;
        gradientKeys[1].time = 0.25f;
        gradientKeys[2].color = Color.yellow;
        gradientKeys[2].time = 0.5f;
        gradientKeys[3].color = new Color(1,0.5399091f, 0.0235849f, 1);
        gradientKeys[3].time = 0.75f;
        gradientKeys[4].color = Color.red;
        gradientKeys[4].time = 1.0f;
        
        _gradient.colorKeys = gradientKeys;
    }
    Color HexToColor(string hex)
    {
        Color color = new Color();
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
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
        queryType = (QueryType)EditorGUILayout.EnumPopup("Heatmap type", queryType);
        
        GUILayout.Label("Display options:", EditorStyles.boldLabel);
        DisplayHeatMapTypes(queryType);
        
        // Queries available popup
        List<QueryDataStructure> listQueries = _queryHandler.GetQueryList();
        bool hasData = listQueries.Count > 0 ? true : false;
        
        GUILayout.Label($"Queries: {listQueries.Count}", EditorStyles.boldLabel);
        if (hasData)
        {
            List<string> availableQueries = new List<string>();
            foreach (var q in listQueries) availableQueries.Add($"{q.name}_{q.id}");
            
            int currentIndex = _selectedQueryIndex;
            
            _selectedQueryIndex = EditorGUILayout.Popup("Available queries to draw:", _selectedQueryIndex,
                availableQueries.ToArray());
            
            if (currentIndex != _selectedQueryIndex)
            {
                _currentQueryDataStructure = listQueries.ElementAt(_selectedQueryIndex);
                GUILayout.Label($"Select query to draw: {_currentQueryDataStructure}", EditorStyles.boldLabel); 
                DrawQuery(_currentQueryDataStructure);
            }
        }
        
        if (queryType == QueryType.CUBES || queryType == QueryType.SHADER)
        {
            GUILayout.Label("Query:", EditorStyles.boldLabel);

            // Granularity type dropdown
            _selectedGranularityTypeIndex =
                EditorGUILayout.Popup("Granularity Type:", _selectedGranularityTypeIndex, _granularityTypes);
            _queryHandler.granularityType = (QueryHandeler.GranularityType)_selectedGranularityTypeIndex;

            // Query type dropdown
            _selectedQueryTypeIndex = EditorGUILayout.Popup("Query Type:", _selectedQueryTypeIndex, _queryTypes);
            _query = _queryHandler.GetFinalQuery(_queryTypes[_selectedQueryTypeIndex], _selectedMinAge, _selectedMaxAge, _selectedMinTime, _selectedMaxTime,_countries[_selectedCountryTypeIndex], _sex[_selectedSex]);

            // Interaction type dropdown
            if (_queryTypes[_selectedQueryTypeIndex] == "Interaction")
                _selectedInteractionTypeIndex = EditorGUILayout.Popup("Interaction Type:", _selectedInteractionTypeIndex, _interactionTypes);

            DrawFilterOptions();    
            
            // disable button if query in progress
            EditorGUI.BeginDisabledGroup(_query != null && _webRequest != null && !_webRequest.isDone);

            // Send Query
            if (GUILayout.Button("Send query") && _webRequest == null)
            {
                Debug.Log($"HeatmapEditorWindow: Button pressed: Send query");
                _currentHttpRequestCoroutine =
                    EditorCoroutineUtility.StartCoroutineOwnerless(RequestQuery(_query,
                        _queryTypes[_selectedQueryTypeIndex], queryType));
                _currentProcessedQueryType = _selectedQueryTypeIndex;
            }

            EditorGUI.EndDisabledGroup();
        }
        else
        {
            DrawPathDisplay();
        }
        
        EditorGUI.BeginDisabledGroup(!hasData);
        if (GUILayout.Button("Clear selected query"))
        {
            Debug.Log($"HeatmapEditorWindow: Button pressed: Clear selected query");
            var data = listQueries.ElementAt(_selectedQueryIndex);
            if (data.type == QueryType.CUBES) _heatmapDrawer.RemoveHeatmapCube(data);
            else if (data.type == QueryType.PATH) _heatmapDrawer.RemovePath(data);
            _queryHandler.ClearQuery(data);
        }

        if (GUILayout.Button("Clear queries"))
        {
            Debug.Log($"HeatmapEditorWindow: Button pressed: Clear queries");
            _heatmapDrawer.RemoveAllHeatMapCubes();
            _heatmapDrawer.RemoveAllHeatMapPaths();
            _queryHandler.ClearQueryList();
        }

        EditorGUI.EndDisabledGroup();
        if (_queryHandler.GetQueryList().Count > 0)
        {
            var data = listQueries.ElementAt(_selectedQueryIndex);
            if (data.type == QueryType.CUBES) DisplayHeatMapDrawSlider(listQueries);
        }
    }

    private void DrawFilterOptions()
    {
        GUILayout.Label("Filter options:", EditorStyles.boldLabel);
        float min = _selectedMinAge, max = _selectedMaxAge;
        EditorGUILayout.MinMaxSlider(ref min, ref max, 1, 99);
        _selectedMinAge = (int)MathF.Round(min);
        _selectedMaxAge = (int)MathF.Round(max);
        EditorGUILayout.IntField("Min Age",_selectedMinAge);
        EditorGUILayout.IntField("Max Age",_selectedMaxAge);
        _selectedCountryTypeIndex = EditorGUILayout.Popup("Country:", _selectedCountryTypeIndex, _countries);
        _selectedSex = EditorGUILayout.Popup("Sex:", _selectedSex, _sex);
        float minTime = _selectedMinTime, maxTime = _selectedMaxTime;
        EditorGUILayout.MinMaxSlider(ref minTime, ref maxTime, 0, 500);
        _selectedMinTime = (int)MathF.Round(minTime);
        _selectedMaxTime = (int)MathF.Round(maxTime);
        EditorGUILayout.FloatField("Min Time (Seconds)", _selectedMinTime);
        EditorGUILayout.FloatField("Max Time (Seconds)", _selectedMaxTime);
    }

    private void DrawPathDisplay()
    {
         if (_playerIds.Count == 0)
         {
             string query = "SELECT Id FROM User;";
             if (playerQuerying == false)
             {
                 _currentHttpRequestCoroutine =
                     EditorCoroutineUtility.StartCoroutineOwnerless(RequestQuery(query, QueryType.PATH.ToString(), QueryType.PATH, false));
                 playerQuerying = true;
             }
         }
         else
         {
             if (GUILayout.Button("Select player ID"))
             {
                 // create the menu and add items to it
                 GenericMenu menu = new GenericMenu();
                 foreach (ulong playerId in _playerIds)
                 {
                     menu.AddItem(new GUIContent(playerId.ToString()), playerId.Equals(_selectedPlayerId), () =>
                     {
                         _selectedPlayerId = playerId;
                         _sessionIds.Clear();
                         Debug.Log($"HeatmapEditorWindow: Selected playerId {_selectedPlayerId}");
                     });
                 }
                 // display the menu
                 menu.ShowAsContext();
             }
         }
            
         if (_selectedPlayerId == UInt64.MaxValue)
             return;
            
         if (_sessionIds.Count == 0)
         {
             _selectedSessionId = UInt64.MaxValue;
             string query = $"SELECT SessionId FROM Session WHERE PlayerId = {_selectedPlayerId};";
             if (sessionQuerying == false)
             {
                 _currentHttpRequestCoroutine =
                     EditorCoroutineUtility.StartCoroutineOwnerless(RequestQuery(query, QueryType.PATH.ToString(), QueryType.PATH, false));
                 sessionQuerying = true;
             }
         }
         else
         {
             if (GUILayout.Button("Select session ID"))
             {
                 // create the menu and add items to it
                 GenericMenu menu = new GenericMenu();
                 foreach (ulong sessionId in _sessionIds)
                 {
                     menu.AddItem(new GUIContent(sessionId.ToString()), sessionId.Equals(_selectedSessionId), () =>
                     {
                         _selectedSessionId = sessionId;
                         Debug.Log($"HeatmapEditorWindow: Selected sessionId {_selectedSessionId}");
                     });
                 }
                 // display the menu
                 menu.ShowAsContext();
             }
         }

         if (_selectedPlayerId != UInt64.MaxValue && _selectedSessionId != UInt64.MaxValue)
         {
             EditorGUI.BeginDisabledGroup(_webRequest != null && !_webRequest.isDone);
             if (GUILayout.Button("Send path query") && _webRequest == null)
             {
                 Debug.Log($"HeatmapEditorWindow: Button pressed: Send query");
                 string query = $"SELECT PositionX, PositionY, PositionZ FROM Movement WHERE SessionId = {_selectedSessionId};";
                 _currentHttpRequestCoroutine =
                     EditorCoroutineUtility.StartCoroutineOwnerless(RequestQuery(query, QueryType.PATH.ToString(), QueryType.PATH, true));
             }
             EditorGUI.EndDisabledGroup();
         }
    }

    private void DisplayHeatMapDrawSlider(List<QueryDataStructure> listQueries)
    {
        if (listQueries.Count <= 0) return;
        var selectedQueryDataStructure = listQueries.ElementAt(_selectedQueryIndex);
        HeatmapCube hc = _heatmapDrawer.GetHeatMapCube(selectedQueryDataStructure);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Intensity");
        hc.Intensity = EditorGUILayout.Slider(hc.Intensity, 1.0f, 10.0f);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Y Offset");
        hc.YOffSet = EditorGUILayout.Slider(hc.YOffSet, 0.0f, 3.0f);
        EditorGUILayout.EndHorizontal();
    }

    void DisplayHeatMapTypes(QueryType type)
    {
        switch (type)
        {
            case QueryType.SHADER:
                _gradient = EditorGUILayout.GradientField("Gradient", _gradient);
                _intensity = EditorGUILayout.FloatField("Size", _intensity);
                break;
            case QueryType.CUBES:
                _gradient = EditorGUILayout.GradientField("Gradient", _gradient);
                _prefabCube = EditorGUILayout.ObjectField("Prefab:", _prefabCube, typeof(GameObject), false) as GameObject;
                _objectSize = EditorGUILayout.FloatField("Size", _objectSize);
                break;
            case QueryType.PATH:
                _prefabPath = EditorGUILayout.ObjectField("Prefab path:", _prefabPath, typeof(GameObject), false) as GameObject;
                break;
        }
    }

    private IEnumerator RequestQuery(string query, string name, QueryType type, bool saveInList = true)
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
            if (saveInList) _currentProccesedQueryId = _queryHandler.SaveNewQuery(name, type);
            Debug.Log($"Response for {query}: {_webRequest.downloadHandler.text}");
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
        if (playerQuerying)
        {
            playerQuerying = false;
            _playerIds.Clear();
            _sessionIds.Clear();
            // Split the received string into lines
            string[] rows = result.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            // Parse the received lines into arrays
            foreach (string row in rows)
            {
                string[] rowData = row.Split('\n');
                UInt64.TryParse(rowData[0].Split(':')[1], out ulong data);
                _playerIds.Add(data);
            }
            return;
        }
        if (sessionQuerying)
        {
            sessionQuerying = false;
            _sessionIds.Clear();
            // Split the received string into lines
            string[] rows = result.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            // Parse the received lines into arrays
            foreach (string row in rows)
            {
                string[] rowData = row.Split('\n');
                UInt64.TryParse(rowData[0].Split(':')[1], out ulong data);
                _sessionIds.Add(data);
            }

            return;
        }
        
        QueryDataStructure q = _queryHandler.ProcessQueryReceived(result, id); 
        DrawQuery(q);
    }

    void DrawQuery(QueryDataStructure q)
    {
                switch (queryType)
        {
            case QueryType.CUBES:
            {
                _heatmapDrawer.CreateHeatmapCube(q, _gradient, _prefabCube, 1.0f);
            }
                break;
            case QueryType.SHADER:
            {
                _heatmapDrawer.CreateHeatmapShader(q, _gradient, _intensity);
            }
                break;
            case QueryType.PATH:
            {
               PathDataStructure p = q as PathDataStructure;
               p.playerId = _selectedPlayerId;
               p.sessionId = _selectedSessionId;
               _heatmapDrawer.CreatHeatmapPath(q, Color.green, _prefabPath);
            }
                break;
        }
        
    }
    private void OnEnable()
    {
        if (_queryHandler == null) _queryHandler = new();
        if (_heatmapDrawer == null) _heatmapDrawer = new();
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