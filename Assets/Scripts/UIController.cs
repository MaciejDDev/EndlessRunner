using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIController : MonoBehaviour
{
    [SerializeField] TMP_Text _distanceText;

    [SerializeField] Image _gameOverPanel;
    [SerializeField] TMP_Text _gameOverText;

    public static Action OnLevelRetry;

    void Awake() => Player.Instance.PlayerDied += ShowGameOverScreen;

    private void OnDestroy() => Player.Instance.PlayerDied -= ShowGameOverScreen;

    // Update is called once per frame
    void Update()
    {
        _distanceText.SetText($"{Player.Instance.GetDistance()} m");
    }

    void ShowGameOverScreen()
    {
        _gameOverText.SetText($"You ran: {Player.Instance.GetDistance()} m");
        _gameOverPanel.gameObject.SetActive(true);
    }


    public void OnRetryButton()
    {
        _gameOverPanel.gameObject.SetActive(false);
        OnLevelRetry?.Invoke();
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
