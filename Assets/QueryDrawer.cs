using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class QueryStructureOne
{
    public void InsertData(float x, float y, float z, float v)
    {
        Position.Add(new Vector3(x, y, z));
        NormalizedValue.Add(v);
    }
        
    public List<Vector3> Position = new List<Vector3>();
    public List<float> NormalizedValue = new List<float>();
    public List<GameObject> Cube = new List<GameObject>();
}

public class QueryDrawer : MonoBehaviour
{
    public GameObject container;
    public GameObject heatMapPrefab;
    public Gradient gradient; 
    
    [Range(0.0f, 2.0f)] public float yOffset = 0.0f;
    [Range(1.0f, 10.0f)] public float intensity = 1.0f;
    private float previousIntensityValue;

    private float scale = 1.0f;
    public float Scale
    {
        get => scale;
        set
        {
            scale = value;
            SetSize(currentQueryStructure, scale);
        }
    }

    private MaterialPropertyBlock propertyBlock;

    private QueryStructureOne currentQueryStructure;
    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        container = new GameObject("HeatMapContainer");
    }

    private void OnEnable()
    {
        QueryManager queryManager = GetComponent<QueryManager>();
        queryManager.OnQueryDone.AddListener(OnQueryDone);
        queryManager.OnQueryRequested.AddListener(ClearHeatMap);
    }

    private void OnDisable()
    {
        QueryManager queryManager = GetComponent<QueryManager>();
        queryManager.OnQueryDone.RemoveListener(OnQueryDone);
        queryManager.OnQueryRequested.RemoveListener(ClearHeatMap);
    }
    
    private void OnQueryDone(string result)
    {
        currentQueryStructure = new QueryStructureOne();
        // Split the received string into lines
        string[] rows = result.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        // Parse the received lines into arrays
        foreach (string row in rows)
        {
            string[] rowData = row.Split('\n');
            
            float.TryParse(rowData[0].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                out float posX);
            float.TryParse(rowData[1].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                out float posY);
            float.TryParse(rowData[2].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                out float posZ);
            // Parse total damage and normalized damage
            float totalDamage = float.Parse(rowData[3].Split(':')[1]);
            float.TryParse(rowData[4].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                out float parseResult);
            float normalizedDamage = parseResult;
            currentQueryStructure.InsertData(posX, posY, posZ, normalizedDamage);
        }
    
        // Display retrieved data (for demonstration)
        Debug.Log("Positions:");
        for (int i = 0; i < currentQueryStructure.Position.Count; i++)
        {
            Debug.Log($"Position X: {currentQueryStructure.Position[i].x}, Position Y: {currentQueryStructure.Position[i].y}, Position Z: {currentQueryStructure.Position[i].z}, Value: {currentQueryStructure.NormalizedValue[i]}");
        }

        InstantiateHeatMap(currentQueryStructure);
    }

    void InstantiateHeatMap(QueryStructureOne queryStructureOne)
    {
        for (int i = 0; i < queryStructureOne.Position.Count; i++)
        {
            var newPos =  queryStructureOne.Position[i];
            newPos.y = yOffset;
            GameObject cube = Instantiate<GameObject>(heatMapPrefab, newPos, quaternion.identity, container.transform);
            cube.transform.localScale = new Vector3(scale, scale, scale);
            queryStructureOne.Cube.Add(cube);
        }

        SetColor(queryStructureOne);
    }

    void SetColor(QueryStructureOne queryStructureOne)
    {
        for (int i = 0; i < queryStructureOne.Cube.Count; i++)
        {
            Renderer cubeRenderer = queryStructureOne.Cube[i].GetComponent<Renderer>();
            cubeRenderer.GetPropertyBlock(propertyBlock);
            float calculateValue = Mathf.Clamp01(queryStructureOne.NormalizedValue[i] * intensity);
            Color color = gradient.Evaluate(calculateValue);
            propertyBlock.SetColor("_Color", color);
            cubeRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    void SetSize(QueryStructureOne queryStructureOne, float size)
    {
        if (queryStructureOne == null) return;

        foreach (var t in queryStructureOne.Cube)
        {
            t.transform.localScale = new Vector3(size, size, size);
        }
    }

    public void ClearHeatMap(string s)
    {
        for (int i = 0; i < container.transform.childCount; i++)
        {
            Destroy(container.transform.GetChild(i).gameObject);
        }
    }

    private void Update()
    {
        var newPos = container.transform.position;
        newPos.y = yOffset;
        container.transform.position = newPos;
    }
    
    private void OnValidate()
    {
        if (intensity != previousIntensityValue)
        {
            previousIntensityValue = intensity;

            if (currentQueryStructure != null)
                SetColor(currentQueryStructure);
        }
    }

    public void SetYOffset(float v)
    {
        yOffset = v;
    }
    
    public void SetIntensity(float v)
    {
        intensity = v;
        SetColor(currentQueryStructure);
    }
}
