using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEngine;
using static Define;

namespace Data
{
    #region Example
    public class ParentData
    {
        public int TemplateId;
    }

    [Serializable]
    public class ChildData : ParentData
    {
        public int TempIntData;
        public string TempStringData;
        public List<ListData> TempListData = new List<ListData>();
    }

    [Serializable]
    public class ListData
    {
        public int Value_1;
        public int ObjectTemplateId;
    }

    [Serializable]
    public class ChildDataLoader : ILoader<int, ChildData>
    {
        public List<ChildData> Childs = new List<ChildData>();
        public Dictionary<int, ChildData> MakeDict()
        {
            Dictionary<int, ChildData> dict = new Dictionary<int, ChildData>();
            foreach (ChildData child in Childs)
                dict.Add(child.TemplateId, child);
            return dict;
        }
    }
    #endregion

    [Serializable]
    public class CreatureData
    {
        public int TemplateId;
        public string NameDataId;

        public string AnimDataId;
        public string SortingLayerName;

        public float ColliderOffsetX;
        public float ColliderOffsetY;
        public float ColliderSizeX;
        public float ColliderSizeY;

        public float MaxHp;
        public float MaxSpeed;
        public float JumpForce;
    }

    [Serializable]
    public class PlayerData : CreatureData
    {
        public float JumpToMidSpeedThreshold; // Threshold : 경계값
        public float MidToFallSpeedThreshold; // Threshold : 경계값

        public float CoyoteTimeDuration;  // 지면 Check 유예 시간 (초)
    }

    [Serializable]
    public class PlayerDataLoader : ILoader<int, PlayerData>
    {
        public List<PlayerData> PlayersDatas = new List<PlayerData>();
        public Dictionary<int, PlayerData> MakeDict()
        {
            Dictionary<int, PlayerData> dict = new Dictionary<int, PlayerData>();
            foreach (PlayerData data in PlayersDatas)
                dict.Add(data.TemplateId, data);
            return dict;
        }
    }
}
