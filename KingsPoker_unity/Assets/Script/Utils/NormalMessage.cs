using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NormalMessage : MonoBehaviour
{
    public GameObject messagePopUp;
    public Text TitleText;
    public Text messageText;
    public delegate void clickEvent();
    public clickEvent okClickAction;
    public clickEvent cancelAction;
    public RectTransform simpleMessageContainer;
    public SimpleMessageItem simpleMessageItem;
    private ObjectPool2<SimpleMessageItem> simpleMessagePool = new ObjectPool2<SimpleMessageItem>();
    private Transform objectPoolContainer = null;
    public static NormalMessage instance { get; private set; }
    bool pop = false;

    public GameObject oneButtonPopUp;
    public Text oneButtonPopMessage;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        objectPoolContainer = (new GameObject("objectPoolContainer - NormalMessage.cs")).transform;
        objectPoolContainer.SetParent(transform);
        objectPoolContainer.gameObject.SetActive(false);
        simpleMessagePool.activator = (item) =>
        {
            item.transform.SetParent(simpleMessageContainer);
            item.gameObject.SetActive(true);
        };
        simpleMessagePool.deactivator = (item) =>
        {
            item.transform.SetParent(objectPoolContainer);
            item.gameObject.SetActive(false);
        };
        simpleMessagePool.generator = () =>
        {
            SimpleMessageItem item = Instantiate(simpleMessageItem);
            item.transform.SetParent(simpleMessageContainer);
            item.gameObject.SetActive(true);
            return item;
        };

    }
    // Update is called once per frame
    void Update()
    {
        if(pop)
        {
            messagePopUp.SetActive(true);
            pop = false;
        }
    }

    public void OnOneButtonMessagePopUp(string message,clickEvent okAction = null)
    {
        oneButtonPopMessage.text = LocalizeManager.GetLocalString(message);
        oneButtonPopUp.SetActive(true);
        okClickAction = okAction;
    }

    public void AddSimpleMessage(string message)
    {
        SimpleMessageItem item = simpleMessagePool.GetObject();
        item.Setting(message, () =>
        {
            simpleMessagePool.ReturnObject(item);
        });
    }

    public void OnMessagePopup(string title, string message, clickEvent okAction = null, clickEvent cancelAction = null)
    {
        pop = true;
        TitleText.text = title;
        messageText.text = message;
        okClickAction = okAction;
        this.cancelAction = cancelAction;
    }

    public void OnClickOk()
    {
        if (okClickAction != null) 
        {
            okClickAction.Invoke();
        }
        
        messagePopUp.SetActive(false);
    }

    public void OnClickCencel()
    {
        if (cancelAction != null) 
        {
            cancelAction.Invoke();
        }
        
        messagePopUp.SetActive(false);
    }
}
