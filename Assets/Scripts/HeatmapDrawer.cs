using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

public class HeatmapCube : Object
{
    public HeatmapCube(QueryDataStructure data, Gradient gradient, GameObject prefab, float intensity = 1.0f)
    {
        rawData = data;
        this.gradient = gradient;
        this.intensity = intensity;
        
        Init(data, prefab);
    }

    private List<GameObject> Cube = new();
    public QueryDataStructure rawData;
    private float intensity = 1.0f;
    private Gradient gradient;

    private void Init(QueryDataStructure q, GameObject prefab)
    {
        rawData = q;
        InstantiateCubes(q, prefab);
        SetColor();
    }

    private void InstantiateCubes(QueryDataStructure q, GameObject prefab, float scale = 1)
    {
        GameObject container = new GameObject($"Container_{q.name}_{q.id}");
        for (int i = 0; i < q.Position.Count; i++)
        {
            var newPos =  q.Position[i];
            //newPos.y = yOffset;
            GameObject cube = Instantiate<GameObject>(prefab, newPos, quaternion.identity, container.transform);
            cube.transform.localScale = new Vector3(scale, scale, scale);
            //queryDataStructure.Cube.Add(cube);
        }
    }
    void SetColor()
     {
         for (int i = 0; i < Cube.Count; i++)
         {
             MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
             Renderer cubeRenderer = Cube[i].GetComponent<Renderer>();
             cubeRenderer.GetPropertyBlock(propertyBlock);
             float calculateValue = Mathf.Clamp01(rawData.NormalizedValue[i] * intensity);
             Color color = gradient.Evaluate(calculateValue);
             propertyBlock.SetColor("_Color", color);
             cubeRenderer.SetPropertyBlock(propertyBlock);
         }
     }
    public void EditorUpdate()
    {
        // Update Color (Intensity)
        // Update Size
    }
}

public class HeatmapDrawer : Object
{
    private List<HeatmapCube> _heatmapCubes = new();
    public void CreateHeatmapCube(QueryDataStructure data, Gradient gradient, GameObject prefab, float intensity = 1.0f)
    {
        _heatmapCubes.Add(new HeatmapCube(data, gradient, prefab, intensity));
    }

    public void RemoveHeatmapCube(QueryDataStructure data)
    {
        _heatmapCubes.Remove(_heatmapCubes.Find(cube => cube.rawData == data));
    }
    
    public void EditorUpdate()
    {
        foreach (HeatmapCube heatmap in _heatmapCubes)
        {
            heatmap.EditorUpdate();
        }
    }
}
