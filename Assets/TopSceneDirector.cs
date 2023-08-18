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
}
