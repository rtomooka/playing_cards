using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TopSceneDirector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // カップル
    public void OnClickToCouple()
    {
        SceneManager.LoadScene("CoupleScene");
    }

    // High & Low
    public void OnClickToHighLow()
    {
        SceneManager.LoadScene("HighLowScene");
    }

    // Memory
    public void OnClickToMemory()
    {
        SceneManager.LoadScene("MemoryScene");
    }

    // Black Jack
    public void OnClickToBlackJack()
    {
        SceneManager.LoadScene("BlackJackScene");
    }

    // Poker
    public void OnClickToPoker()
    {
        SceneManager.LoadScene("PokerScene");
    }

    // Solitaire
    public void OnClickToSolitaire()
    {
        SceneManager.LoadScene("SolitaireScene");
    }

    public void OnClick_1920x1080()
    {
        // 解像度を1080pに設定
        Screen.SetResolution(1920, 1080, false);
    }

    public void OnClick_1280x720()
    {
        // 解像度を720pに設定
        Screen.SetResolution(1280, 720, false);
    }

    public void OnClick_960x540()
    {
        // 解像度を540pに設定
        Screen.SetResolution(960, 540, false);
    }
}
