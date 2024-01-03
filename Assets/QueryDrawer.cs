// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Globalization;
// using JetBrains.Annotations;
// using Unity.Mathematics;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// public class QueryDataStructure
// {
//     public QueryDataStructure(string name, uint id)
//     {
//         this.name = name;
//         this.id = id;
//     }
//     public void InsertData(float x, float y, float z, float v)
//     {
//         Position.Add(new Vector3(x, y, z));
//         NormalizedValue.Add(v);
//     }
//     
//     public uint id = UInt32.MaxValue;
//     public string name;
//     public List<Vector3> Position = new List<Vector3>();
//     public List<float> NormalizedValue = new List<float>();
// }
//
// public class QueryDrawer : MonoBehaviour
// {
//     public GameObject container;
//     public GameObject heatMapPrefab;
//     public Gradient gradient; 
//     
//     [Range(0.0f, 2.0f)] public float yOffset = 0.0f;
//     [Range(1.0f, 10.0f)] public float intensity = 1.0f;
//     private float previousIntensityValue;
//
//     private float scale = 1.0f;
//     public float Scale
//     {
//         get => scale;
//         set
//         {
//             scale = value;
//             SetSize(_currentQueryDataStructure, scale);
//         }
//     }
//
//     private MaterialPropertyBlock propertyBlock;
//
//     private QueryDataStructure _currentQueryDataStructure;
//     private void Awake()
//     {
//         propertyBlock = new MaterialPropertyBlock();
//         container = new GameObject("HeatMapContainer");
//     }
//
//     private void OnEnable()
//     {
//         QueryManager queryManager = GetComponent<QueryManager>();
//         queryManager.OnQueryDone.AddListener(OnQueryDone);
//         queryManager.OnQueryRequested.AddListener(ClearHeatMap);
//     }
//
//     private void OnDisable()
//     {
//         QueryManager queryManager = GetComponent<QueryManager>();
//         queryManager.OnQueryDone.RemoveListener(OnQueryDone);
//         queryManager.OnQueryRequested.RemoveListener(ClearHeatMap);
//     }
//     
//     private void OnQueryDone(string result)
//     {
//         _currentQueryDataStructure = new QueryDataStructure();
//         // Split the received string into lines
//         string[] rows = result.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
//         // Parse the received lines into arrays
//         foreach (string row in rows)
//         {
//             string[] rowData = row.Split('\n');
//             
//             float.TryParse(rowData[0].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
//                 out float posX);
//             float.TryParse(rowData[1].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
//                 out float posY);
//             float.TryParse(rowData[2].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
//                 out float posZ);
//             // Parse total damage and normalized damage
//             float totalDamage = float.Parse(rowData[3].Split(':')[1]);
//             float.TryParse(rowData[4].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
//                 out float parseResult);
//             float normalizedDamage = parseResult;
//             _currentQueryDataStructure.InsertData(posX, posY, posZ, normalizedDamage);
//         }
//     
//         // Display retrieved data (for demonstration)
//         Debug.Log("Positions:");
//         for (int i = 0; i < _currentQueryDataStructure.Position.Count; i++)
//         {
//             Debug.Log($"Position X: {_currentQueryDataStructure.Position[i].x}, Position Y: {_currentQueryDataStructure.Position[i].y}, Position Z: {_currentQueryDataStructure.Position[i].z}, Value: {_currentQueryDataStructure.NormalizedValue[i]}");
//         }
//
//         InstantiateHeatMap(_currentQueryDataStructure);
//     }
//
//     void InstantiateHeatMap(QueryDataStructure queryDataStructure)
//     {
//         for (int i = 0; i < queryDataStructure.Position.Count; i++)
//         {
//             var newPos =  queryDataStructure.Position[i];
//             newPos.y = yOffset;
//             GameObject cube = Instantiate<GameObject>(heatMapPrefab, newPos, quaternion.identity, container.transform);
//             cube.transform.localScale = new Vector3(scale, scale, scale);
//             //queryDataStructure.Cube.Add(cube);
//         }
//
//         SetColor(queryDataStructure);
//     }
//
//     void SetColor(QueryDataStructure queryDataStructure)
//     {
//         // for (int i = 0; i < queryDataStructure.Cube.Count; i++)
//         // {
//         //     Renderer cubeRenderer = queryDataStructure.Cube[i].GetComponent<Renderer>();
//         //     cubeRenderer.GetPropertyBlock(propertyBlock);
//         //     float calculateValue = Mathf.Clamp01(queryDataStructure.NormalizedValue[i] * intensity);
//         //     Color color = gradient.Evaluate(calculateValue);
//         //     propertyBlock.SetColor("_Color", color);
//         //     cubeRenderer.SetPropertyBlock(propertyBlock);
//         // }
//     }
//
//     void SetSize(QueryDataStructure queryDataStructure, float size)
//     {
//         if (queryDataStructure == null) return;
//
//         // foreach (var t in queryDataStructure.Cube)
//         // {
//         //     t.transform.localScale = new Vector3(size, size, size);
//         // }
//     }
//
//     public void ClearHeatMap(string s)
//     {
//         for (int i = 0; i < container.transform.childCount; i++)
//         {
//             Destroy(container.transform.GetChild(i).gameObject);
//         }
//     }
//
//     private void Update()
//     {
//         var newPos = container.transform.position;
//         newPos.y = yOffset;
//         container.transform.position = newPos;
//     }
//     
//     private void OnValidate()
//     {
//         if (intensity != previousIntensityValue)
//         {
//             previousIntensityValue = intensity;
//
//             if (_currentQueryDataStructure != null)
//                 SetColor(_currentQueryDataStructure);
//         }
//     }
//
//     public void SetYOffset(float v)
//     {
//         yOffset = v;
//     }
//     
//     public void SetIntensity(float v)
//     {
//         intensity = v;
//         SetColor(_currentQueryDataStructure);
//     }
// }
