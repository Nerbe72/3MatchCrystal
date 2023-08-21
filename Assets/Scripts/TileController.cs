using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class TileController : MonoBehaviour
{
    public static TileController instance;

    private Coroutine co = null;

    #region ����
    public int Combo = 0;
    public bool IsSwapRunning = false;

    private bool m_isMatched = true;
    private bool m_firstEnter = true;

    private TileManager tileMng;
    private float m_swapSpeed = 2.5f;
    private float m_dropSpeed = 2.5f;
    private float distance = 0.05f;

    private List<Tile> collapseTiles = new List<Tile>();
    private Dictionary<int, int> collapseX = new Dictionary<int, int>();
    private Dictionary<int, int> m_dropTileCount = new Dictionary<int, int>();

    private List<Tile> m_matchedTiles = new List<Tile>();
    #endregion

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
    }

    private void Start()
    {
        tileMng = TileManager.instance;
    }

    //����� - �����̽��ٷ� searchAll ȣ��
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (co == null)
                StartCoroutine(SearchingAll());
        }
    }

    /*
     * ���� Ÿ�� �ϳ��� ���ڸ��� ���ư��� ����
     * 
     * �ڷ�ƾ �����
     * 
     */

    public void Swap(Tile target, Vector2 direction)
    {
        StartCoroutine(SwapTile(target, direction));
    }

    //Ÿ�� ��ȯ
    private IEnumerator SwapTile(Tile target, Vector2 direction)
    {
        if (target == null) yield break;

        Tile target2 = null;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x < -distance)
            {
                //���� Ÿ�� ����
                target2 = tileMng.TileGrid[target.Grid.x, target.Grid.y - 1];
            }
            else if (direction.x > distance)
            {
                //������ Ÿ�� ����
                target2 = tileMng.TileGrid[target.Grid.x, target.Grid.y + 1];
            }
        }
        else
        {
            if (direction.y < -distance)
            {
                //�Ʒ��� Ÿ�� ����
                target2 = tileMng.TileGrid[target.Grid.x - 1, target.Grid.y];
            }
            else if (direction.y > distance)
            {
                //���� Ÿ�ϰ� ��ȯ
                target2 = tileMng.TileGrid[target.Grid.x + 1, target.Grid.y];
            }
        }

        if (target2 == null) yield break;

        (int x, int y) target_temp = target.Grid;
        (int x, int y) target2_temp = target2.Grid;

        target.SetMove(target2_temp);
        target2.SetMove(target_temp);

        yield return CheckMoving();
        Debug.Log("Swap End");
        yield return SearchingAll();
        Debug.Log("SearchAll End");

        //Ÿ���� ��Ī���� ������ �ǵ���
        if (!m_isMatched &&
            (target != null && target2 != null))
        {
            target.SetMove(target_temp);
            target2.SetMove(target2_temp);
        }
        yield return CheckMoving();

        yield break;
    }

    //��� Ÿ�� Ž�� �� 3match Ȯ��
    private IEnumerator SearchingAll()
    {
        m_isMatched = false;

        for (int x = 0; x < tileMng.xSize; x++)
        {
            for (int y = 0; y < tileMng.ySize; y++)
            {
                if (tileMng.TileGrid[x, y] == null) continue;

                //Ÿ���� id�� ������
                TileID targetID = tileMng.TileGrid[x, y].id;

                if (y >= 2)
                    Matching(targetID, x, y, true); //��

                if (x >= 2)
                    Matching(targetID, x, y, false); //��
            }
        }

        DestroyTile();
        yield return new WaitForEndOfFrame();

        Collapse();
        yield return new WaitForEndOfFrame();
        SetCollapseData();
        yield return CheckMoving();
        yield return new WaitForEndOfFrame();

        Drop();
        yield return CheckMoving();
        yield return new WaitForEndOfFrame();

        ////todo �ݺ�
        if (m_isMatched)
        {
            yield return SearchingAll();
        }

        co = null;
        yield break;
    }

    //��Ī�� Ÿ�� ����
    private void DestroyTile()
    {
        int count = m_matchedTiles.Count;

        for (int i = 0; i < count; i++)
        {
            AddDropCount(m_matchedTiles[i].Grid.y);
            m_matchedTiles[i].BreakThis();
        }
        m_matchedTiles.Clear();
    }

    //���߿� �� Ÿ���� Ž�� �� ����
    private void Collapse()
    {
        //y���� ���� x�� ��ȸ�ϴ� null�� ������ ����� Ž���ϰ� break;
        //collapseX�� ���� null���� y�� ���� x�� ����
        int sizeX = tileMng.xSize;
        int sizeY = tileMng.ySize;

        collapseTiles.Clear();
        collapseX.Clear();

        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                if (tileMng.TileGrid[x, y] == null)
                {
                    for (int aboveX = x + 1; aboveX < sizeX; aboveX++)
                    {
                        if (tileMng.TileGrid[aboveX, y] != null)
                        {
                            collapseTiles.Add(tileMng.TileGrid[aboveX, y]);
                        }

                        //y�� ù �� �ڸ� x ����
                        if (!collapseX.ContainsKey(y))
                            collapseX.Add(y, x);
                    }
                    break;
                }
            }
        }
    }

    //todo ���� �ذ�
    private void SetCollapseData()
    {
        int count = collapseTiles.Count;

        for (int i = 0; i < count; i++)
        {
            int cy = collapseTiles[i].Grid.y;
            (int x, int y)destGrid = (collapseX[cy], cy);
            collapseTiles[i].SetMove(destGrid);
            collapseX[cy]++;
        }
    }

    //Ÿ���� ������ŭ �����ϰ� ����
    private void Drop()
    {

        int[] keys = m_dropTileCount.Keys.ToArray<int>();
        int length = keys.Length;
        
        //Ű �迭 Ž���� ���� �ݺ���
        for(int i = 0; i < length; i++)
        {
            //(Ű��)keys[i] = y��ǥ, droptilecount[Ű] = ������ Ÿ�� ����
            for (int x = m_dropTileCount[keys[i]]; x > 0; x--)
            {
                Vector2 startPos = new Vector2(-2.1f + (keys[i] * 0.7f), 2.2f + (m_dropTileCount[keys[i]] - x * 0.7f));
                Tile obj = tileMng.SetAndReturnTile(startPos, (TileID)UnityEngine.Random.Range((int)TileID.Red, (int)TileID.Count),
                                                        tileMng.xSize - x, keys[i]);

                obj.SetMove((tileMng.xSize - x, keys[i]));
            }
        }

        m_dropTileCount.Clear();
    }
     
    private void Matching(TileID _targetID, int _x, int _y, bool _isHorizontal)
    {
        if (_isHorizontal)
        {
            if ((tileMng.TileGrid[_x, _y - 1].id == tileMng.TileGrid[_x, _y - 2].id) && (tileMng.TileGrid[_x, _y - 1].id == _targetID))
            {
                if (!m_matchedTiles.Contains(tileMng.TileGrid[_x, _y]))
                    m_matchedTiles.Add(tileMng.TileGrid[_x, _y]);

                if (!m_matchedTiles.Contains(tileMng.TileGrid[_x, _y - 1]))
                    m_matchedTiles.Add(tileMng.TileGrid[_x, _y - 1]);

                if (!m_matchedTiles.Contains(tileMng.TileGrid[_x, _y - 2]))
                    m_matchedTiles.Add(tileMng.TileGrid[_x, _y - 2]);

                m_isMatched = true;
            }
        }
        else
        {
            if ((tileMng.TileGrid[_x - 1, _y].id == tileMng.TileGrid[_x - 2, _y].id) && (tileMng.TileGrid[_x - 1, _y].id == _targetID))
            {
                if (!m_matchedTiles.Contains(tileMng.TileGrid[_x, _y]))
                    m_matchedTiles.Add(tileMng.TileGrid[_x, _y]);

                if (!m_matchedTiles.Contains(tileMng.TileGrid[_x - 1, _y]))
                    m_matchedTiles.Add(tileMng.TileGrid[_x - 1, _y]);

                if (!m_matchedTiles.Contains(tileMng.TileGrid[_x - 2, _y]))
                    m_matchedTiles.Add(tileMng.TileGrid[_x - 2, _y]);

                m_isMatched = true;
            }
        }
    }

    private void AddDropCount(int _y)
    {
        if (!m_dropTileCount.ContainsKey(_y))
            m_dropTileCount.Add(_y, 0);

        m_dropTileCount[_y]++;
    }

    private IEnumerator CheckMoving()
    {
        bool isDropping = false;

        while (true)
        {
            yield return new WaitForEndOfFrame();

            isDropping = false;

            for (int i = 0; i < tileMng.xSize; i++)
            {
                for (int j = 0; j < tileMng.ySize; j++)
                {
                    //��� ���� null�� �ƴϸ� �۵����� �ƴ��� Ȯ��
                    if (tileMng.TileGrid[i, j] != null)
                    {
                        if (tileMng.TileGrid[i, j].IsMoving)
                        {
                            isDropping = true;
                            break;
                        }
                    }
                }
            }

            if (!isDropping)
                break;
        }

        yield break;
    }

}
