using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class QueryDataStructure
{
    public QueryDataStructure(string name, uint id, HeatmapEditorWindow.QueryType type)
    {
        this.name = name;
        this.id = id;
        this.type = type;
    }
    public virtual void InsertData(float x, float y, float z)
    {
        Position.Add(new Vector3(x, y, z));
    }
    
    public uint id = UInt32.MaxValue;
    public string name;
    public HeatmapEditorWindow.QueryType type;
    public List<Vector3> Position = new List<Vector3>();
}

public class PathDataStructure : QueryDataStructure
{
    public UInt64 playerId;
    public UInt64 sessionId;
    public PathDataStructure(string name, uint id, HeatmapEditorWindow.QueryType type) : base(name, id, type)
    {
        this.playerId = playerId;
        this.sessionId = sessionId;
    }
}
public class NormalizedDataStructure : QueryDataStructure
{
    public List<float> NormalizedValue = new List<float>();
    public NormalizedDataStructure(string name, uint id, HeatmapEditorWindow.QueryType type) : base(name, id, type)
    {
    }
    public void InsertData(float x, float y, float z, float v)
    {
        base.InsertData(x, y, z);
        NormalizedValue.Add(v);
    }
}

public class QueryHandeler
{
  private List<QueryDataStructure> _queryList = new List<QueryDataStructure>();
  
  private uint id = 0;

  private uint GetNewId()
  {
      id++;
      return id;
  }
  
  private float granularity = 1.0f;
  private string Granularity => granularity.ToString().Replace(',', '.');
  public GranularityType granularityType = GranularityType.ROUND;
  public List<QueryDataStructure> GetQueryList()
  {
      return _queryList;
  }

  public uint SaveNewQuery(string queryName, HeatmapEditorWindow.QueryType type)
  {
      uint id = GetNewId();
      QueryDataStructure q = null;
      if (type == HeatmapEditorWindow.QueryType.CUBES || type == HeatmapEditorWindow.QueryType.SHADER)
      {
           q = new NormalizedDataStructure(queryName, id, type);
      }
      else if (type == HeatmapEditorWindow.QueryType.PATH)
      {
          q = new PathDataStructure(queryName, id, type);
      }
      _queryList.Add(q);
      return id;
  }
  
  public void ClearQuery(QueryDataStructure q)
  {
        if (_queryList.Contains(q))
            _queryList.Remove(q);
  }
  
  public void ClearQueryList()
  {
      _queryList.Clear();
  }
  
  public QueryDataStructure ProcessQueryReceived(string result, uint id)
  {
       QueryDataStructure q = _queryList.Find(s => s.id == id);
       if (q == null)
         Debug.LogError("Null query");
           
       // Split the received string into lines
       string[] rows = result.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
       // Parse the received lines into arrays
       foreach (string row in rows)
       {
           if (q.type == HeatmapEditorWindow.QueryType.CUBES || q.type == HeatmapEditorWindow.QueryType.SHADER)
           {
               var nData = q as NormalizedDataStructure;
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
               nData.InsertData(posX, posY, posZ, normalizedDamage);
           }
           else if (q.type == HeatmapEditorWindow.QueryType.PATH)
           {
               var pathData = q as PathDataStructure;
               string[] rowData = row.Split('\n');
                
               float.TryParse(rowData[0].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                   out float posX);
               float.TryParse(rowData[1].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                   out float posY);
               float.TryParse(rowData[2].Split(':')[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                   out float posZ);
               
               pathData.InsertData(posX, posY, posZ);
           }
       }
      
       // Display retrieved data (for demonstration)
       // Debug.Log("Positions:");
       // for (int i = 0; i < q.Position.Count; i++)
       // {
       //    Debug.Log($"Position X: {q.Position[i].x}, Position Y: {q.Position[i].y}, Position Z: {q.Position[i].z}, Value: {q.NormalizedValue[i]}");
       // }

       return q;
  }
  public string GetQueryType(string queryType)
  {
      // Adjust the query based on the selected type
      string query = "";
      switch (queryType)
      {
        case "DamagePositionNormalized":
          query = $"{GetGranularityQueryString(granularityType)} SUM(Damage) AS TotalDamageInPosition, SUM(Damage) / MAX(SUM(Damage)) OVER() AS NormalizedDamage FROM Hit GROUP BY GridPositionX, GridPositionY, GridPositionZ;";
          break;

        case "PlayerDamagePositionNormalized":
          query = $"{GetGranularityQueryString(granularityType)} SUM(Damage) AS TotalDamageInPosition, SUM(Damage) / MAX(SUM(Damage)) OVER() AS NormalizedDamage FROM Hit WHERE Hitter = \"Staff\" GROUP BY GridPositionX, GridPositionY, GridPositionZ;";
          break;

        case "EnemyDamagePositionNormalized":
          query = $"{GetGranularityQueryString(granularityType)} SUM(Damage) AS TotalDamageInPosition, SUM(Damage) / MAX(SUM(Damage)) OVER() AS NormalizedDamage FROM Hit WHERE Hitter != \"Staff\" GROUP BY GridPositionX, GridPositionY, GridPositionZ;";
          break;
            case "Interaction":
            
          break;
        default:
          Debug.LogError($"Unsupported query type: {queryType}");
          return null;
      }

      return query;
  }

  
  private string GetGranularityQueryString(GranularityType type)
  {
    switch (type)
    {
      case GranularityType.ROUND:
      {
        string h1 =
          $"SELECT ROUND(PositionX * {Granularity})/{Granularity} AS GridPositionX, ROUND(PositionY * {Granularity})/{Granularity} AS GridPositionY, ROUND(PositionZ * {Granularity})/{Granularity} AS GridPositionZ,";
        return h1;
      }
      case GranularityType.FLOOR:
      {
        string h1 =
          $"SELECT FLOOR(PositionX / {Granularity}) AS GridPositionX, FLOOR(PositionY / {Granularity}) AS GridPositionY, FLOOR(PositionZ / {Granularity}) AS GridPositionZ,";
        return h1;
      }
      default:
        return string.Empty;
    }
  }
  
  public enum GranularityType
  {
    /// <summary>
    /// ROUND(Position * Granularity) / Granularity: This expression multiplies the Position by the Granularity value, rounds the result to the nearest integer,
    /// and then divides by the Granularity. It rounds the value to the nearest multiple of the inverse of the granularity.
    /// For example, if you have Position = 7 and Granularity = 2, the result would be ROUND(7 * 2) / 2 = ROUND(14) / 2 = 14 / 2 = 7.
    /// It provides the closest multiple of the granularity value to the original value.iginal value.
    /// </summary>
    ROUND = 0, //
        
    /// <summary>
    /// FLOOR(Position / Granularity): This expression divides the Position by the Granularity value and then takes the FLOOR of the result.
    /// It rounds down the division result to the nearest integer towards negative infinity.
    /// For instance, if you have Position = 7 and Granularity = 2, the result would be FLOOR(7 / 2) = FLOOR(3.5) = 3.
    /// It always provides a result that is less than or equal to the original value.
    /// </summary>
    FLOOR = 1, //
  }
}
