using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;
using System;
using MapHelper;

public class TilemapExporter : EditorWindow
{
    private Tilemap tilemap;

    [MenuItem("Tools/Export Tilemap Hint Graph")]
    public static void ShowWindow()
    {
        GetWindow<TilemapExporter>("Tilemap Exporter");
    }

    private void OnGUI()
    {
        tilemap = (Tilemap)EditorGUILayout.ObjectField("Tilemap", tilemap, typeof(Tilemap), true);

        if (GUILayout.Button("Export as JSON"))
        {
            ExportTileHints();
        }
    }

    private void ExportTileHints()
    {
        if (tilemap == null)
        {
            Debug.LogWarning("Tilemap을 지정해주세요.");
            return;
        }

        TileNodeList tileList = new TileNodeList();

        BoundsInt bounds = tilemap.cellBounds;
        for (int y = bounds.yMax; y >= bounds.yMin; y--)
        {
            for (int x = bounds.xMin; x <= bounds.xMax; x++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase baseTile = tilemap.GetTile(cellPos);

                if (baseTile is CustomTile customTile)
                {
                    TileNode node = new TileNode
                    {
                        x = x,
                        y = y,
                        TileType = customTile.TileType,

                        // To Do : 임의값으로 하지 말 것, Tile 점과 점 사이 거리 계산
                        requiredJumpPower = customTile.requiredJumpPower
                    };
                    tileList.tiles.Add(node);
                }
            }
        }

        string json = JsonUtility.ToJson(tileList, true);
        string dir = Application.dataPath + "/@Resources/Data/JsonData";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string path = $"{dir}/TileHintMapData.json";
        File.WriteAllText(path, json);

        Debug.Log($"타일 힌트 정보가 저장되었습니다: {path}");
        AssetDatabase.Refresh();
    }
}
