using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "CustomTile", menuName = "Tiles/CustomTile")]
public class CustomTile : Tile
{
    [Space] [Space] [Header("For Design")]
    public Define.ETileType TileType;
    public float requiredJumpPower;
}
