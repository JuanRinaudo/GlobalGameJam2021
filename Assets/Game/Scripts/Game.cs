using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{

    public static Game instance;

    public Player player;
    public bool running = false;

    public AudioClip victorySound;
    public AudioClip deathSound;

    public GameObject gameGeneratingUI;
    public GameObject gameStartUI;
    public GameObject gameLostUI;
    public GameObject gameWonUI;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gameGeneratingUI.SetActive(false);
        gameStartUI.SetActive(true);
    }

    void Update()
    {
        
    }

    public void PlayGame()
    {
        gameStartUI.SetActive(false);
        running = true;
    }

    public void GameWon()
    {
        Audio.Instance.PlaySound(victorySound);

        gameWonUI.SetActive(true);
        running = false;
    }

    public void GameLost()
    {
        Audio.Instance.PlaySound(deathSound);

        gameLostUI.SetActive(true);
        running = false;
    }

    public void ResetGame()
    {
        running = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
