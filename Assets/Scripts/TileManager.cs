using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;

    public GameObject m_tilePrefab;
    private float xStartPos = -2.1f;
    private float yStartPos = -4f;

    [HideInInspector] public Dictionary<TileID, Sprite> TileColor = new Dictionary<TileID, Sprite>();
    [HideInInspector] public Tile[,] TileGrid;
    [HideInInspector] public List<Tile> Tiles = new List<Tile>();
    public int xSize = 8;
    public int ySize = 7;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        InitTileColorData();

        //크기 초기화
        TileGrid = new Tile[xSize, ySize];
    }

    public Vector2 GetPositionFromXY(int _x, int _y)
    {
        return new Vector2(xStartPos + (_y * 0.7f), yStartPos + (_x * 0.7f));
    }

    public Tile SetAndReturnTile(Vector2 _pos, TileID _id, int _x, int _y)
    {
        Tile obj = Instantiate(m_tilePrefab).GetComponent<Tile>();
        obj.SetData(_pos, _id, _x, _y);

        return obj;
    }

    private void InitTileColorData()
    {
        for (int i = 0; i < (int)TileID.Count; i++)
        {
            TileColor.Add((TileID)i, Resources.Load<Sprite>("Top_View/" + ((TileID)i).ToString()));
        }
    }
    
    public void InitTileGrid()
    {
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (TileGrid[x, y] != null)
                    TileGrid[x, y].BreakThis();
                GenerateTile(x, y);
            }
        }
    }

    public void GenerateTile(int x, int y)
    {
        Tile obj = Instantiate(m_tilePrefab).GetComponent<Tile>();
        obj.SetData(
            GetPositionFromXY(x, y),
            (TileID)Random.Range((int)TileID.Red, (int)TileID.Count),
            x, y);
        TileGrid[x, y] = obj;
    }

}
