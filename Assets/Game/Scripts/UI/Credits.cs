using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Credits : MonoBehaviour
{
    [Tooltip("�������� ����� ������ ������")]
    [SerializeField] float delay;
    [Tooltip("������������ ������ ������")]
    [SerializeField] float duration;
    [Tooltip("���������� ������������ �� ������ ������ ������")]
    [SerializeField] float timePerSymbol;
    [Tooltip("������������� ��������� ������")]
    [SerializeField] float appearanceIntence;
    [Tooltip("������������� ������������ ������")]
    [SerializeField] float disappearanceIntence;
    [Tooltip("������ ��������� ����������")]
    [TextArea(5,10)]
    [SerializeField] List<string> content = new List<string>();
    [Tooltip("����������� ���������")]
    [SerializeField] GameObject tooltip;
    TextMeshProUGUI tMesh;
    int index = 0;
    bool waitAnyKey = false;

    IEnumerator Hide() //������������ ������
    {

        yield return new WaitForSeconds(disappearanceIntence);
        if (tMesh.alpha >= 0f)
        {
            tMesh.alpha -= disappearanceIntence;
            StartCoroutine(Hide());
        }
        else
        {
            yield return new WaitForSeconds(delay);
            Next();
        }
    }

    IEnumerator Show() //��������� ������
    {

        yield return new WaitForSeconds(appearanceIntence);
        if (tMesh.alpha <= 1f)
        {
            tMesh.alpha += appearanceIntence;
            StartCoroutine(Show());
        }
        else
        {
            yield return new WaitForSeconds(duration + tMesh.text.Length*timePerSymbol);
            StartCoroutine(Hide());
        }
    }

    void Next() //��������� �����
    {
        index++;
        if (index < content.Count)
        {
            tMesh.text = content[index];
            StartCoroutine(Show());
        }
        else
        {
            StartCoroutine(ShowTooltip());
        }
    }
    IEnumerator ShowTooltip()
    {
        yield return new WaitForSeconds(delay);
        tooltip.SetActive(true);
        waitAnyKey = true;
    }
    void BackToMainMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    private void Start()
    {
        Time.timeScale = 1;
        tMesh = GetComponent<TextMeshProUGUI>();
        tMesh.text = content[0];
        tMesh.alpha = 0;
        StartCoroutine(Show());
    }
    private void Update()
    {
        if (waitAnyKey&&(Input.anyKey))
        {
            BackToMainMenu();
        }
    }
}
