using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

public class HeatmapCube : Object
{
    public HeatmapCube(QueryDataStructure data, Gradient gradient, GameObject prefab, float intensity = 1.0f)
    {
        rawData = data;
        this.gradient = gradient;
        this._intensity = intensity;
        
        Init(data, prefab);
    }
    
    private List<GameObject> Cube = new();
    public QueryDataStructure rawData;
    private float _intensity = 1.0f;
    private float _yOffSet = 0.0f;
    public float Intensity
    {
        get => _intensity;
        set
        {
            _intensity = value;
            SetColor();
        }
    }
    public float YOffSet
    {
        get => _yOffSet;
        set
        {
            _yOffSet = value;
            SetOffset();
        }
    }
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
            Cube.Add(cube);
        }
    }
    public void DestroyCubes()
    {
        GameObject container = GameObject.Find($"Container_{rawData.name}_{rawData.id}");
        DestroyImmediate(container.gameObject);
    }
    void SetColor()
     {
         for (int i = 0; i < Cube.Count; i++)
         {
             MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
             Renderer cubeRenderer = Cube[i].GetComponent<Renderer>();
             cubeRenderer.GetPropertyBlock(propertyBlock);
             float calculateValue = Mathf.Clamp01(rawData.NormalizedValue[i] * Intensity);
             Color color = gradient.Evaluate(calculateValue);
             propertyBlock.SetColor("_Color", color);
             cubeRenderer.SetPropertyBlock(propertyBlock);
         }
     }

    public void SetOffset()
    {
        for (int i = 0; i < Cube.Count; i++)
        {
            var newPos =  Cube[i].transform.position;
            newPos.y = _yOffSet;
            Cube[i].transform.position = newPos;
        }
    }
}

public class HeatmapShader
{
    private Material _mMaterial;
    private float[] _points;
    private int _pointCount;
    private float _intensity;
    private List<Vector3> _positions;
    public HeatmapShader(QueryDataStructure structure, Material mat, Gradient gradient,float intensity)
    {
        _mMaterial = mat;
        //_mMaterial.SetColor();
        _points = new float[structure.Position.Count * 3];
        _intensity = intensity;
        _positions = structure.Position;
    }

    public void Generate()
    {
        Debug.Log("Generating heatmap shader");
        foreach (Vector3 p in _positions)
        {
            Vector3 rayOrigin = p;
            rayOrigin.y += 1;

            Ray ray = new Ray(rayOrigin, Vector3.down);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 10.0f,LayerMask.GetMask("Heatmap"))) {
                AddHitPoint(hit.textureCoord.x*4-2,hit.textureCoord.y*4-2);
            }
        }
    }
    
    void AddHitPoint(float px, float py)
    {
        _points[_pointCount * 3] = px;
        _points[_pointCount * 3 + 1] = py;
        _points[_pointCount * 3 + 2] = _intensity;
        _pointCount++;

        _pointCount %= _positions.Count;
        _mMaterial.SetFloatArray("_Hits",_points);
        _mMaterial.SetInt("_HitCount",_pointCount);
        
        Debug.Log($"hit point added {px} {py}");
    }
}

public class HeatmapDrawer : Object
{
    private List<HeatmapCube> _heatmapCubes = new List<HeatmapCube>();
    private List<HeatmapShader> _heatmapShaders = new List<HeatmapShader>();

    public void CreateHeatmapCube(QueryDataStructure data, Gradient gradient, GameObject prefab, float intensity = 1.0f)
    {
        Debug.Log($"HeatmapDrawer: Creating heatmap cube {data.name}_{data.id}");
        HeatmapCube hc = new HeatmapCube(data, gradient, prefab, intensity);
        _heatmapCubes.Add(hc);
    }

    public HeatmapCube GetHeatMapCube(QueryDataStructure data)
    {
        return _heatmapCubes.Find(cube => cube.rawData.id == data.id);
    }

    public void RemoveHeatmapCube(QueryDataStructure data)
    {
        Debug.Log($"HeatmapDrawer: Removing heatmap cube {data.name}_{data.id}");
        HeatmapCube hc = _heatmapCubes.Find(cube => cube.rawData.id == data.id);
        if (hc != null)
        {
            hc.DestroyCubes();
            _heatmapCubes.RemoveAll(element => element.rawData.id == data.id);
        }
    }

    public void RemoveAllHeatMapCubes()
    {
        if (_heatmapCubes.Count == 0) return;
        
        foreach (HeatmapCube heatmap in _heatmapCubes)
        {
            heatmap.DestroyCubes();
            _heatmapCubes.Remove(heatmap);
        }
        
        _heatmapCubes.Clear();
    }

    public void CreateHeatmapShader(QueryDataStructure data,Material mat, Gradient gradient, float intensity)
    {
        HeatmapShader heatmapShader = new HeatmapShader(data,mat,gradient,intensity);
        
        heatmapShader.Generate();
    }
    // public void EditorUpdate()
    // {
    //     if (_heatmapCubes.Count == 0) return;
    //     
    //     foreach (HeatmapCube heatmap in _heatmapCubes)
    //     {
    //         heatmap.EditorUpdate();
    //     }
    // }
}
