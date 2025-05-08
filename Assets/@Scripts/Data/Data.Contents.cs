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
}
