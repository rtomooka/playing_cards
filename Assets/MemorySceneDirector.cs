using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 神経衰弱
public class MemorySceneDirector : MonoBehaviour
{
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] Text textTimer;

    List<CardController> cards;

    // 縦横のカード枚数
    int width = 5;
    int height = 4;

    List<CardController> selecetCards;
    int selectCountMax = 2;

    // タイマー用の変数群
    bool isGameEnd;
    float gameTimer;
    int oldSecond;

    // Start is called before the first frame update
    void Start()
    {
        cards = cardsDirector.GetMemoryCards();

        // 配置用のオフセット値
        Vector2 offset = new Vector2((width - 1) / 2.0f, (height - 1) / 2.0f);

        if (cards.Count < width * height)
        {
            Debug.LogError("カードが足りません");
        }

        // カードを並べる
        for (int i=0; i<width*height; i++)
        {
            // 横
            float x = (i % width - offset.x) * CardController.Width;

            // 縦
            float y = (i / width - offset.y) * CardController.Height;

            cards[i].transform.position = new Vector3(x, 0, y);
            cards[i].FlipCard(false);
        }

        selecetCards = new List<CardController>();
        oldSecond = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if(isGameEnd) return;
        // タイマー更新
        gameTimer += Time.deltaTime;
        textTimer.text = getTimerText(gameTimer);

        if (Input.GetMouseButtonUp(0))
        {
            // 3回目のタップ
            if (!canOpen()) return;

            // 当たり判定
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // ヒットしたカードを取得
                CardController card = hit.collider.gameObject.GetComponent<CardController>();

                // カード未ヒットまたは選択済みなら処理しない
                if (!card || selecetCards.Contains(card)) return;

                card.FlipCard();
                selecetCards.Add(card);
            }
        }
    }

    // タイマー値
    string getTimerText(float timer)
    {
        int sec = (int)timer % 60;
        string ret = textTimer.text;

        if (oldSecond != sec)
        {
            int min = (int)timer / 60;
            string pmin = string.Format("{0:D2}", min);
            string psec = string.Format("{0:D2}", sec);

            ret = pmin + ":" + psec;
            oldSecond = sec;
        }
        return ret;
    }

    // めくったカードの判定
    bool canOpen()
    {
        if (selecetCards.Count < selectCountMax) return true;

        bool equal = true;
        foreach (var item in selecetCards)
        {
            item.FlipCard(false);
            // 数字判定
            if (item.No != selecetCards[0].No)
            {   
                equal = false;
            }
        }

        if (equal)
        {
            // ペアを非表示にする
            foreach (var item in selecetCards)
            {
                item.gameObject.SetActive(false);
            }

            // 全体が非表示になっている場合はゲームクリア
            isGameEnd = true;
            foreach (var item in cards)
            {
                if (item.gameObject.activeSelf)
                {
                    isGameEnd = false;
                    break;
                }                
            }

            if (isGameEnd)
            {
                textTimer.text = "クリア!!" + getTimerText(gameTimer);
            }
        }

        selecetCards.Clear();
        return false;
    }
}
