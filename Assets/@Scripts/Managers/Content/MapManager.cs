using System;
using System.Collections.Generic;
using UnityEngine;
using MapHelper;
using static Define;

namespace MapHelper
{
    [Serializable]
    public class TileNode
    {
        public int x;
        public int y;
        public ETileType TileType;
        public float requiredJumpPower;
    }

    [Serializable]
    public class TileEdge
    {
        public Vector2Int from;
        public Vector2Int to;
        public EdgeType edgeType;
        public float cost;
    }

    [Serializable]
    public class TileGraph
    {
        public List<TileNode> nodes = new();
        public List<TileEdge> edges = new();

        public Dictionary<Vector2Int, TileNode> nodeMap = new();
        public Dictionary<Vector2Int, List<TileEdge>> edgeMap = new();

        public void BuildCache()
        {
            nodeMap.Clear();
            edgeMap.Clear();

            foreach (var node in nodes)
            {
                var pos = new Vector2Int(node.x, node.y);
                nodeMap[pos] = node;
            }

            foreach (var edge in edges)
            {
                if (!edgeMap.ContainsKey(edge.from))
                    edgeMap[edge.from] = new List<TileEdge>();

                edgeMap[edge.from].Add(edge);
            }
        }
    }

    public static class TileGraphBuilder
    {
        public static TileGraph Build(List<TileNode> nodes, float jumpRadius = 4f)
        {
            var graph = new TileGraph { nodes = nodes };

            Dictionary<Vector2Int, TileNode> nodeMap = new();
            foreach (var node in nodes)
                nodeMap[new Vector2Int(node.x, node.y)] = node;

            // 1) Node 순회
            for (int i = 0; i < nodes.Count; i++)
            {
                // Self Define
                TileNode from = nodes[i];
                Vector2Int fromPos = new Vector2Int(from.x, from.y);

                if (from.TileType == ETileType.DeadEnd)
                    continue;

                if (from.TileType == ETileType.Jumpable)
                {
                    for (int j = 0; j < nodes.Count; j++)
                    {
                        if (i == j) continue;

                        TileNode to = nodes[j];
                        if (to.TileType != ETileType.Jumpable) continue;

                        Vector2Int toPos = new Vector2Int(to.x, to.y);
                        float sqrDist = (toPos - fromPos).sqrMagnitude; // Tile to Tile 거리 계산

                        if (sqrDist <= jumpRadius * jumpRadius)
                        {
                            // 사이 타일 검사
                            if (!HasBlockedBetween(fromPos, toPos, nodeMap))
                                AddBidirectionalEdge(fromPos, toPos, EdgeType.Jump, Mathf.Sqrt(sqrDist), graph.edges);
                        }
                    }
                }

                // 수평 인접 연결
                Vector2Int[] directions = { Vector2Int.left, Vector2Int.right };
                foreach (var dir in directions)
                {
                    Vector2Int neighbor = fromPos + dir;

                    // Jumpable(from) <-> Jumpable(to) 사이에 HorizontalOnly가 끼어있으면 연결 금지
                    if (nodeMap.TryGetValue(neighbor, out TileNode to))
                    {
                        if (to.TileType == ETileType.DeadEnd)
                            continue;

                        bool isBetweenBlocked = false;
                        if (from.TileType == ETileType.Jumpable && to.TileType == ETileType.Jumpable)
                        {
                            if (HasBlockedBetween(fromPos, neighbor, nodeMap))
                                isBetweenBlocked = true;
                        }

                        if (!isBetweenBlocked)
                        {
                            float dist = Vector2.Distance((Vector2)fromPos, (Vector2)neighbor);
                            AddBidirectionalEdge(fromPos, neighbor, EdgeType.Horizontal, dist, graph.edges);
                        }
                    }
                }
            }

            graph.BuildCache();
            return graph;
        }

        // 양방향 간선 추가
        private static void AddBidirectionalEdge(Vector2Int a, Vector2Int b, EdgeType type, float cost, List<TileEdge> edges)
        {
            edges.Add(new TileEdge { from = a, to = b, edgeType = type, cost = cost });
            edges.Add(new TileEdge { from = b, to = a, edgeType = type, cost = cost });
        }

        // fromPos → toPos 사이의 경로 중 HorizontalOnly 타일이 존재하는지 검사
        private static bool HasBlockedBetween(Vector2Int fromPos, Vector2Int toPos, Dictionary<Vector2Int, TileNode> nodeMap)
        {
            Vector2Int delta = toPos - fromPos;
            int dx = Math.Sign(delta.x);
            int dy = Math.Sign(delta.y);
            int steps = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));

            for (int i = 1; i < steps; i++) // fromPos, toPos 제외
            {
                Vector2Int checkPos = fromPos + new Vector2Int(dx * i, dy * i);
                if (nodeMap.TryGetValue(checkPos, out TileNode midNode))
                {
                    if (midNode.TileType == ETileType.HorizontalOnly)
                        return true; // 막힘
                }
            }

            return false; // 막힌 타일 없음
        }
    }
}

[Serializable]
public class TileNodeList
{
    public List<TileNode> tiles = new List<TileNode>();
}

public class MapManager
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }

    public TileGraph TileMapGraph { get; private set; }

    public TileGraph LoadMap(string mapName)
    {
        DestroyMap();

        GameObject map = Managers.Resource.Instantiate(mapName);
        map.transform.position = Vector3.zero;
        map.name = $"@Map_{mapName}";

        Map = map;
        MapName = mapName;
        CellGrid = map.GetComponent<Grid>();
        map.FindChild(name: "Ground", recursive: true).layer = (int)ELayer.Ground;
        
        return TileMapGraph;
    }

    public void DestroyMap()
    {
        // ClearObjects();

        if (Map != null)
            Managers.Resource.Destroy(Map);
    }

    public TileGraph ParseMapGraphData()
    {
        // 1. JSON 파싱
        TextAsset TileHintMapData = Managers.Resource.Load<TextAsset>("TileHintMapData");
        TileNodeList tileNodeList = JsonUtility.FromJson<TileNodeList>(TileHintMapData.text);

        // 2. GraphBuilder를 이용해 연결된 그래프 구성
        TileMapGraph = TileGraphBuilder.Build(tileNodeList.tiles);

        return TileMapGraph;
    }
}
