using System;
using System.Collections.Generic;
using UnityEngine;
using MapHelper;
using static Define;
using System.IO;
using System.Linq;

#region Find Path
namespace MapHelper
{
    [Serializable]
    public class Cell
    {
        public HashSet<BaseController> _objects { get; private set; } = new HashSet<BaseController>();

        public BaseController bc;
        public Player pc;

        public int x;
        public int y;
        public ETileType TileType;
        public float requiredJumpPower;

        #region Helpers

        public List<BaseController> ReturnAllObject()
        {
            return _objects.ToList();
        }

        public BaseController ReturnObjectByType(EObjectType objecyType)
        {
            switch (objecyType)
            {
                case EObjectType.Player:
                    return pc;
                default:
                    return bc;
            }
        }

        public void SetObjectByType(BaseController obj)
        {
            switch (obj.ObjectType)
            {
                case EObjectType.Player:
                    pc = obj as Player;
                    _objects.Add(obj);
                    break;
                default:
                    bc = obj;
                    _objects.Add(obj);
                    break;
            }
        }

        public void RemoveObjectByType(EObjectType objecyType)
        {
            switch (objecyType)
            {
                case EObjectType.Player:
                    _objects.Remove(pc);
                    pc = null;
                    break;
                default:
                    _objects.Remove(bc);
                    bc = null;
                    break;
            }
        }

        #endregion

        public void Clear()
        {
            _objects.Clear();
            bc = null;
            pc = null;
        }
    }

    [Serializable]
    public class CellEdge
    {
        public Vector3Int from;
        public Vector3Int to;
        public EdgeType edgeType;
        public float cost;
    }

    [Serializable]
    public class TileNodeData // Parsing 틀
    {
        public List<Cell> TileNodes = new List<Cell>();
    }

    [Serializable]
    public class TileMapData
    {
        public List<Cell> Cells = new();
        public List<CellEdge> Edges = new();

        public int MinX = int.MaxValue;
        public int MaxX = int.MinValue;
        public int MinY = int.MaxValue;
        public int MaxY = int.MinValue;

        public Dictionary<Vector3Int, Cell> CellMap = new();
        public Dictionary<Vector3Int, List<CellEdge>> EdgeMap = new();

        public void BuildCache()
        {
            CellMap.Clear();
            EdgeMap.Clear();

            foreach (var node in Cells)
            {
                var pos = new Vector3Int(node.x, node.y);
                CellMap[pos] = node;
            }

            foreach (var edge in Edges)
            {
                if (!EdgeMap.ContainsKey(edge.from))
                    EdgeMap[edge.from] = new List<CellEdge>();

                EdgeMap[edge.from].Add(edge);
            }
        }
    }

    public static class TileMapBuilder
    {
        public static TileMapData Build(List<Cell> cells, float jumpRadius = 4f)
        {
            var graph = new TileMapData { Cells = cells };

            Dictionary<Vector3Int, Cell> cellMap = new();
            foreach (var cell in cells)
                cellMap[new Vector3Int(cell.x, cell.y)] = cell;

            // 2) Min/Max X,Y 계산
            foreach (var pos in cellMap.Keys)
            {
                if (pos.x < graph.MinX) graph.MinX = pos.x;
                if (pos.x > graph.MaxX) graph.MaxX = pos.x;
                if (pos.y < graph.MinY) graph.MinY = pos.y;
                if (pos.y > graph.MaxY) graph.MaxY = pos.y;
            }

            // 1) Cell 순회
            for (int i = 0; i < cells.Count; i++)
            {
                // Self Define
                Cell from = cells[i];
                Vector3Int fromPos = new Vector3Int(from.x, from.y);

                if (from.TileType == ETileType.DeadEnd)
                    continue;

                if (from.TileType == ETileType.Jumpable)
                {
                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (i == j) continue;

                        Cell to = cells[j];
                        if (to.TileType != ETileType.Jumpable) continue;

                        Vector3Int toPos = new Vector3Int(to.x, to.y);
                        float sqrDist = (toPos - fromPos).sqrMagnitude; // Tile to Tile 거리 계산

                        if (sqrDist <= jumpRadius * jumpRadius)
                        {
                            // 사이 타일 검사
                            if (!HasBlockedBetween(fromPos, toPos, cellMap))
                                AddBidirectionalEdge(fromPos, toPos, EdgeType.Jump, Mathf.Sqrt(sqrDist), graph.Edges);
                        }
                    }
                }

                // 수평 인접 연결
                Vector3Int[] directions = { Vector3Int.left, Vector3Int.right };
                foreach (var dir in directions)
                {
                    Vector3Int neighbor = fromPos + dir;

                    // Jumpable(from) <-> Jumpable(to) 사이에 HorizontalOnly가 끼어있으면 연결 금지
                    if (cellMap.TryGetValue(neighbor, out Cell to))
                    {
                        if (to.TileType == ETileType.DeadEnd)
                            continue;

                        bool isBetweenBlocked = false;
                        if (from.TileType == ETileType.Jumpable && to.TileType == ETileType.Jumpable)
                        {
                            if (HasBlockedBetween(fromPos, neighbor, cellMap))
                                isBetweenBlocked = true;
                        }

                        if (!isBetweenBlocked)
                        {
                            float dist = Vector3Int.Distance(fromPos, neighbor);
                            AddBidirectionalEdge(fromPos, neighbor, EdgeType.Horizontal, dist, graph.Edges);
                        }
                    }
                }
            }

