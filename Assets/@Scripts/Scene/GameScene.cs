using System;
using System.Collections.Generic;
using UnityEngine;
using MapHelper;
using static Define;

public class GameScene : BaseScene
{
    TileGraph _currnetTileGraph;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = EScene.GameScene;

        _currnetTileGraph = Managers.Map.LoadMap("BaseMap");

        Managers.Object.Spawn<Player>(new Vector3(-2.5f, -4), 10); // 10 -> Player Id
        Managers.Object.Spawn<Enemy>(new Vector3(-2.5f, -4), 10); // 10 -> To Do

        return true;
    }

    public override void Clear()
    {
        
    }

    #region Gizmo - Tile Map Node
    public bool ON_GIZMO = true;
    public float nodeSize = 0.2f;
    public bool drawJumpRadius = false;
    public float jumpRadius = 5f;
    private void OnDrawGizmos()
    {
        if (ON_GIZMO == false)
            return;

        // 1. 노드 표시
        Gizmos.color = Color.yellow;
        foreach (var node in _currnetTileGraph.nodes)
        {
            Vector3 pos = new Vector3(node.x + 0.5f, node.y + 0.5f, 0);
            Gizmos.DrawCube(pos, Vector3.one * nodeSize);

            if (drawJumpRadius && node.TileType == ETileType.Jumpable)
            {
                Gizmos.color = new Color(0, 1f, 1f, 0.2f);
                Gizmos.DrawWireSphere(pos, jumpRadius);
                Gizmos.color = Color.yellow;
            }
        }

        // 2. 간선 표시
        foreach (var edge in _currnetTileGraph.edges)
        {
            Vector3 from = new Vector3(edge.from.x + 0.5f, edge.from.y + 0.5f, 0);
            Vector3 to = new Vector3(edge.to.x + 0.5f, edge.to.y + 0.5f, 0);

            Gizmos.color = edge.edgeType == EdgeType.Jump ? Color.cyan : Color.white;
            Gizmos.DrawLine(from, to);
        }
    }
    #endregion
}
