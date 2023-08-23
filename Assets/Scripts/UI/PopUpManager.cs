using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    public static PopUpManager instance;

    [SerializeField] private GameObject m_popUp;
    [SerializeField] private TMP_Text m_popUpText;
    [SerializeField] private TMP_Text m_popUpScore;
    [SerializeField] private Button m_start;
    [SerializeField] private Button m_end;

    private void Awake()
    {
        Time.timeScale = 0f;
        Init();
    }

    private void Init()
    {
        m_popUp.SetActive(true);
        m_popUpText.text = "";
        m_popUpScore.text = "";
        m_start.onClick.AddListener(GameStart);
        m_end.onClick.AddListener(GameEnd);
    }

    private void GameStart()
    {
        m_popUp.SetActive(false);
        TileManager.instance.InitTileGrid();
        TileController.instance.FixTile();
        ScoreManager.instance.TotalScore = 0;
        ScoreManager.instance.ScoreText.text = "0";
        Time.timeScale = 1f;
        TimeController.instance.StartTime();
    }

    private void GameEnd()
    {

    }
}
