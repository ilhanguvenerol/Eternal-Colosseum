using System.Collections;
using UnityEngine;

public class WaveClearUI : MonoBehaviour
{
    public GameObject waveClearPanel;

    public void ShowWaveClear()
    {
        StartCoroutine(ShowRoutine());
    }

    // Brief transition screen shown before loading the shop scene.
    IEnumerator ShowRoutine()
    {
        waveClearPanel.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        GameLoopManager.Instance.LoadShop();
    }
}