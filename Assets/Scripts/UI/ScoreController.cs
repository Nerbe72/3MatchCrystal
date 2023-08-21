using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public static ScoreController instance;


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

    private void Update()
    {
        
    }

    public void AddScore(int destroyedCount, int combo)
    {
        ScoreManager.instance.TotalScore += (destroyedCount * 100) * Mathf.Clamp((combo / 2), 1, 4);
    }
}
