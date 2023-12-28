using System;
using UnityEngine;

namespace Server
{
    [System.Serializable]
    public class DataTable
    {
        public string Table;
        public string ToJson()
        {
            Table = this.ToString().Replace("Server.", "");
            return JsonUtility.ToJson(this);;
        }
    }
    
    [System.Serializable]
    public class User : DataTable
    {
        public UInt64 Id;
        public string Name;
        public string Sex;
        public int Age;
        public string Country;
        public string DateCreated;
    }
    [System.Serializable]
    public class Session : DataTable
    {
        public UInt64 SessionId;
        public UInt64 PlayerId;
        public string Start;
        public string End;
    }
    [System.Serializable]
    public class Hit : DataTable
    {
       public UInt64 HitId;
       public UInt64 SessionId;
       public int PostionX;
       public int PostionY;
       public int PostionZ;
       public string TimeStamp;
       public string AttackType;
       public int Damage;
       public string Hitter;
       public string Hitted;
       public int SourcePositionX;
       public int SourcePositionY;
       public int SourcePositionZ;
    }
    
    [System.Serializable]
    public class Death : DataTable
    {
        public UInt64 DeathId;
        public UInt64 SessionId;
        public int PostionX;
        public int PostionY;
        public int PostionZ;
        public string TimeStamp;
        public string DeathType;
    }
    
    [System.Serializable]
    public class Interaction : DataTable
    {
        public UInt64 InteractionId;
        public UInt64 SessionId;
        public int PostionX;
        public int PostionY;
        public int PostionZ;
        public string TimeStamp;
        public string interactionType;
    }
    
    [System.Serializable]
    public class Kill : DataTable
    {
       public UInt64 KillId;
       public UInt64 SessionId;
       public int PostionX;
       public int PostionY;
       public int PostionZ;
       public string TimeStamp;
       public string enemyType;
    }
    
    [System.Serializable]
    public class Movement : DataTable
    { 
       public UInt64 MovementId;
       public UInt64 SessionId;
       public int PostionX;
       public int PostionY;
       public int PostionZ;
       public string TimeStamp;
    }
    
}