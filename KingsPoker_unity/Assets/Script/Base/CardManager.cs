using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CardManager : WebsocketListenBehaviour
{
    public PlayerManager playerManager;

    [HideInInspector]
    public long roomNumber;

    [HideInInspector]
    public GAME_TYPE gameType;
    protected CardAnimationManager cardDrawer;

    [SerializeField]
    protected float firstCardDrawIntaval = 0.1f;

    [SerializeField]
    protected float firstCardDrawDuration = 0.3f;

    [SerializeField]
    protected float cardDrawIntaval = 0.1f;

    [SerializeField]
    protected float cardDrawDuration = 0.3f;

    [SerializeField]
    protected RectTransform cardDeck;

    [SerializeField]
    private GameObject cardObj;

    [SerializeField]
    private Transform cardParent;
    private static CardManager instance = null;
    public static CardManager Instance
    {
        get { return instance; }
    }

    protected ObjectPool2<GameObject> cardPool = new ObjectPool2<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        CardManager.instance = this;
        roomNumber = InfoManager.enterRn;
        cardPool.generator = CardGenerator;
        cardPool.deactivator = CardDeactivator;
        cardPool.activator = CardActivator;
        cardPool.remover = CardRemover;
        cardDrawer = GetComponentInChildren<CardAnimationManager>();
    }

    // Update is called once per frame
    void Update() { }

    public GameObject CardGenerator()
    {
        var obj = Instantiate(cardObj, cardParent);
        obj.SetActive(false);
        return obj;
    }

    public void CardDeactivator(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void CardActivator(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void CardRemover(GameObject obj)
    {
        Destroy(obj);
    }

    protected override void ReceivePacket(Packet packet)
    {
        base.ReceivePacket(packet);
        int p = packet.p;
        JObject c = packet.c;

        switch (p)
        {
            case 111: //PC_ROOM_COMMAND
                if (!RoomNumberCheck(c, roomNumber))
                {
                    break;
                }
                int command = (int)c["command"];
                if (
                    (ROOM_COMMAND)command == ROOM_COMMAND.end
                    || (ROOM_COMMAND)command == ROOM_COMMAND.start
                )
                {
                    GameEnd();
                }
                break;
        }
    }

    protected virtual void GameEnd() { }

    protected bool RoomNumberCheck(JObject data, long gtn)
    {
        long n = data.ValueOrDefault<long>("gtn", 0);
        return n == gtn;
    }

    public virtual void CardSetting(JObject c) { }

    public void DrawCard(Card playerCard, string cardData, float delay)
    {
        DrawCard(cardDeck, playerCard, cardData, delay);
    }

    public virtual void DrawCard(RectTransform from, Card playerCard, string cardData, float delay)
    {
        var obj = cardPool.GetObject();
        var targetRect = playerCard.transform.parent.GetComponent<RectTransform>();
        obj.transform.SetParent(targetRect.parent);
        obj.transform.SetAsLastSibling();
        var rect = obj.GetComponent<RectTransform>();

        var card = obj.GetComponent<Card>();
        card.SetCard(cardData, true);
        //var cardData = cardsData[i];


        cardDrawer.CardMove(
            rect,
            from,
            targetRect,
            delay,
            cardDrawDuration,
            () =>
            {
                playerCard.SetCard(cardData, true);
                playerCard.gameObject.SetActive(true);
                cardPool.ReturnObject(obj);
            }
        );
    }
}
