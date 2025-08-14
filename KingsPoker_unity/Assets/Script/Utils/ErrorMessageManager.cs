using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ErrorHandlingType
{
    NONE,
    SHUTDOWN,
    RECALL
}

public class SystemErrorMessage
{
    public int code;
    public string title;
    public string body;
    public ErrorHandlingType handlingType;

    public SystemErrorMessage(int code, string title, string body, ErrorHandlingType handlingType)
    {
        this.code = code;
        this.title = title;
        this.body = body;
        this.handlingType = handlingType;
    }
}

public class ErrorMessageManager : MonoBehaviour
{
    [Header("Application Error Pop Up")]
    public GameObject applicationErrorPopUp;
    public Text applicationErrorTitleText;
    public Text applicationErrorBodyText;
    public Button applicationErrorCloseButton;

    [Header("Network Error Pop Up")]
    public GameObject networkErrorPopUp;
    public Text networkErrorTitleText;
    public Text networkErrorBodyText;
    public Button networkErrorCloseButton;

    [Header("Game Error Pop Up")]
    public GameObject gameErrorPopUp;
    public Text gameErrorTitleText;
    public Text gameErrorBodyText;
    public Button gameErrorCloseButton;

    [Header("Reconnect Error Pop Up")]
    public GameObject recErrorPopUp;
    public Text recErrorTitleText;
    public Text recErrorBodyText;
    public Button recErrorCloseButton;

    private bool errorOpen = false;
    private Queue<ErrData> gameErrQueue = new Queue<ErrData>();
    private Queue<ErrData> networkErrQueue = new Queue<ErrData>();
    private Queue<ErrData> appErrQueue = new Queue<ErrData>();

    private List<string> uniqueKeys = new List<string>();

    public Dictionary<int, SystemErrorMessage> systemErrors =
        new Dictionary<int, SystemErrorMessage>();

    #region Singleton Initialize

    private static ErrorMessageManager instance;

    public static ErrorMessageManager Instance
    {
        get { return instance; }
    }

