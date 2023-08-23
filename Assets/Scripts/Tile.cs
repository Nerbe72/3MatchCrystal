using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer m_sprite;
    private TileManager m_tileMng;

    #region Public Data
    public TileID id;
    public bool IsMoving = false;
    public (int x, int y)Grid;
    #endregion

    private float check = 0;
    private float m_fallingSpeed = 2.8f;

    private Vector2 startPosition;
    private Vector2 destPosition;

    private void OnEnable()
    {
        m_sprite = GetComponent<SpriteRenderer>();
        m_tileMng = TileManager.instance;

        SetColor((TileID)UnityEngine.Random.Range((int)TileID.Red, (int)TileID.Count));
        startPosition = m_tileMng.GetPositionFromXY(Grid.x, Grid.y);
    }

    private void Update()
    {
        Moving();
    }

    private void Moving()
    {
        if (IsMoving)
        {
            check += Time.deltaTime * m_fallingSpeed;

            transform.position = Vector2.Lerp(startPosition, destPosition, check);

            if (check >= 1f)
            {
                IsMoving = false;
                startPosition = destPosition;
                check = 0f;
            }
        }
    }

    public void SetMove((int x, int y)destGrid)
    {
        IsMoving = true;
        SetXY(destGrid.x, destGrid.y);
        destPosition = m_tileMng.GetPositionFromXY(Grid.x, Grid.y);
    }

    public void SetData(Vector2 _pos, TileID _id, int _x, int _y)
    {
        transform.position = _pos;
        startPosition = _pos;

        id = _id;
        m_sprite.sprite = m_tileMng.TileColor[_id];
        SetXY(_x, _y);
    }

    public void SetColor(TileID _id)
    {
        id = _id;
        m_sprite.sprite = m_tileMng.TileColor[_id];
    }

    public void SetXY(int _x, int _y)
    {
        Grid.x = _x;
        Grid.y = _y;
        gameObject.name = $"{_x}x{_y}";
        m_tileMng.TileGrid[Grid.x, Grid.y] = this;
    }

    public void SetMove(int _x, int _y)
    {
        SetXY(_x, _y);
        destPosition = m_tileMng.GetPositionFromXY(Grid.x, Grid.y);
        check = 0f;
        IsMoving = true;
    }

    public void BreakThis()
    {
        m_tileMng.TileGrid[Grid.x, Grid.y] = null;
        Destroy(gameObject);
        Destroy(this);
    }
}
