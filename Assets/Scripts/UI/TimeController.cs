using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController instance;

    public TMP_Text TimeText;

    [SerializeField] private int MaxTimeSeconds;
    public TimeSpan MaxTime;
    public TimeSpan CurrentTime;

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

        MaxTime = TimeSpan.FromSeconds(MaxTimeSeconds);
        CurrentTime = MaxTime;
        TimeText.text = CurrentTime.Minutes + ":" + CurrentTime.Seconds;
    }

    private void OnValidate()
    {
        MaxTimeSeconds = Mathf.Clamp(MaxTimeSeconds, 1, 999);
    }

    public void StartTime()
    {
        StartCoroutine(TimeOn());
    }

    private IEnumerator TimeOn()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);

            CurrentTime -= TimeSpan.FromSeconds(1);
            TimeText.text = CurrentTime.Minutes + ":" + CurrentTime.Seconds;

            if (CurrentTime == TimeSpan.Zero)
            {
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        //타임오버 - Game End
        Debug.Log("Time Over");

        yield break;
    }
}
