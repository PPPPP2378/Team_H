using UnityEngine;
using System.Collections.Generic;

public class PanelSwitcher : MonoBehaviour
{
    public List<GameObject> pages;   // ページを順番に登録（Page1, Page2, Page3...）
    private int currentPage = 0;

    private void Start()
    {
        ShowPage(0);  // 最初は1ページ目を表示
    }

    public void ShowPage(int index)
    {
        if (index < 0 || index >= pages.Count) return;

        // すべて非表示
        foreach (var page in pages)
        {
            page.SetActive(false);
        }

        // 指定ページだけ表示
        pages[index].SetActive(true);
        currentPage = index;
    }

    public void NextPage()
    {
        int next = currentPage + 1;
        if (next < pages.Count)
        {
            ShowPage(next);
        }
    }

    public void PrevPage()
    {
        int prev = currentPage - 1;
        if (prev >= 0)
        {
            ShowPage(prev);
        }
    }
}
