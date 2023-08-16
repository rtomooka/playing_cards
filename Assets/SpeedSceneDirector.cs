using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SpeedSceneDirector : MonoBehaviour
{
    // カード管理
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] Text textResultInfo;

    // カード
    List<CardController> cards; // 全カード
    List<CardController> hand1p, hand2p; // プレイヤー手札
    List<CardController> layout1p, layout2p; // プレイヤー場札
    CardController[] leadCards; // 台札
    CardController selectCard; // 選択中のカード

    // ゲーム管理
    float cpuTimer;  //cpuの思考時間
    bool isGameEnd;

    const int LeadCardNo = 100;
    const float CpuRandomTimerMin = 3;
    const float CpuRandomTimerMax = 6;

    const float RestartMoveSpeed = 0.8f;
    const float RestartRotateSpeed = 0.8f;

    const float DealCardMoveSpeed = 0.5f;
    const float PlayCardMoveSpeed = 0.8f;

    const float HandPositionX = 0.15f;
    const float HandPositionZ = -0.1f;

    const float LayoutPositionX = -0.12f;
    const float LayoutPositionZ = -0.13f;

    const float LeadPositionX = -0.05f;
    const float StackCardHeight = 0.0001f;

    // Start is called before the first frame update
    void Start()
    {
        cards = cardsDirector.GetShuffleCards();

        hand1p = new List<CardController>();
        hand2p = new List<CardController>();
        layout1p = new List<CardController>();
        layout2p = new List<CardController>();
        leadCards = new CardController[2];
        textResultInfo.text = "";

        // スピードは1p と 2pで色を分ける
        // 1枚確認して1pへ渡し、おなじ色のカードは1pに渡す
        // 色が違う場合は、2pに渡す
        CardController firstCard = addCardHand(hand1p, cards[0]);
        foreach (var item in cards)
        {
            List<CardController> hand = hand1p;

            if (firstCard.SuitColor != item.SuitColor)
            {
                hand = hand2p;
            }
            // 追加済みのカードは処理されないので、追加するのみ
            addCardHand(hand, item);
        }

        // 場札を並べる
        ShuffleLayout(layout1p);
        ShuffleLayout(layout2p);

        // 台札を並べる
        playCardFromHand(hand1p);
        playCardFromHand(hand2p);

        // CPU行動タイマー
        cpuTimer = Random.Range(CpuRandomTimerMin, CpuRandomTimerMax);
    }

    // Update is called once per frame
    void Update()
    {
        // クリア済み
        if (isGameEnd) return; 
        // リセット判定
        if (tryResetLeadCards()) return;

        // プレイヤー操作
        if (Input.GetMouseButtonUp(0))
        {
            playerSelectCard();
        }

        // CPU操作
        cpuTimer -= Time.deltaTime;
        if (0 > cpuTimer)
        {
            autoSelectCard(layout2p);
            // 次回動作までの時間を再設定
            cpuTimer = Random.Range(CpuRandomTimerMin, CpuRandomTimerMax);
        }
    }

    // 手札にカードを追加する
    CardController addCardHand(List<CardController> hand, CardController card)
    {
        // 1P設定
        int playerNo = 0;
        int dir = 1;
        // 2P設定
        if (hand2p == hand)
        {
            playerNo = 1;
            dir *= -1;
        }

        // カードがない、追加済みの場合は処理しない
        if (!card || hand.Contains(card)) return null;

        // 1p と 2p で位置を対称にする
        card.transform.position = new Vector3(HandPositionX * dir, 0, HandPositionZ * dir);
        card.PlayerNo = playerNo;
        card.FlipCard(false);

        // 手札に追加
        hand.Add(card);
        return card;
    }

    // 手札から1枚カードを引く
    CardController getCardHand(List<CardController> hand)
    {
        CardController card = null;
        if (0 < hand.Count)
        {
            card = hand[0];
            hand.Remove(card);
        }
        return card;
    }

    // カードを移動して表向きにする
    void moveCardOpen(CardController card, Vector3 pos, float speed)
    {
        card.transform.DOKill();
        card.transform.DORotate(Vector3.zero, speed);
        card.transform.DOMove(pos, speed).OnComplete(() => card.FlipCard());
    }

    // 手札のカードを1枚引いて場札に移動する
    void dealCard(List<CardController> hand)
    {
        // 手札を1枚取得
        CardController card = getCardHand(hand);
        if (!card) return;

        // 1P設定
        List<CardController> layout = layout1p;
        int dir = 1;
        // 2P設定
        if (hand2p == hand)
        {
            layout = layout2p;
            dir *= -1;
        }

        // 手札の空いている場所へ1枚追加
        for (int i = 0; i < layout.Count; i++)
        {
            // カードがある場合はスキップ
            if (layout[i]) continue;

            // 内部データ更新
            layout[i] = card;

            // 目標位置
            float x = (i * CardController.Width + LayoutPositionX) * dir;
            float z = LayoutPositionZ * dir;
            Vector3 pos = new Vector3(x, 0, z);

            // 元の位置を保存する
            card.HandPosition = pos;

            // アニメーション
            float dist = Vector3.Distance(card.transform.position, pos);
            moveCardOpen(card, pos, dist / DealCardMoveSpeed);
            // 追加されたら終了
            break;
        }
    }

    // 場札を入れ替える
    void ShuffleLayout(List<CardController> layout)
    {
        // アニメーション中は処理しない
        foreach (var item in layout)
        {
            if (!item) continue;
            if (DOTween.IsTweening(item.transform)) return;
        }

        // 1P設定
        List<CardController> hand = hand1p;
        // 2P設定
        if (layout == layout2p)
        {
            hand = hand2p;
        }

        // 場札を手札に戻す
        foreach (var item in layout)
        {
            addCardHand(hand, item);
        }
        layout.Clear();

        cardsDirector.ShuffleCards(hand);

        // 4枚並べる
        for (int i = 0; i < 4; i++)
        {
            layout.Add(null); // 枠だけ追加する
            dealCard(hand);
        }
    }

    // 台札カードを更新する
    void updateLead(int index, CardController card)
    {
        // プレイヤー番号を台札に設定
        card.PlayerNo = LeadCardNo;
        card.Index = index;
        leadCards[index] = card;
    }

    // 手札から1枚台札に出す、なければ場札から出す
    void playCardFromHand(List<CardController> hand)
    {
        // 1P設定
        int index = 0;
        int dir = 1;
        List<CardController> layout = layout1p;
        // 2P設定
        if (hand2p == hand)
        {
            index = 1;
            dir *= -1;
            layout = layout2p;
        }

        // カードを手札から取得
        CardController card = getCardHand(hand);

        // 手札がなければ場札から出す
        if (!card)
        {
            foreach (var item in layout)
            {
                if (!item) continue;
                playCardFromLayout(item, index, true);
                return;
            }
        }

        // カードがある場合は少し上に置く
        float y = 0;
        if (leadCards[index])
        {
            y = leadCards[index].transform.position.y + StackCardHeight;
        }

        // 目的地
        Vector3 pos = new Vector3(LeadPositionX * dir, y, 0);
        // 距離
        float dist = Vector3.Distance(card.transform.position, pos);
        // アニメーション
        card.transform.DORotate(Vector3.zero, RestartRotateSpeed);
        card.transform.DOMove(pos, dist / RestartMoveSpeed).OnComplete(
            () =>
            {
                updateLead(index, card);
                card.FlipCard();
            }
        );
    }

    // カード選択
    void setSelectCard(CardController card = null)
    {
        // 非選択
        if (selectCard)
        {
            selectCard.gameObject.transform.position = selectCard.HandPosition;
            selectCard = null;
        }

        if (!card) return;

        // カード選択
        Vector3 pos = card.transform.position;
        pos.z += 0.02f;
        card.transform.position = pos;

        selectCard = card;
    }

    // プレイヤーの処理
    void playerSelectCard()
    {
        // カードを選択
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        CardController card = hit.collider.gameObject.GetComponent<CardController>();

        if (!card || !card.isFrontUp) return;

        // カード選択中に台札を選択
        if (selectCard && LeadCardNo == card.PlayerNo)
        {
            playCardFromLayout(selectCard, card.Index);
            setSelectCard();  // 非選択へ
        }
        // 手札
        else if (0 == card.PlayerNo)
        {
            setSelectCard(card);
        }
        else
        {
            // 何もしない
        }
    }

    // カードを置けるかどうか判定
    bool isMovable(CardController moveCard, CardController leadCard)
    {
        // カードがない場合は処理しない
        if (!moveCard || !leadCard) return false;
        // アニメーション中は処理しない
        if (DOTween.IsTweening(moveCard.transform)) return false;

        // 数字チェック
        int min = leadCard.No - 1;
        if (1 > min) min = 13;

        int max = leadCard.No + 1;
        if (13 < max) max = 1;

        // 1小さいor 1大きい場合は置ける
        if (min == moveCard.No || max == moveCard.No)
        {
            return true;
        }

        return false;
    }

    // 場札のカード枚数を返す
    int cardCount(List<CardController> layout)
    {
        int count = 0;
        foreach (var item in layout)
        {
            if (item) count++;
        }
        return count;
    }

    // 勝敗チェック
    void checkResult()
    {
        // 決着済み
        if (isGameEnd) return;

        if (1 > hand1p.Count && 1 > cardCount(layout1p))
        {
            textResultInfo.text = "1P勝利!!";
            isGameEnd = true;
        }
        else if (1 > hand2p.Count && 1 > cardCount(layout2p))
        {
            textResultInfo.text = "1P勝利!!";
            isGameEnd = true;
        }
        else
        {
            // 決着つかず
        }
    }

    // 場札から台札へ1枚出す
    void playCardFromLayout(CardController card, int index, bool force = false)
    {
        // 1P設定
        List<CardController> layout = layout1p;
        List<CardController> hand = hand1p;
        // 2P設定
        if (1 == card.PlayerNo)
        {
            layout = layout2p;
            hand = hand2p;
        }

        // 目的地
        Vector3 pos = leadCards[index].transform.position;
        // カードを上に乗せる
        pos.y += StackCardHeight;
        // 場札と台札の距離
        float dist = Vector3.Distance(card.transform.position, pos);
        // 角度をずらす
        float ry = Random.Range(-15.0f, 15.0f);
        card.transform.DORotate(new Vector3(0, ry, 0), dist / PlayCardMoveSpeed);

        // 移動完了時に台札の状態をチェックする
        card.transform.DOMove(pos, dist / PlayCardMoveSpeed).OnComplete(
            () =>
            {
                // 移動完了後、台札に置けるかどうか
                if (isMovable(card, leadCards[index]) || force)
                {
                    // 台札更新
                    updateLead(leadCards[index].Index, card);
                    // 場札更新
                    layout[layout.IndexOf(card)] = null;
                    // 手札から1枚引く
                    dealCard(hand);
                    // 勝敗チェック
                    checkResult();
                }
                // 置けない場合
                else
                {
                    moveCardOpen(card, card.HandPosition, dist / PlayCardMoveSpeed);
                }
            }
        );
    }

    // シャッフルボタン
    public void OnClickShuffle()
    {
        setSelectCard();
        ShuffleLayout(layout1p);
    }

    // CPUの動作
    void autoSelectCard(List<CardController> layout)
    {
        // 場札
        foreach (var layoutCard in layout)
        {
            // 台札
            foreach (var leadCard in leadCards)
            {
                // 置けない場合はスキップ
                if (!isMovable(layoutCard, leadCard)) continue;
                // 置けたら終了
                playCardFromLayout(layoutCard, leadCard.Index);
                return;
            }
        }

        // 置けない場合はシャッフル
        ShuffleLayout(layout);
    }

    // 台札のリセット
    bool tryResetLeadCards()
    {
        // アニメーション中は処理しない
        if (null != DOTween.PlayingTweens()) return false;

        // 場札のリストを作成
        List<CardController> allLayout = new List<CardController>(layout1p);
        allLayout.AddRange(layout2p);

        // 移動可能なカードを調査
        foreach (var layoutCard in allLayout)
        {
            foreach (var leadCard in leadCards)
            {
                // シャッフルを繰り返せば置ける
                if (isMovable(layoutCard, leadCard)) return false;
            }
        }

        // 台札を非表示
        foreach (var item in cards)
        {
            if (LeadCardNo == item.PlayerNo) 
            {
                item.gameObject.SetActive(false);
            }
        }

        // 台札をリセット
        setSelectCard();
        playCardFromHand(hand1p);
        playCardFromHand(hand2p);

        return true;
    }

    // シーンの初期化
    public void OnClickRestart()
    {   
        SceneManager.LoadScene("SpeedScene");
    }
}
