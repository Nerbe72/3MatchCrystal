using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public static ScoreController instance;

    private ScoreManager m_scoreManager;
    private int beforeScore;
    private int tempScore;
    private Coroutine scoreCo;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        m_scoreManager = ScoreManager.instance;
        beforeScore = m_scoreManager.TotalScore;
    }

    private void Update()
    {
        RedrawScore();
    }

    private void RedrawScore()
    {
        if (beforeScore == m_scoreManager.TotalScore) return;

        if (scoreCo != null)
        {
            StopCoroutine(scoreCo);
        }

        scoreCo = StartCoroutine(ScoreLerp());
        beforeScore = m_scoreManager.TotalScore;
    }

    public void AddScore(int destroyedCount, int combo)
    {
        m_scoreManager.TotalScore += (destroyedCount * 100) * Mathf.Clamp((combo / 2), 1, 4);
    }

    private IEnumerator ScoreLerp()
    {
        float time = 0;
        int currentGeo = m_scoreManager.TotalScore;
        tempScore = beforeScore;

        while (true)
        {
            time += Time.deltaTime * 10f;

            m_scoreManager.ScoreText.text = ((int)Mathf.Lerp(tempScore, currentGeo, time)).ToString();

            //Debug.Log(time);

            if (time >= 1f)
            {
                beforeScore = currentGeo;
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        scoreCo = null;
        yield break;
    }
}
