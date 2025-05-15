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

    public Dictionary<int, Data.PlayerData> PlayerDataDic { get; private set; } = new Dictionary<int, Data.PlayerData>();
    public Dictionary<int, Data.PlayerMovementData> PlayerMovementDataDic { get; private set; } = new Dictionary<int, Data.PlayerMovementData>();
    public Dictionary<int, Data.EnemyMovementData> EnemyMovementDataDic { get; private set; } = new Dictionary<int, Data.EnemyMovementData>();

    public void Init()
    {
        // Example
        // ChildDataDic = LoadJson<Data.ChildDataLoader, int, Data.ChildData>("ChildData").MakeDict();

        PlayerDataDic = LoadJson<Data.PlayerDataLoader, int, Data.PlayerData>("PlayerData").MakeDict();
        PlayerMovementDataDic = LoadJson<Data.PlayerMovementDataLoader, int, Data.PlayerMovementData>("PlayerMovementData").MakeDict();
        EnemyMovementDataDic = LoadJson<Data.EnemyMovementDataLoader, int, Data.EnemyMovementData>("EnemyMovementData").MakeDict();
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
