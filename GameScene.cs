using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using static Define;

[Serializable]
public class TileNode
{
    public int x;
    public int y;
    public int TileType;             // 0 = 수평만, 1 = 점프 가능, 2 = 막다른 길
    public float requiredJumpPower; // 확장용
}

public enum EdgeType { Horizontal, Jump }

[Serializable]
public class TileEdge
{
    public Vector2Int from;
    public Vector2Int to;
    public EdgeType edgeType;
    public float cost;
}

[Serializable]
public class TileNodeList
{
    public List<TileNode> tiles = new();
}

public class GameScene : BaseScene
{
    public TextAsset JsonPlatformGraph;
    // private PlatformGraphDataWithLinks graph;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = EScene.GameScene;

        // 1. JSON 파싱
        TextAsset TileHintMapData = Managers.Resource.Load<TextAsset>("TileHintMapData");
        TileNodeList tileNodeList = JsonUtility.FromJson<TileNodeList>(TileHintMapData.text);

        // 2. GraphBuilder를 이용해 연결된 그래프 구성
        TileGraph graph = TileGraphBuilder.Build(tileNodeList.tiles);

        return true;
    }

    public override void Clear()
    {
        
    }
}
