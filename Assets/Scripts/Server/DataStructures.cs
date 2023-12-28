using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server
{
    [System.Serializable]
    public class DataTable
    {
        public string Table;

        public string ToJson()
        {
            Table = this.ToString().Replace("Server.", "");
            return JsonUtility.ToJson(this);
        }
    }

    [System.Serializable]
    public class PositionTable : DataTable // Inheritance to later work better when displaying visually in the scene.
    {
        public UInt64 SessionId;
        public int PositionX;
        public int PositionY;
        public int PositionZ;
        public string TimeStamp;
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
    public class Hit : PositionTable
    {
        [NonSerialized] public UInt64 HitId;
        public string AttackType;
        public int Damage;
        public string Hitter;
        public string Hitted;
        public int SourcePositionX;
        public int SourcePositionY;
        public int SourcePositionZ;
    }

    [System.Serializable]
    public class Death : PositionTable
    {
        [NonSerialized] public UInt64 DeathId;
        public string DeathType;
    }

    [System.Serializable]
    public class Interaction : PositionTable
    {
        [NonSerialized] public UInt64 InteractionId;
        public string interactionType;
    }

    [System.Serializable]
    public class Kill : PositionTable
    {
        [NonSerialized] public UInt64 KillId;
        public string enemyType;
    }

    [System.Serializable]
    public class Movement : PositionTable
    {
        [NonSerialized] public UInt64 MovementId;
    }
}