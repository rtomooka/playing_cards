using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SolitaireSceneDirector : MonoBehaviour
{
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] Text textTimer;

    // 土台
    [SerializeField] GameObject stock; // 山札
    [SerializeField] List<Transform> foundation; // 組札
    [SerializeField] List<Transform> column; // 場札

    // カード
    List<CardController> cards; // 全カード
    List<CardController> stockCards; // 手札
    List<CardController> wasteCards; // 交換するカード

    CardController selectCard;
    Vector3 startPosition;

    bool isGameEnd;

    float gameTimer;
    int oldSecond;

    // 組札のサイズ
    const float StackCardHeight = 0.0001f;
    const float StackCardWidth = 0.02f;

    const float SortWasteCardTime = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        cards = cardsDirector.GetShuffleCards();
        wasteCards = new List<CardController>();
        stockCards = new List<CardController>();

        // カードの初期設定
        foreach (var item in cards)
        {
            item.PlayerNo = 0;
            item.FlipCard(false);
            stockCards.Add(item);
        }

        int cardIndex = 0;
        int columnCount = 0;

        foreach (var item in column)
        {
            // ソリティア用にずらしながら配置する
            columnCount++;
            for (int i = 0; i < columnCount; i++)
            {
                // カードの枚数が足りない場合は終了
                if (cards.Count - 1 < cardIndex) break;

                CardController card = cards[cardIndex];
                CardController parent = item.GetComponent<CardController>();
                if (0 != i)
                {
                    parent = cards[cardIndex - 1];
                }

                putCard(parent, card);
                stockCards.Remove(card);
                cardIndex++;
            }
            // 最後のカードのみオープン
            cards[cardIndex - 1].FlipCard();
        }

        stackStockCards();
    }

    // Update is called once per frame
    void Update()
    {
        // ゲームクリア
        if (isGameEnd) return;

        // 経過時間
        gameTimer += Time.deltaTime;
        // タイマー表示
        textTimer.text = getTimerText(gameTimer);

        // マウスON
        if (Input.GetMouseButtonDown(0))
        {
            setSelectCard();
        }
        // ドラッグ中
        else if (Input.GetMouseButton(0))
        {
            moveCard();
        }
        // マウスOFF
        else if (Input.GetMouseButtonUp(0))
        {
            releaseCard();
        }
        else
        {
            // Do Nothing...
        }
    }

    void putCard(CardController parent, CardController child)
    {
        child.transform.parent = parent.transform;
        Vector3 pos = parent.transform.position;

        pos.y += StackCardHeight;

        // 
        if (column.Contains(parent.transform.root) && !column.Contains(parent.transform))
        {
            pos.z -= StackCardWidth;
        }

        child.transform.position = pos;
        wasteCards.Remove(child);
    }

    void stackStockCards()
    {
        // 山札の上に並べる
        for (int i = 0; i < stockCards.Count; i++)
        {
            CardController card = stockCards[i];
            card.FlipCard(false);

            Vector3 pos = stock.transform.position;
            pos.y += (i + 1) * StackCardHeight;
            card.transform.position = pos;
            card.transform.parent = stock.transform;
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

    // スクリーンからワールドポジションへ変換
    Vector3 getScreenToWorldPosition()
    {
        // スクリーン座標を取得
        Vector3 cameraPositon = Input.mousePosition;
        // 足りない高さはカメラ位置を利用
        cameraPositon.z = Camera.main.transform.position.y;
        // 座標変換
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(cameraPositon);
        return worldPosition;
    }

    // めくり札を並べる
    void sortWasteCards()
    {
        float startx = stock.transform.position.x - CardController.Width * 2;

        for (int i = 0; i < wasteCards.Count; i++)
        {
            CardController card = wasteCards[i];
            float x = startx + i * StackCardWidth;
            float y = i * StackCardHeight;
            // アニメーションで移動
            card.transform.DOMove(new Vector3(x, y, stock.transform.position.z), SortWasteCardTime);
        }
    }

    void setSelectCard()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        GameObject obj = hit.collider.gameObject;
        CardController card = obj.GetComponent<CardController>();

        // 選択解除
        selectCard = null;

        // 山札がない場合
        if (obj == stock)
        {
            // めくり札と捨て札を山札に戻す
            stockCards.AddRange(wasteCards);
            foreach (var item in cards)
            {
                if (item.gameObject.activeSelf) continue;

                item.gameObject.SetActive(true);
                stockCards.Add(item);
            }
            wasteCards.Clear();

            // シャッフルしてサイド並べる
            cardsDirector.ShuffleCards(stockCards);
            stackStockCards();
        }

        // カード以外や土台の場合は処理しない
        if (!card || 0 > card.PlayerNo) return;

        // オープン済みのカード
        if (card.isFrontUp)
        {
            // 山札の場合は、1番手前の場合のみ選択する
            if (wasteCards.Contains(card) && card != wasteCards[wasteCards.Count - 1]) return;

            // 元の位置に戻すように位置を保存
            card.HandPosition = card.transform.position;
            // カードを選択
            selectCard = card;
            // ドラッグ用にポジションを取得
            startPosition = getScreenToWorldPosition();
        }
        // 未オープンの場合
        else
        {
            // カードをオープン(共通)
            if (1 > card.transform.childCount)
            {
                card.transform.DORotate(Vector3.zero, SortWasteCardTime).OnComplete(() =>
                {
                    card.FlipCard();
                });
            }
            // 山札の場合はめくり札へ
            if (card.transform.root == stock.transform)
            {
                // 4枚めの場合は、1枚目を破棄
                if (3 < wasteCards.Count + 1)
                {
                    wasteCards[0].gameObject.SetActive(false);
                    wasteCards.RemoveAt(0);
                }

                // 山札からめくり札へ
                stockCards.Remove(card);
                wasteCards.Add(card);

                // 並べなおす
                sortWasteCards();
                stackStockCards();
            }
        }
    }

    // カード移動
    void moveCard()
    {
        if (!selectCard) return;
        // ポジションの差分
        Vector3 diff = getScreenToWorldPosition() - startPosition;
        Vector3 pos = selectCard.transform.position + diff;

        // 移動中はカードを少し浮かせる
        pos.y = 0.01f;

        // ポジションを更新
        selectCard.transform.position = pos;
        startPosition = getScreenToWorldPosition();
    }

    // カードを離す
    void releaseCard()
    {
        if (!selectCard) return;

        // 1番手前のカード
        CardController frontCard = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            // 当たり判定のあるカードを取得
            CardController card = hit.transform.gameObject.GetComponent<CardController>();

            // カード以外、選択中のカードは除外
            if (!card || card == selectCard) continue;

            // 一番手前のカードを取得(子オブジェクトが一番少ない)
            if (!frontCard || frontCard.transform.childCount > card.transform.childCount)
            {
                frontCard = card;
            }
        }

        // 組札に置けるカード
        if (frontCard && foundation.Contains(frontCard.transform.root)
                      && 1 > selectCard.transform.childCount
                      && frontCard.No + 1 == selectCard.No
                      && frontCard.Suit == selectCard.Suit)
        {
            putCard(frontCard, selectCard);

            // クリア判定
            bool fieldEnd = true;
            foreach (var item in column)
            {
                if (0 < item.childCount) fieldEnd = false;
            }

            // カードを全て使用していたらクリア
            isGameEnd = fieldEnd && 1 > wasteCards.Count
                                 && 1 > stockCards.Count;
        }
        // 場札に置けるカード
        else if (frontCard && foundation.Contains(frontCard.transform.root)
                           && 1 > frontCard.transform.childCount
                           && frontCard.No - 1 == selectCard.No
                           && frontCard.SuitColor != selectCard.SuitColor)
        {
            putCard(frontCard, selectCard);
        }
        // 置けない場合
        else
        {
            selectCard.transform.position = selectCard.HandPosition;
        }
    }

    // 再読み込み
    public void OnClickRestart()
    {
        SceneManager.LoadScene("SolitaireScene");
    }
}
