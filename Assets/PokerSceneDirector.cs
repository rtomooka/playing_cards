using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PokerSceneDirector : MonoBehaviour
{
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] Button buttonBetCoin;
    [SerializeField] Button buttonPlay;
    [SerializeField] Button buttonChange;
    [SerializeField] Text textGameInfo;
    [SerializeField] Text textRate;

    // ボタン内部のテキスト
    Text textButtonBetCoin;
    Text textButtonChange;

    // カード
    List<CardController> cards; // 全カード
    List<CardController> hand; // 手札
    List<CardController> selectCards; // 交換するカード

    // 山札のインデックス
    int dealCardCount;

    // 手持ちコイン
    [SerializeField] int playerCoin;

    // 交換できる回数
    [SerializeField] int cardChangeCountMax;

    // ベットしたコイン
    int betCoin;
    // 交換した回数
    int cardChangeCount;

    //　掛け率
    int straightFlushRate = 10;
    int fourCardRate = 8;
    int fullHouseRate = 6;
    int flushRate = 5;
    int straightRate = 4;
    int threeCardRate = 3;
    int twoPairRate = 2;
    int onePairRate = 1;

    // アニメーション時間
    const float SortHandTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        cards = cardsDirector.GetShuffleCards();

        hand = new List<CardController>();
        selectCards = new List<CardController>();

        textButtonBetCoin = buttonBetCoin.GetComponentInChildren<Text>();
        textButtonChange = buttonChange.GetComponentInChildren<Text>();

        // 山札初期化
        restartGame(false);

        // テキストとボタンを更新
        updateTexts();
        setButtonInPlay(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                CardController card = hit.collider.gameObject.GetComponent<CardController>();
                setSelectCard(card);
            }
        }
    }

    // 山札からカードを引く
    CardController addHand()
    {
        CardController card = cards[dealCardCount++];
        hand.Add(card);
        return card;
    }

    // カードをオープン
    void openHand(CardController card)
    {
        card.transform.DORotate(Vector3.zero, SortHandTime).OnComplete(() => { card.FlipCard(); });
    }

    // 手札を並べる
    void sortHand()
    {
        float x = -CardController.Width * 2;
        foreach (var item in hand)
        {
            Vector3 pos = new Vector3(x, 0, 0);
            item.transform.DOMove(pos, SortHandTime);
            x += CardController.Width;
        }
    }

    // ゲームの開始
    void restartGame(bool deal = true)
    {
        // 手札・選択カードをリセット
        hand.Clear();
        selectCards.Clear();

        // 交換回数をリセット
        cardChangeCount = cardChangeCountMax;
        dealCardCount = 0;

        // カードをシャッフルして山札位置へ
        cardsDirector.ShuffleCards(cards);
        foreach (var item in cards)
        {
            item.gameObject.SetActive(true);
            item.FlipCard(false);
            item.transform.position = new Vector3(0, 0, 0.18f);
        }

        // 以下、配る処理
        if (!deal) return;
        for (int i = 0; i < 5; i++)
        {
            openHand(addHand());
        }

        sortHand();
    }

    // レート表の更新
    void updateTexts()
    {
        textButtonBetCoin.text = "手持ちコイン" + playerCoin;
        textGameInfo.text = "BET枚数" + betCoin;

        textRate.text = "ストレートフラッシュ " + (straightFlushRate * betCoin) + "\n"
            + "フォーカード " + (fourCardRate * betCoin) + "\n"
            + "フルハウス " + (fullHouseRate * betCoin) + "\n"
            + "フラッシュ " + (flushRate * betCoin) + "\n"
            + "ストレート " + (straightRate * betCoin) + "\n"
            + "スリーカード " + (threeCardRate * betCoin) + "\n"
            + "ツーペア " + (twoPairRate * betCoin) + "\n"
            + "ワンペア " + (onePairRate * betCoin) + "\n";
    }

    // ゲーム中のボタン表示
    void setButtonInPlay(bool disp = true)
    {
        textButtonChange.text = "終了";
        // 交換ボタン
        buttonChange.gameObject.SetActive(disp);

        // ベット・プレイボタン
        buttonBetCoin.gameObject.SetActive(!disp);
        buttonPlay.gameObject.SetActive(!disp);
    }

    // コインのベット
    public void OnClickBetCoin()
    {
        // コインが不足している場合は処理しない
        if (1 > playerCoin) return;

        playerCoin--;
        betCoin++;
        updateTexts();
    }

    // ゲームプレイ
    public void OnClickPlay()
    {
        restartGame();
        setButtonInPlay();
        updateTexts();
    }

    void setSelectCard(CardController card)
    {
        if (!card || !card.isFrontUp) return;

        Vector3 pos = card.transform.position;

        // 2回目の選択で非選択へ
        if (selectCards.Contains(card))
        {
            pos.z -= 0.02f;
            selectCards.Remove(card);
        }
        // 選択状態へ
        else if (cards.Count > dealCardCount + selectCards.Count)
        {
            pos.z += 0.02f;
            selectCards.Add(card);
        }

        // カード
        card.transform.position = pos;
        // ボタン更新
        textButtonChange.text = "交換";
        if (1 > selectCards.Count)
        {
            textButtonChange.text = "終了";
        }
    }

    // カードの交換
    public void OnClickChange()
    {
        // 交換しない場合は、直ぐに役の清算を行う
        if (1 > selectCards.Count)
        {
            cardChangeCount = 0;
        }

        // カードの交換処理
        foreach (var item in selectCards)
        {
            item.gameObject.SetActive(false);
            hand.Remove(item);

            openHand(addHand());
        }

        selectCards.Clear();
        sortHand();
        setButtonInPlay();

        // 残り交換可能回数を更新
        cardChangeCount--;

        // 交換完了したら清算
        if (1 > cardChangeCount)
        {
            checkHandRank();
        }
    }

    void checkHandRank()
    {
        // フラッシュチェック
        bool flush = true;
        // 1枚目のカードのマーク
        SuitType suit = hand[0].Suit;

        foreach (var item in hand)
        {
            // 1枚目と違ったら終了
            if (suit != item.Suit)
            {
                flush = false;
                break;
            }
        }

        // ストレートチェック
        bool straight = false;
        for (int i = 0; i < hand.Count; i++)
        {
            // 何枚数字が連続したか
            int straightcount = 0;
            // 現在のカード番号
            int cardno = hand[i].No;

            // 1枚目から連続しているか調べる
            for (int j = 0; j < hand.Count; j++)
            {
                // 同じカードはスキップ
                if (i == j) continue;

                // 見つけたい数字は現在の数字+1
                int targetno = cardno + 1;
                // 13の次は1
                if (13 < targetno) targetno = 1;

                // ターゲットの数字発見
                if (targetno == hand[j].No)
                {
                    // 連続回数をカウント
                    straightcount++;
                    // 今回のカード番号(次回+1される)
                    cardno = hand[j].No;
                    // jはまた0から始める
                    j = -1;
                }
            }

            if (3 < straightcount)
            {
                straight = true;
                break;
            }
        }

        // 同じ数字のチェック
        int pair = 0;
        bool threecard = false;
        bool fourcard = false;
        List<CardController> checkcards = new List<CardController>();

        for (int i = 0; i < hand.Count; i++)
        {
            if (checkcards.Contains(hand[i])) continue;

            // 同じ数字のカード枚数
            int samenocount = 0;
            int cardno = hand[i].No;

            for (int j = 0; j < hand.Count; j++)
            {
                if (i == j) continue;
                if (cardno == hand[j].No)
                {
                    samenocount++;
                    checkcards.Add(hand[j]);
                }
            }

            // ワンペア、ツーペア、スリーカード、フォーカード判定
            if (1 == samenocount)
            {
                pair++;
            }
            else if (2 == samenocount)
            {
                threecard = true;
            }
            else if (3 == samenocount)
            {
                fourcard = true;
            }
        }

        // フルハウス
        bool fullhouse = false;
        if (1 == pair && threecard)
        {
            fullhouse = true;
        }

        // ストレートフラッシュ
        bool straightflush = false;
        if (flush && straight)
        {
            straightflush = true;
        }

        // 役の判定
        int addcoin = 0;
        string infotext = "役無し... ";

        if (straightflush)
        {
            addcoin = straightFlushRate * betCoin;
            infotext = "ストレートフラッシュ!! ";
        }
        else if (fourcard)
        {
            addcoin = fourCardRate * betCoin;
            infotext = "フォーカード!! ";
        }
        else if (fullhouse)
        {
            addcoin = fullHouseRate * betCoin;
            infotext = "フルハウス!! ";
        }
        else if (flush)
        {
            addcoin = flushRate * betCoin;
            infotext = "フラッシュ!! ";
        }
        else if (straight)
        {
            addcoin = straightRate * betCoin;
            infotext = "ストレート!! ";
        }
        else if (threecard)
        {
            addcoin = threeCardRate * betCoin;
            infotext = "スリーカード!! ";
        }
        else if (2 == pair)
        {
            addcoin = twoPairRate * betCoin;
            infotext = "ツーペア!! ";
        }
        else if (1 == pair)
        {
            addcoin = onePairRate * betCoin;
            infotext = "ワンペア!! ";
        }

        // コイン取得
        playerCoin += addcoin;

        // テキスト更新
        updateTexts();
        textGameInfo.text = infotext + addcoin;

        // 次回のゲーム用
        betCoin = 0;
        setButtonInPlay(false);
    }
}
