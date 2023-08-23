using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class TouchController : MonoBehaviour
{
    private Tile target;
    private Vector2 m_startPos;
    private Vector2 m_endPos;

    private void Update()
    {
        //Ÿ���� �������� �ְų� Ÿ���� ���������� �� ���콺 ���� ����
        if (TileController.instance.isDropping) return;

        MouseDown();
        MouseUp();
    }

    private void MouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_startPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Tile"))
                    target = hit.collider.GetComponent<Tile>();
            }
        }
    }

    private void MouseUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            m_endPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            Vector2 dist = m_endPos - m_startPos;

            TileController.instance.Swap(target, dist);

            //�Ϸ��� �ʱ�ȭ
            m_startPos = Vector2.zero;
            m_endPos = Vector2.zero;
            target = null;
        }
    }
}
