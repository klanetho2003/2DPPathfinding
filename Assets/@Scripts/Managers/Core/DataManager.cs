using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    // Example
    public Dictionary<int, Data.ChildData> ChildDataDic { get; private set; } = new Dictionary<int, Data.ChildData>();

    public void Init()
    {
        // Example
        ChildDataDic = LoadJson<Data.ChildDataLoader, int, Data.ChildData>("ChildData").MakeDict();

        
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
