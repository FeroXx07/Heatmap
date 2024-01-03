// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class Queries : MonoBehaviour
// {
//     public class FunctionCaller
//     {
//         private Action lastCalledAction;
//
//         public void SaveAction(Action action)
//         {
//             lastCalledAction = action;
//         }
//
//         public void InvokeSavedAction()
//         {
//             lastCalledAction?.Invoke();
//         }
//     }
//     public enum GranularityType
//     {
//         /// <summary>
//         /// ROUND(Position * Granularity) / Granularity: This expression multiplies the Position by the Granularity value, rounds the result to the nearest integer,
//         /// and then divides by the Granularity. It rounds the value to the nearest multiple of the inverse of the granularity.
//         /// For example, if you have Position = 7 and Granularity = 2, the result would be ROUND(7 * 2) / 2 = ROUND(14) / 2 = 14 / 2 = 7.
//         /// It provides the closest multiple of the granularity value to the original value.iginal value.
//         /// </summary>
//         ROUND = 0, //
//         
//         /// <summary>
//         /// FLOOR(Position / Granularity): This expression divides the Position by the Granularity value and then takes the FLOOR of the result.
//         /// It rounds down the division result to the nearest integer towards negative infinity.
//         /// For instance, if you have Position = 7 and Granularity = 2, the result would be FLOOR(7 / 2) = FLOOR(3.5) = 3.
//         /// It always provides a result that is less than or equal to the original value.
//         /// </summary>
//         FLOOR = 1, //
//     }
//     public float granularity = 1.0f;
//
//     private string Granularity => granularity.ToString().Replace(',', '.');
//
//     public GranularityType granularityType = GranularityType.ROUND;
//     private QueryManager queryManager;
//
//     private FunctionCaller lastCalledQuery;
//     private void Awake()
//     {
//         queryManager = GetComponent<QueryManager>();
//         lastCalledQuery = new FunctionCaller();
//     }
//
//     public void SetGranularityValue(float v)
//     {   
//         GetComponent<QueryDrawer>().Scale = v;
//         granularity = v;
//         lastCalledQuery?.InvokeSavedAction();
//     }
//     
//     public void SetGranularityType(int v)
//     {
//         granularityType = (GranularityType)v;
//         lastCalledQuery?.InvokeSavedAction();
//     }
//     
//     private string GetGranularityQueryString(GranularityType type)
//     {
//         switch (type)
//         {
//             case GranularityType.ROUND:
//             {
//                 string h1 =
//                     $"SELECT ROUND(PositionX * {Granularity})/{Granularity} AS GridPositionX, ROUND(PositionY * {Granularity})/{Granularity} AS GridPositionY, ROUND(PositionZ * {Granularity})/{Granularity} AS GridPositionZ,";
//                 return h1;
//             }
//             case GranularityType.FLOOR:
//             {
//                 string h1 =
//                     $"SELECT FLOOR(PositionX / {Granularity}) AS GridPositionX, FLOOR(PositionY / {Granularity}) AS GridPositionY, FLOOR(PositionZ / {Granularity}) AS GridPositionZ,";
//                 return h1;
//             }
//             default:
//                 return string.Empty;
//         }
//     }
//     
//     public void DoQuery_DamagePositionNormalized()
//     {
//         string query = $"{GetGranularityQueryString(granularityType)} SUM(Damage) AS TotalDamageInPosition, SUM(Damage) / MAX(SUM(Damage)) OVER() AS NormalizedDamage FROM Hit GROUP BY GridPositionX, GridPositionY, GridPositionZ;";
//         queryManager.RequestQuery(query);
//         lastCalledQuery.SaveAction(DoQuery_DamagePositionNormalized);
//     }
//
//     public void DoQuery_PlayerDamagePositionNormalized()
//     {
//         string query =
//             $"{GetGranularityQueryString(granularityType)} SUM(Damage) AS TotalDamageInPosition, SUM(Damage) / MAX(SUM(Damage)) OVER() AS NormalizedDamage FROM Hit WHERE Hitter = \"Staff\" GROUP BY GridPositionX, GridPositionY, GridPositionZ;";
//         queryManager.RequestQuery(query);
//         lastCalledQuery.SaveAction(DoQuery_PlayerDamagePositionNormalized);
//     }
//     
//     public void DoQuery_EnemyDamagePositionNormalized()
//     {
//         string query =
//             $"{GetGranularityQueryString(granularityType)} SUM(Damage) AS TotalDamageInPosition, SUM(Damage) / MAX(SUM(Damage)) OVER() AS NormalizedDamage FROM Hit WHERE Hitter != \"Staff\" GROUP BY GridPositionX, GridPositionY, GridPositionZ;";
//         queryManager.RequestQuery(query);
//         lastCalledQuery.SaveAction(DoQuery_EnemyDamagePositionNormalized);
//     }
// }
