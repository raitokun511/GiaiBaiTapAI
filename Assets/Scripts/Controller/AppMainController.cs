using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppMainController : MonoBehaviour
{
    [SerializeField]
    RectTransform MainPanel;
    [SerializeField]
    RectTransform SolveTabBtn;
    [SerializeField]
    RectTransform LearnTabBtn;
    [SerializeField]
    RectTransform InfoTabBtn;

    [SerializeField]
    RectTransform SolveFragment;
    [SerializeField]
    RectTransform LearnFragment;
    [SerializeField]
    RectTransform InfoFragment;


    // Start is called before the first frame update
    void Start()
    {
        DisableAllTab();
        OnSolveTabClick();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSolveTabClick()
    {
        DisableAllTab();
        DisableAllFragment();

        EnableTab(SolveTabBtn);
        SolveFragment.gameObject.SetActive(true);
    }

    public void OnDictTabClick()
    {
        DisableAllTab();
        DisableAllFragment();

        EnableTab(LearnTabBtn);
        LearnFragment.gameObject.SetActive(true);

    }

    public void OnInfoTabClick()
    {
        DisableAllTab();
        DisableAllFragment();

        EnableTab(InfoTabBtn);
        InfoFragment.gameObject.SetActive(true);

    }

    public void DisableAllTab()
    {
        foreach (Transform t in MainPanel.transform)
        {
            RectTransform tRect = t.GetComponent<RectTransform>();
            SetStateTab(tRect, false);
        }
    }
    void DisableAllFragment()
    {
        SolveFragment.gameObject.SetActive(false);
        LearnFragment.gameObject.SetActive(false);
        InfoFragment.gameObject.SetActive(false);
    }

    void EnableTab(RectTransform tab)
    {
        SetStateTab(tab, true);
    }
    void SetStateTab(RectTransform tab, bool enable)
    {
        if (tab.childCount > 1)
        {
            Image image = tab.GetChild(0).GetComponent<Image>();
            Text text = tab.GetChild(1).GetComponent<Text>();
            if (enable)
            {

                image.color = Color.black;
                text.color = Color.black;
                tab.GetComponent<Image>().color = new Color(0.82f, 0.82f, 0.82f, 1);
            }
            else
            {
                image.color = AppConstant.UNSELECTED_COLOR;
                text.color = AppConstant.UNSELECTED_COLOR;
                tab.GetComponent<Image>().color = Color.white;
            }
        }
    }

}
