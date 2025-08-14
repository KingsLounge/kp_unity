using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HandLogFlowInfo : MonoBehaviour
{
    [SerializeField]
    private Text potText;

    [SerializeField]
    private GameObject otherBalloonPrefab;

    [SerializeField]
    private GameObject myBalloonPrefab;

    private ObjectPool2<HandLogBalloon> otherBalloonPool = new ObjectPool2<HandLogBalloon>();
    private ObjectPool2<HandLogBalloon> myBalloonPool = new ObjectPool2<HandLogBalloon>();
    private List<HandLogBalloon> usingOtherBalloons = new List<HandLogBalloon>();
    private List<HandLogBalloon> usingMyBalloons = new List<HandLogBalloon>();

    public Transform balloonContainerBG = null;
    public Transform balloonContainer = null;

    public float overHeight
    {
        get
        {
            return (balloonContainer as RectTransform).rect.height
                - (balloonContainerBG as RectTransform).rect.height;
        }
    }

    public float height
    {
        get
        {
            float height = 0;
            usingOtherBalloons.ForEach(u => height += (u.transform as RectTransform).rect.height);
            usingMyBalloons.ForEach(u => height += (u.transform as RectTransform).rect.height);
            return height;
        }
    }

    void Awake()
    {
        otherBalloonPool.activator = (balloon) =>
        {
            usingOtherBalloons.Add(balloon);
            balloon.transform.SetParent(balloonContainer, false);
            balloon.gameObject.SetActive(true);
        };
        myBalloonPool.activator = (balloon) =>
        {
            usingMyBalloons.Add(balloon);
            balloon.transform.SetParent(balloonContainer, false);
            balloon.gameObject.SetActive(true);
        };
        otherBalloonPool.deactivator = (balloon) =>
        {
            usingOtherBalloons.Remove(balloon);
            balloon.transform.SetParent(null, false);
            balloon.gameObject.SetActive(false);
        };
        myBalloonPool.deactivator = (balloon) =>
        {
            usingMyBalloons.Remove(balloon);
            balloon.transform.SetParent(null, false);
            balloon.gameObject.SetActive(false);
        };
        otherBalloonPool.generator = () =>
        {
            HandLogBalloon component = Instantiate(otherBalloonPrefab)
                .GetComponent<HandLogBalloon>();
            //usingOtherBalloons.Add(component);
            component.transform.SetParent(balloonContainer, false);
            return component;
        };
        myBalloonPool.generator = () =>
        {
            HandLogBalloon component = Instantiate(myBalloonPrefab).GetComponent<HandLogBalloon>();
            //usingMyBalloons.Add(component);
            component.transform.SetParent(balloonContainer, false);
            return component;
        };
    }

    private void OnDisable()
    {
        Clear();
    }

    private void Clear()
    {
        for (int i = usingOtherBalloons.Count - 1; i >= 0; i--)
        {
            otherBalloonPool.ReturnObject(usingOtherBalloons[i]);
        }
        for (int i = usingMyBalloons.Count - 1; i >= 0; i--)
        {
            myBalloonPool.ReturnObject(usingMyBalloons[i]);
        }
    }

    public void SetBetFlow(JArray data)
    {
        Clear();
        long pot = 0;
        for (int i = 0; i < data.Count; i++)
        {
            JObject bet = data[i] as JObject;
            // string nick = bet.ValueOrDefault("n", "");
            int seat = bet.ValueOrDefault("s", -1);
            int action = bet.ValueOrDefault("a", 0);
            long chip = bet.ValueOrDefault<long>("c", 0);
            var userData = HandLogDetailViewer.Instance.GetUserData(seat);
            string gid = "";
            string url = "";
            string nick = "";
            int icon = 0;
            if (userData != null)
            {
                gid = userData.p.ToString();
                url = userData.u.ToString();
                nick = userData.n.ToString();
                icon = userData.i.ToObject<int>();
            }
            bool isMe = gid == MyStatus.gid;
            string format = chip > 0 ? "{0}\n{1}" : "{0}";
            string contents = string.Format(
                format,
                LocalizeManager.GetLocalString(
                    "ingame_holdem_" + ((POKER_BETTYPE)action).ToString()
                ),
                MoneyToString.Converting(chip)
            );
            HandLogBalloon balloon = isMe
                ? myBalloonPool.GetObject()
                : otherBalloonPool.GetObject();
            pot += chip;
            balloon.Setting(nick, seat, contents, gid, url, icon);
        }
        potText.text = MoneyToString.Converting(pot);
        potText.gameObject.SetActive(pot > 0);
    }
}
