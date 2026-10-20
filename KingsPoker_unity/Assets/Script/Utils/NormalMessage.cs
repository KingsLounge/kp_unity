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
        RestoreButtonLabels();
        pop = true;
        TitleText.text = title;
        messageText.text = message;
        okClickAction = okAction;
        this.cancelAction = cancelAction;
    }

    /// 버튼 글자를 바꿔 띄운다(예: 다시 시도 / 로그인 화면으로). 창이 닫히면 원래 글자로 돌아간다.
    /// 버튼은 씬에서 OnClickOk / OnClickCencel 에 연결된 것으로 찾는다
    public void OnMessagePopup(string title, string message, clickEvent okAction, clickEvent cancelAction, string okLabel, string cancelLabel)
    {
        OnMessagePopup(title, message, okAction, cancelAction);
        SetButtonLabel(nameof(OnClickOk), okLabel);
        SetButtonLabel(nameof(OnClickCencel), cancelLabel);
    }

    private readonly Dictionary<Text, string> originalLabels = new Dictionary<Text, string>();

    private void SetButtonLabel(string method, string label)
    {
        if (string.IsNullOrEmpty(label) || messagePopUp == null)
            return;
        foreach (var button in messagePopUp.GetComponentsInChildren<Button>(true))
        {
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentMethodName(i) != method)
                    continue;
                var text = button.GetComponentInChildren<Text>(true);
                if (text == null)
                    continue;
                if (!originalLabels.ContainsKey(text))
                    originalLabels[text] = text.text;
                text.text = label;
            }
        }
    }

    private void RestoreButtonLabels()
    {
        foreach (var e in originalLabels)
        {
            if (e.Key != null)
                e.Key.text = e.Value;
        }
        originalLabels.Clear();
    }

    public void OnClickOk()
    {
        // 글자를 먼저 되돌린다: 눌린 동작이 곧바로 다음 창(글자 바꾼 OnMessagePopup)을 띄울 수 있어서
        RestoreButtonLabels();
        if (okClickAction != null) 
        {
            okClickAction.Invoke();
        }
        
        messagePopUp.SetActive(false);
    }

    public void OnClickCencel()
    {
        RestoreButtonLabels();
        if (cancelAction != null) 
        {
            cancelAction.Invoke();
        }
        
        messagePopUp.SetActive(false);
    }
}
