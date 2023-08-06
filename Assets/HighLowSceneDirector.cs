using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighLowSceneDirector : MonoBehaviour
{
    // 共通カード管理クラス
    [SerializeField] CardsDirector cardsDirector;

    // UI
    [SerializeField] GameObject buttonHigh;
    [SerializeField] GameObject buttonLow;
    [SerializeField] Text textInfo;

    // ゲームで使うカード
    List<CardController> cards;

    // 現在のインデックス
    int cardIndex;

    // 勝利数
    int winCount;

    // 待機時間
    const float nextWaitTimer = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        // シャッフルされたカードを取得
        cards = cardsDirector.GetHighLowCards();

        // 初期位置と向きを設定
        for (int ii = 0; ii < cards.Count; ii++)
        {
            cards[ii].transform.position = new Vector3(0, 0, 0.15f);
            cards[ii].FlipCard(false);
        }

        // 2枚配る
        dealCards();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 2枚配る
    void dealCards()
    {
        cards[cardIndex].transform.position = new Vector3(-0.05f, 0, 0);
        cards[cardIndex].GetComponent<CardController>().FlipCard();
        cards[cardIndex + 1].transform.position = new Vector3(0.05f, 0, 0);
        setHighLowButtons(true);
    }

    // ボタンの表示・非表示
    void setHighLowButtons(bool active)
    {
        buttonHigh.SetActive(active);
        buttonLow.SetActive(active);
    }

    // HIGHボタンの選択
    public void OnClickHigh()
    {
        setHighLowButtons(false);
        checkHighLow(true);
    }

    public void OnClickLow()
    {
        setHighLowButtons(false);
        checkHighLow(false);
    }

    void checkHighLow(bool high)
    {
        // 右のカードをオープン
        cards[cardIndex + 1].GetComponent<CardController>().FlipCard();

        string result = "LOSE... : ";

        int leftNo = cards[cardIndex].GetComponent<CardController>().No;
        int rightNo = cards[cardIndex + 1].GetComponent<CardController>().No;

        if (leftNo == rightNo)
        {
            result = "NO GAME!!";
        }
        // Highを選択した場合
        else if (high) 
        {
            if (leftNo < rightNo) 
            {
                winCount++;
                result = "WIN!! : ";
                GetComponent<AudioSource>().Play();
            }
        }
        // Lowを選択した場合
        else
        {
            if (leftNo > rightNo) 
            {
                winCount++;
                result = "WIN!! : ";
                GetComponent<AudioSource>().Play();
            }
        }

        // 結果を追加
        textInfo.text = result + winCount;

        // 次のゲームを用意
        StartCoroutine(nextCards());

    }

    // 次のゲーム
    IEnumerator nextCards()
    {
        // 指定時間の待機
        yield return new WaitForSeconds(nextWaitTimer);

        // 前回のゲームのカードを非表示
        cards[cardIndex].gameObject.SetActive(false);
        cards[cardIndex + 1].gameObject.SetActive(false);

        // 次のカード
        cardIndex += 2;

        // カード枚数が足りない場合は終了
        if (cards.Count - 1 <= cardIndex)
        {
            textInfo.text = "終了!! " + winCount;
        }
        // 次のゲームのカードを配る
        else
        {
            dealCards();
        }
    }

    public void OnClickRestart() 
    {
        SceneManager.LoadScene("HighLowScene");
    }
}
