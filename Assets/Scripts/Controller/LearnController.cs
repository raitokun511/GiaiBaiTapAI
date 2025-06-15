using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearnController : MonoBehaviour
{
    public static LearnController instance;

    [SerializeField]
    RectTransform LearnPractiseTab;
    [SerializeField]
    RectTransform LearnMainTab;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void OpenLearnTab(int subject)
    {
        LearnMainTab.gameObject.SetActive(false);
        LearnPractiseTab.gameObject.SetActive(true);
    }


}
