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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
