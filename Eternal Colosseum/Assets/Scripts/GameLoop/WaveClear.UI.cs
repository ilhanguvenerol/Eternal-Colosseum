using System.Collections;
using UnityEngine;

public class WaveClearUI : MonoBehaviour
{
    public GameObject waveClearPanel;

    private void Awake()
    {
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.waveClearUI = this;
        }
    }

    public void ShowWaveClear()
    {
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        waveClearPanel.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        GameLoopManager.Instance.LoadShop();
    }
}