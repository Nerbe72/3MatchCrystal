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
        InitTileGrid();
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
    
    private void InitTileGrid()
    {
        //크기 초기화
        TileGrid = new Tile[xSize, ySize];

        //타일 그리기
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                Tile obj = Instantiate(m_tilePrefab).GetComponent<Tile>();
                obj.SetData(
                    GetPositionFromXY(x, y),
                    (TileID)Random.Range((int)TileID.Red, (int)TileID.Count),
                    x, y);
                TileGrid[x, y] = obj;

                //if (x != 0)
                //{
                //    TileGrid[x - 1, y].TopTile = TileGrid[x, y];
                //    TileGrid[x, y].BottomTile = TileGrid[x - 1, y];
                //}
            }
        }
    }

}
