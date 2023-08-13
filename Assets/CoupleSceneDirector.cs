using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CoupleSceneDirector : MonoBehaviour
{
    // カード管理
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] Text textTimer;
    List<CardController> cards;

    // 並べるカードの縦横
    int width = 4;
    int height = 4;

    // 選択中のカード
    CardController selectCard;
    List<CardController> fieldCards;

    // タイマー処理用
    bool isGameEnd;
    float gameTimer;
    int oldSecond;

    // シャッフル時のアニメーション時間
    const float FieldResetTime = 1;

    [SerializeField] Text textClear;

    // Start is called before the first frame update
    void Start()
    {
        cards = cardsDirector.GetShuffleCards();
        resetField(FieldResetTime);
        isGameEnd = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameEnd) return;
        // タイマー更新
        gameTimer += Time.deltaTime;
        textTimer.text = getTimerText(gameTimer);

        if (Input.GetMouseButtonUp(0))
        {
            matchCards();
        }
    }

    // カードを等間隔に並べる
    List<CardController> sortFieldCards(List<CardController> cards, int width, int height, float speed)
    {
        List<CardController> ret = new List<CardController>();

        Vector2 offset = new Vector2((width - 1) / 2.0f, (height - 1) / 2.0f);

        for (int ii = 0; ii < width * height; ii++)
        {
            if (cards.Count - 1 < ii) break;
            if (!cards[ii]) continue;

            // インデックス更新
            Vector2Int index = new Vector2Int(ii % width, ii / width);
            cards[ii].Index = ii;
            cards[ii].IndexPosition = index;

            // 新しい表示位置
            Vector3 pos = new Vector3(
                (index.x - offset.x) * CardController.Width,
                0,
                (index.y - offset.y) * CardController.Height);

            // 距離
            float dist = Vector3.Distance(cards[ii].transform.position, pos);

            // アニメーション
            cards[ii].transform.DOMove(pos, dist / speed);
            cards[ii].transform.DORotate(new Vector3(0, 0, 0), dist / speed);

            // カードを保存
            ret.Add(cards[ii]);
        }

        return ret;
    }

    // デッキからフィールドへカードを配置
    void resetField(float speed)
    {
        // デッキへ移動
        foreach (var item in cards)
        {
            item.PlayerNo = -1;
            item.IndexPosition = new Vector2Int(-100, -100);
            item.transform.position = new Vector3(0.2f, 0, -0.15f);
            item.FlipCard(false);
        }

        // 並んでいるカードを取得
        fieldCards = sortFieldCards(cards, width, height, speed);

        // フィールドカードを選択可能にする
        foreach (var item in fieldCards)
        {
            item.PlayerNo = 0;
            // デッキから削除
            cards.Remove(item);
        }
    }

    void setSelectCard(CardController card = null)
    {
        Vector3 pos;
        // 一旦非選択表示
        if (selectCard)
        {
            pos = selectCard.transform.position;
            selectCard.transform.position = new Vector3(pos.x, 0, pos.z);
            selectCard = null;
        }

        if (!card) return;

        // 選択状態へ
        selectCard = card;
        pos = selectCard.transform.position;
        pos.y += 0.02f;
        selectCard.transform.position = pos;

    }

    void matchCards()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        // ヒットしたカードを取得
        CardController card = hit.collider.gameObject.GetComponent<CardController>();

        // カード未ヒットなら処理しない
        if (!card) return;

        // カード同士のインデックスの距離
        int dist = 0;
        if (selectCard)
        {
            dist = (int)Vector2Int.Distance(selectCard.IndexPosition, card.IndexPosition);
        }
        // 2枚目選択
        if (1 == dist && selectCard.No == card.No)
        {
            // フィールドのインデックスを更新
            fieldCards[selectCard.Index] = null;
            fieldCards[card.Index] = null;

            // そろったカードを非表示
            selectCard.gameObject.SetActive(false);
            card.gameObject.SetActive(false);

            // カードを詰める
            for (int ii = 0; ii < fieldCards.Count; ii++)
            {
                if (fieldCards[ii]) continue;
                // カードを探す
                for (int jj = ii + 1; jj < fieldCards.Count; jj++)
                {
                    if (!fieldCards[jj]) continue;
                    fieldCards[ii] = fieldCards[jj];
                    fieldCards[jj] = null;
                    break;
                }
            }

            setSelectCard();
            sortFieldCards(fieldCards, width, height, FieldResetTime / 2);

            bool endField = true;
            foreach (var item in fieldCards)
            {
                if (item)
                {
                    endField = false;
                    break;
                }
            }

            isGameEnd = endField && (1 > cards.Count);
            if (isGameEnd)
            {
                textClear.gameObject.SetActive(true);
            }            
        }
        // カード選択
        else if (0 == card.PlayerNo)
        {
            setSelectCard(card);
        }

        // デッキからカードを1枚出す
        else
        {
            for (int ii = 0; ii < fieldCards.Count; ii++)
            {
                if (fieldCards[ii]) continue;
                fieldCards[ii] = cards[0];
                fieldCards[ii].PlayerNo = 0;
                cards.RemoveAt(0);
                break;
            }

            setSelectCard();
            sortFieldCards(fieldCards, width, height, FieldResetTime);
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

    public void OnClickShuffle()
    {
        // アニメーションを停止
        if (null != DOTween.PlayingTweens())
        {
            foreach (var item in DOTween.PlayingTweens())
            {
                item.Kill();
            }
        }

        // フィールドのカードをデッキに戻す
        foreach (var item in fieldCards)
        {
            if (!item) continue;
            cards.Add(item);
        }
        fieldCards.Clear();

        // デッキをシャッフル
        cardsDirector.ShuffleCards(cards);

        // フィールドに再配置
        resetField(FieldResetTime / 2);
    }
    
    public void OnClickRestart()
    {
        SceneManager.LoadScene("CoupleScene");
    }
}
