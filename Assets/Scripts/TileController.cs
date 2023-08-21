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

    #region 변수
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

    //디버그 - 스페이스바로 searchAll 호출
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (co == null)
                StartCoroutine(SearchingAll());
        }
    }

    /*
     * 가끔 타일 하나가 제자리로 돌아가는 문제
     * 
     * 코루틴 덜어내기
     * 
     */

    public void Swap(Tile target, Vector2 direction)
    {
        StartCoroutine(SwapTile(target, direction));
    }

    //타일 교환
    private IEnumerator SwapTile(Tile target, Vector2 direction)
    {
        if (target == null) yield break;

        Tile target2 = null;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x < -distance)
            {
                //왼쪽 타일 선택
                target2 = tileMng.TileGrid[target.Grid.x, target.Grid.y - 1];
            }
            else if (direction.x > distance)
            {
                //오른쪽 타일 선택
                target2 = tileMng.TileGrid[target.Grid.x, target.Grid.y + 1];
            }
        }
        else
        {
            if (direction.y < -distance)
            {
                //아래쪽 타일 선택
                target2 = tileMng.TileGrid[target.Grid.x - 1, target.Grid.y];
            }
            else if (direction.y > distance)
            {
                //위쪽 타일과 교환
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

        //타일이 매칭되지 않으면 되돌림
        if (!m_isMatched &&
            (target != null && target2 != null))
        {
            target.SetMove(target_temp);
            target2.SetMove(target2_temp);
        }
        yield return CheckMoving();

        yield break;
    }

    //모든 타일 탐색 및 3match 확인
    private IEnumerator SearchingAll()
    {
        m_isMatched = false;

        for (int x = 0; x < tileMng.xSize; x++)
        {
            for (int y = 0; y < tileMng.ySize; y++)
            {
                if (tileMng.TileGrid[x, y] == null) continue;

                //타겟의 id를 가져옴
                TileID targetID = tileMng.TileGrid[x, y].id;

                if (y >= 2)
                    Matching(targetID, x, y, true); //우

                if (x >= 2)
                    Matching(targetID, x, y, false); //상
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

        ////todo 반복
        if (m_isMatched)
        {
            yield return SearchingAll();
        }

        co = null;
        yield break;
    }

    //매칭된 타일 제거
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

    //공중에 뜬 타일을 탐색 및 저장
    private void Collapse()
    {
        //y값에 따른 x를 순회하다 null을 만나면 상단을 탐색하고 break;
        //collapseX에 최초 null값의 y에 따른 x값 저장
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

                        //y별 첫 빈 자리 x 저장
                        if (!collapseX.ContainsKey(y))
                            collapseX.Add(y, x);
                    }
                    break;
                }
            }
        }
    }

    //todo 문제 해결
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

    //타일을 갯수만큼 생성하고 떨굼
    private void Drop()
    {

        int[] keys = m_dropTileCount.Keys.ToArray<int>();
        int length = keys.Length;
        
        //키 배열 탐색을 위한 반복문
        for(int i = 0; i < length; i++)
        {
            //(키값)keys[i] = y좌표, droptilecount[키] = 부족한 타일 갯수
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
                    //모두 돌며 null이 아니며 작동중이 아닌지 확인
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