    #endregion

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (!errorOpen)
        {
            if (gameErrQueue.Count > 0)
            {
                InvokeGameError(gameErrQueue.Dequeue());
            }
            else if (networkErrQueue.Count > 0)
            {
                InvokeNetworkError(networkErrQueue.Dequeue());
            }
            else if (appErrQueue.Count > 0)
            {
                InvokeApplicationError(appErrQueue.Dequeue());
            }
        }
    }

    private void ErrorHandling(
        GameObject popUp,
        ErrorHandlingType handlingType,
        System.Action callback = null
    )
    {
        popUp.SetActive(false);

        switch (handlingType)
        {
            case ErrorHandlingType.SHUTDOWN:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
            case ErrorHandlingType.RECALL:
                if (callback != null)
                    callback();
                break;
            case ErrorHandlingType.NONE:
            default:
                break;
        }
    }

    [System.Obsolete()]
    public void AddSystemError(
        int code,
        string title,
        string body,
        ErrorHandlingType handlingType = ErrorHandlingType.NONE
    )
    {
        systemErrors.Add(code, new SystemErrorMessage(code, title, body, handlingType));
    }

    /// <summary>
    /// 애플리케이션 에러를 호출합니다.
    /// </summary>
    /// <param name="code">에러 코드</param>
    /// <param name="title">에러 제목</param>
    /// <param name="body">에러 내용</param>
    /// <param name="handlingType">에러 핸들링 방법</param>
    public void addApplicationError(
        int code,
        string title,
        string body,
        ErrorHandlingType handlingType = ErrorHandlingType.NONE,
        System.Action action = null,
        System.Action callback = null
    )
    {
        var errData = new ErrData();

        errData.code = code;
        errData.title = LocalizeManager.GetLocalString(title);
        errData.body = LocalizeManager.GetLocalString(body) + (code != 0 ? "[" + code + "]" : "");
        errData.action = action;
        errData.callback = callback;
        errData.handlingType = handlingType;
        appErrQueue.Enqueue(errData);
    }

    public void InvokeApplicationError(ErrData data)
    {
        LoadingCircle.Instance.StopSpin();
        errorOpen = true;
        if (data.action != null)
        {
            data.action.Invoke();
        }

        applicationErrorTitleText.text = data.title;
        applicationErrorBodyText.text = data.body;

        applicationErrorCloseButton.onClick.RemoveAllListeners();
        applicationErrorCloseButton.onClick.AddListener(() =>
        {
            ErrorHandling(applicationErrorPopUp, data.handlingType, data.callback);
            errorOpen = false;
        });

        applicationErrorPopUp.SetActive(true);
    }

    public void InvokeGameError(ErrData data)
    {
        LoadingCircle.Instance.StopSpin();
        errorOpen = true;
        if (data.action != null)
        {
            data.action.Invoke();
        }

        gameErrorTitleText.text = data.title;
        gameErrorBodyText.text = data.body;

        gameErrorCloseButton.onClick.RemoveAllListeners();
        gameErrorCloseButton.onClick.AddListener(() =>
        {
            ErrorHandling(gameErrorPopUp, data.handlingType, data.callback);
            errorOpen = false;
        });

        gameErrorPopUp.SetActive(true);
    }

    public void InvokeNetworkError(ErrData data)
    {
        LoadingCircle.Instance.StopSpin();
        errorOpen = true;
        if (data.action != null)
        {
            data.action.Invoke();
        }

        networkErrorTitleText.text = data.title;
        networkErrorBodyText.text = data.body;

        networkErrorCloseButton.onClick.RemoveAllListeners();
        networkErrorCloseButton.onClick.AddListener(() =>
        {
            if (uniqueKeys.Contains(data.uniqueKey))
            {
                uniqueKeys.Remove(data.uniqueKey);
            }

            errorOpen = false;

            ErrorHandling(networkErrorPopUp, data.handlingType, data.callback);
        });

        networkErrorPopUp.SetActive(true);
    }

    /// <summary>
    /// SYS_ERR_NETWORK를 호출합니다.
    /// </summary>
    /// <param name="code">에러 코드</param>
    /// <param name="title">에러 제목</param>
    /// <param name="body">에러 내용</param>
    /// <param name="handlingType">에러 핸들링 방법</param>
    public void AddNetworkError(
        int code,
        string title,
        string body,
        ErrorHandlingType handlingType = ErrorHandlingType.NONE,
        System.Action action = null,
        System.Action callback = null,
        string uniqueKey = null
    )
    {
        if (uniqueKey != null)
        {
            if (uniqueKeys.Contains(uniqueKey))
            {
                return;
            }
            else
            {
                uniqueKeys.Add(uniqueKey);
            }
        }

        var errData = new ErrData();

        errData.code = code;
        errData.title = LocalizeManager.GetLocalString(title);
        errData.body = LocalizeManager.GetLocalString(body) + (code != 0 ? "[" + code + "]" : "");
        errData.action = action;
        errData.callback = callback;
        errData.handlingType = handlingType;
        errData.uniqueKey = uniqueKey;

        networkErrQueue.Enqueue(errData);
    }

    public void AddNetworkErrorNotLocal(
        int code,
        string title,
        string body,
        ErrorHandlingType handlingType = ErrorHandlingType.NONE,
        System.Action action = null,
        System.Action callback = null
    )
    {
        var errData = new ErrData();

        errData.code = code;
        errData.title = LocalizeManager.GetLocalString(title);
        errData.body = body + (code != 0 ? "[" + code + "]" : "");
        errData.action = action;
        errData.callback = callback;
        errData.handlingType = handlingType;
        networkErrQueue.Enqueue(errData);
    }

    /// <summary>
    /// 게임 에러를 호출합니다.
    /// </summary>
    /// <param name="code">에러 코드</param>
    /// <param name="title">에러 제목</param>
    /// <param name="body">에러 내용</param>
    /// <param name="handlingType">에러 핸들링 방법</param>
    public void AddGameError(
        int code,
        string title,
        string body,
        ErrorHandlingType handlingType = ErrorHandlingType.NONE,
        System.Action callfront = null,
        System.Action callback = null
    )
    {
        var errData = new ErrData();

        errData.code = code;
        errData.title = LocalizeManager.GetLocalString(title);
        errData.body = LocalizeManager.GetLocalString(body) + (code != 0 ? "[" + code + "]" : "");
        errData.action = callfront;
        errData.callback = callback;
        errData.handlingType = handlingType;
        gameErrQueue.Enqueue(errData);
    }

    public void AddGameErrorBodyNotLocal(
        int code,
        string title,
        string body,
        ErrorHandlingType handlingType = ErrorHandlingType.NONE,
        System.Action action = null,
        System.Action callback = null
    )
    {
        var errData = new ErrData();

        errData.code = code;
        errData.title = LocalizeManager.GetLocalString(title);
        errData.body = body + (code != 0 ? "[" + code + "]" : "");
        errData.action = action;
        errData.callback = callback;
        errData.handlingType = handlingType;
        gameErrQueue.Enqueue(errData);
    }

    public void ReconnectErrorPopup(
        int code,
        string title,
        string body,
        ErrorHandlingType handlingType = ErrorHandlingType.NONE,
        System.Action action = null,
        System.Action callback = null
    )
    {
        var errData = new ErrData();

        errData.code = code;
        errData.title = LocalizeManager.GetLocalString(title);
        errData.body = LocalizeManager.GetLocalString(body) + (code != 0 ? "[" + code + "]" : "");
        errData.action = action;
        errData.callback = callback;
        errData.handlingType = handlingType;

        LoadingCircle.Instance.StopSpin();
        errorOpen = true;
        if (errData.action != null)
        {
            errData.action.Invoke();
        }

        recErrorTitleText.text = errData.title;
        recErrorBodyText.text = errData.body;

        recErrorCloseButton.onClick.RemoveAllListeners();
        recErrorCloseButton.onClick.AddListener(() =>
        {
            ErrorHandling(recErrorPopUp, errData.handlingType, errData.callback);
            errorOpen = false;
        });

        recErrorPopUp.SetActive(true);
    }

    public void ReconnectSuccess()
    {
        errorOpen = false;
        recErrorPopUp.SetActive(false);
    }

    [System.Obsolete()]
    public void RemoveSystemError(int code)
    {
        SystemErrorMessage systemErrorMessage;
        if (systemErrors.TryGetValue(code, out systemErrorMessage) == true)
        {
            systemErrors.Remove(code);
        }
    }

    [System.Obsolete()]
    public void ClearSystemError()
    {
        systemErrors.Clear();
    }

    public void ErrorInvoke(string errorMasage, int time) { }
}

public class ErrData
{
    public int code;
    public string title;
    public string body;
    public ErrorHandlingType handlingType = ErrorHandlingType.NONE;
    public System.Action action = null;
    public System.Action callback = null;
    public string uniqueKey = null;
}
