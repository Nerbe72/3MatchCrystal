using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        
    }

    private void Init()
    {

    }
}