            graph.BuildCache();
            return graph;
        }

        // 양방향 간선 추가
        private static void AddBidirectionalEdge(Vector3Int a, Vector3Int b, EdgeType type, float cost, List<CellEdge> edges)
        {
            edges.Add(new CellEdge { from = a, to = b, edgeType = type, cost = cost });
            edges.Add(new CellEdge { from = b, to = a, edgeType = type, cost = cost });
        }

        // fromPos → toPos 사이의 경로 중 HorizontalOnly 타일이 존재하는지 검사
        private static bool HasBlockedBetween(Vector3Int fromPos, Vector3Int toPos, Dictionary<Vector3Int, Cell> cellMap)
        {
            Vector3Int delta = toPos - fromPos;
            int dx = Math.Sign(delta.x);
            int dy = Math.Sign(delta.y);
            int steps = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));

            for (int i = 1; i < steps; i++) // fromPos, toPos 제외
            {
                Vector3Int checkPos = fromPos + new Vector3Int(dx * i, dy * i);
                if (cellMap.TryGetValue(checkPos, out Cell midCell))
                {
                    if (midCell.TileType == ETileType.HorizontalOnly)
                        return true; // 막힘
                }
            }

            return false; // 막힌 타일 없음
        }
    }
}
#endregion

public class MapManager
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }

    public TileMapData TileMapData { get; private set; }

    public Vector3Int World2Cell(Vector3 worldPos) { return CellGrid.WorldToCell(worldPos); }
    public Vector3 Cell2World(Vector3Int cellPos) { return CellGrid.CellToWorld(cellPos); }

    public TileMapData LoadMap(string mapName)
    {
        DestroyMap();

        GameObject map = Managers.Resource.Instantiate(mapName);
        map.transform.position = Vector3.zero;
        map.name = $"@Map_{mapName}";

        Map = map;
        MapName = mapName;
        CellGrid = map.GetComponent<Grid>();
        map.FindChild(name: "Ground", recursive: true).layer = (int)ELayer.Ground;
        
        return TileMapData;
    }

    public void DestroyMap()
    {
        // ClearObjects();

        if (Map != null)
            Managers.Resource.Destroy(Map);
    }

    public TileMapData ParseMapData()
    {
        /// tileNodeData를 가공해서 TileMapData로 변형

        // 1. JSON 파싱
        TextAsset TileHintMapData = Managers.Resource.Load<TextAsset>("TileHintMapData");
        TileNodeData tileNodeData = JsonUtility.FromJson<TileNodeData>(TileHintMapData.text);

        // 2. GraphBuilder를 이용해 연결된 그래프 구성
        TileMapData = TileMapBuilder.Build(tileNodeData.TileNodes);

        return TileMapData;
    }

    #region Cell Update Method
    public bool MoveTo(Creature obj, Vector3Int cellPos, bool forceMove = false)
    {
        if (CanGo(obj, cellPos) == false)
            return false;

        // 기존 좌표에 있던 오브젝트를 삭제한다
        // (단, 처음 신청했으면 해당 CellPos의 오브젝트가 본인이 아닐 수도 있음)
        RemoveObject(obj);

        // 새 좌표에 오브젝트를 등록한다.
        AddObject(obj, cellPos);

        // 셀 좌표 이동
        obj.SetCellPos(cellPos, forceMove);

        //Debug.Log($"Move To {cellPos}");

        return true;
    }

    public bool CanGo(BaseController self, Vector3 worldPos, bool ignoreObjects = false, bool ignoreSemiWall = false)
    {
        return CanGo(self, World2Cell(worldPos), ignoreObjects, ignoreSemiWall);
    }

    public bool CanGo(BaseController self, Vector3Int cellPos)
    {
        if (cellPos.x < TileMapData.MinX || cellPos.x > TileMapData.MaxX)
            return false;
        if (cellPos.y < TileMapData.MinY || cellPos.y > TileMapData.MaxY)
            return false;

        if (self.IsValid() == false)
        {
            List<BaseController> objs = GetObjects(cellPos);
            if (objs.Count == 0)
                return true;
        }
        else
        {
            Cell cell = GetCell(cellPos);
            BaseController bc = cell.ReturnObjectByType(self.ObjectType);

            if (bc.IsValid() == false)
                return true;
        }

        return false;
    }

    public List<BaseController> GetObjects(Vector3Int cellPos)
    {
        Cell cell = GetCell(cellPos);
        List<BaseController> objects = cell.ReturnAllObject();

        return objects;
    }

    public Cell GetCell(Vector3Int cellPos)
    {
        Cell cell = null;

        // 없으면 만들기 null check하기 귀찮
        if (TileMapData.CellMap.TryGetValue(cellPos, out cell) == false)
        {
            cell = new Cell();
            TileMapData.CellMap.Add(cellPos, cell);
        }

        return cell;
    }

    public bool RemoveObject(BaseController obj)
    {
        Cell cell = GetCell(obj.CellPos);
        BaseController prev = cell.ReturnObjectByType(obj.ObjectType);

        // 처음 신청했으면 해당 CellPos의 오브젝트가 본인이 아닐 수도 있음
        if (prev != obj)
            return false;

        cell.RemoveObjectByType(obj.ObjectType);
        return true;
    }

    private bool AddObject(BaseController obj, Vector3Int cellPos)
    {
        if (CanGo(obj, cellPos) == false)
        {
            Debug.LogWarning($"AddObject Failed_1");
            return false;
        }

        Cell cell = GetCell(cellPos);
        BaseController prev = cell.ReturnObjectByType(obj.ObjectType);
        if (prev != null)
        {
            Debug.LogWarning($"{obj.name}, AddObject Failed_2");
            return false;
        }

        cell.SetObjectByType(obj);
        return true;
    }
    #endregion

    #region A* PathFinding
    public struct PQNode : IComparable<PQNode>
    {
        public int H; // Heuristic
        public Vector3Int CellPos;
        public int Depth;

        public int CompareTo(PQNode other)
        {
            if (H == other.H)
                return 0;
            return H < other.H ? 1 : -1;
        }
    }

    public List<Vector3Int> FindPathSideView(BaseController self, Vector3Int start, Vector3Int dest, int maxDepth = 10)
    {
        Dictionary<Vector3Int, Vector3Int> parent = new();
        Dictionary<Vector3Int, int> best = new();
        PriorityQueue<PQNode> pq = new();

        Vector3Int pos = start;
        Vector3Int closest = start;
        int closestH = (dest - pos).sqrMagnitude;

        pq.Push(new PQNode { H = closestH, CellPos = pos, Depth = 0 });
        parent[pos] = pos;
        best[pos] = closestH;

        while (pq.Count > 0)
        {
            PQNode node = pq.Pop();
            pos = node.CellPos;

            if (pos == dest)
                break;

            if (node.Depth >= maxDepth)
                continue;

            foreach (int dx in new[] { -1, 1 }) // Only left/right
            {
                Vector3Int next = new Vector3Int(pos.x + dx, pos.y, 0);

                if (CanGo(self, next) == false)
                    continue;

                int h = (dest - next).sqrMagnitude;

                if (best.TryGetValue(next, out int existing) && existing <= h)
                    continue;

                best[next] = h;
                parent[next] = pos;
                pq.Push(new PQNode { H = h, CellPos = next, Depth = node.Depth + 1 });

                if (h < closestH)
                {
                    closestH = h;
                    closest = next;
                }
            }
        }

        return ReconstructPath(parent, parent.ContainsKey(dest) ? dest : closest);
    }

    private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> parent, Vector3Int dest)
    {
        List<Vector3Int> path = new();
        if (!parent.ContainsKey(dest)) return path;

        Vector3Int cur = dest;
        while (parent[cur] != cur)
        {
            path.Add(cur);
            cur = parent[cur];
        }

        path.Reverse();
        return path;
    }
    #endregion
}
