using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackJackDirector : MonoBehaviour
{
    // 共通カード管理クラス
    [SerializeField] CardsDirector cardsDirector;

    // UI
    [SerializeField] GameObject buttonHit;
    [SerializeField] GameObject buttonStay;
    [SerializeField] Text textPlayerInfo;
    [SerializeField] Text textDealerInfo;

    // 山札
    List<CardController> cards;

    // 手札
    List<CardController> playerHand;
    List<CardController> dealerHand;

    // 山札のインデックス
    int cardIndex;
    const float NextWaitTime = 1;

    // Start is called before the first frame update
    void Start()
    {
        // 山札をシャッフル
        cards = cardsDirector.GetShuffleCards();
        foreach (var item in cards)
        {
            item.transform.position = new Vector3(100, 0, 0);
            item.FlipCard(false);
        }

        // 手札を用意
        playerHand = new List<CardController>();
        dealerHand = new List<CardController>();

        cardIndex = 0;
        CardController card;
        
        // ディーラーが2枚引く
        card = hitCard(dealerHand);
        card = hitCard(dealerHand);

        // 1枚を表にする
        card.FlipCard();

        // プレイヤーが2枚引き、表にする
        hitCard(playerHand).FlipCard();
        hitCard(playerHand).FlipCard();

        textPlayerInfo.text = "";
    }

    // Update is called once per frame
    void Update()
    {

    }

    // ヒット
    CardController hitCard(List<CardController> hand)
    {
        // プレイヤーの初期位置
        float x = -0.1f;
        float z = -0.05f;

        // ディーラーの初期位置
        if (dealerHand == hand)
        {
            z = 0.1f;
        }

        // 手札がある場合は、右に並べる
        if(0 < hand.Count)
        {
            x = hand[hand.Count - 1].transform.position.x;
            z = hand[hand.Count - 1].transform.position.z;
        }
        
        // 山札からカードを取得
        CardController card = cards[cardIndex];
        card.transform.position = new Vector3(x + CardController.Width, 0, z);
        hand.Add(card);
        cardIndex++;

        return card;
    }
}
