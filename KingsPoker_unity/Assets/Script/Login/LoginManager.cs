using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using BestHTTP;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CreateAuthCondition
{
    public bool isEssential = true;
    public Toggle toggle;
}

public class LoginManager : MonoBehaviour
{
    private static LoginManager _instance = null;
    public static LoginManager instance
    {
        get { return _instance; }
        private set
        {
            if (_instance != null)
                GameObject.Destroy(_instance.gameObject);
            _instance = value;
        }
    }

    [Header("Login Buttons")]
    public GameObject[] loginButtons;

    [Header("AuthServicePanel")]
    public GameObject authServicePanel;

    [Header("EmailLoginPanel")]
    public GameObject emailLoginPanel;

    [Space]
    [Header("Tab")]
    public Toggle loginTab;
    public Toggle createAuthTab;

    [Space]
    [Header("Login")]
    public RectTransform loginBackground;
    public GameObject loginPanel;

    public InputField emailText;
    public Text emailErrorText;

    public InputField passwordText;
    public Text passwordErrorText;

    [Space]
    [Header("Condition")]
    //public GameObject step1;
    public List<CreateAuthCondition> conditions = new List<CreateAuthCondition>();
    public Toggle allAgreeConditionToggle;
    public Button confirmConditionButton;

    [Space]
    [Header("CreateAuth")]
    public GameObject step2;
    public InputField createAuthEmailText;
    public Text createAuthEmailErrorText;
    public bool validCreateAuthEmail;

    public InputField createAuthPasswordText;
    public Text createAuthPasswordErrorText;
    public bool validCreateAuthPassword;

    public InputField createAuthPasswordConfirmText;
    public Text createAuthPasswordConfirmErrorText;
    public bool validCreateAuthConfirmPassword;

    public InputField nicknameText;
    public Text nicknameErrorText;
    public bool validNickname;

    public Button createAccountButton;

    [Header("ForgotPassword")]
    public GameObject FindPasswodPanel;
    public InputField FindPasswordEmail;

    [Header("Dev Login")]
    public Dropdown devIdText;
    public InputField devPasswordText;

    [Header("Nickname Check")]
    public GameObject nicknameCheckPanel;
    public InputField nicknameCheckText;
    public Text nicknameCheckNotificationText;

    [Header("New")]
    public GameObject login;
    public GameObject dev;
    public GameObject webviewTop;

    [Header("Adult")]
    public GameObject adultWindow;
    public string kakaoUrl = "";
    public string niceUrl = "";
    public string danalUrl = "";
    public Button adultOkButton;

    public GameObject dropOutPopup;
    public Text dropOutText;

    [Header("Agree")]
    public GameObject agreePanel;
    public URLOpener termsOpener;
    public URLOpener privacyOpener;

    [Space]
    public static bool recvBlock = false;

    private bool isLoginFirebase = false;
    public delegate void loginError(string s);

    private bool isAutoLogin = false;
    public bool isAllAgreeEventFromToggle = true;

    private int gameServerTimeOut = 9;
    private int gameServerCurrentTime = 0;

    public static System.Action loginReset;

    private bool isWebViewAdultCertComplete = false;

    private string certurl = "";
    private string telegramUrl;
    private bool needTermsofuse = false;
    private bool needAdultcert = false;
    private string reason = "";

    public string email;
    public GameObject autologinWindow;
    public Image loginTypeImage;
    public List<LoginTypeImage> loginTypeImages;

    private void Awake()
    {
        loginReset += FailGameServerLogin;
        DontDestroyOnLoad(gameObject);
        instance = this;
        emailText.text = PlayerPrefs.GetString("email_login_id", "");
        passwordText.text = PlayerPrefs.GetString("email_login_password", "");
    }

    private void Start()
    {
        PublisherApiManager.Instance.PingCheckCancel();
    }

    #region UI Controlls
    /// <summary>
    /// 자동 로그인
    /// </summary>
    public async void OnLoginButton()
    {
        //for(int i = 0; i < loginButtons.Length; i++)
        //{
        //    loginButtons[i].SetActive(true);
        //}

        //WebSocketManager.defaultCli.OnMessage += WebSocketOnMessage;
#if UNITY_STANDALONE && !UNITY_EDITOR
        WebLogin();
        if (DevOptionsManager.devOptions.mode == MODE.dev)
        {
            dev.SetActive(true);
        }
#else
        if (DevOptionsManager.devOptions.mode == MODE.dev)
        {
            //FirebaseManager.Instance.SignOut();
            dev.SetActive(true);
            SetLoginMode(true);
            if (ServerConfigManager.Contains("telegram_login_url"))
            {
                telegramUrl = ServerConfigManager.GetConfig("telegram_login_url").ToString();
            }
        }
        else
        {
            isAutoLogin = await FirebaseManager.Instance.CheckAutoLoginAsync();

            Debug.Log("isAutoLogin " + isAutoLogin);

            Debug.Log("FirebaseManager.Instance.loginType " + FirebaseManager.Instance.loginType);

            if (isAutoLogin == true && FirebaseManager.Instance.loginType != LoginType.NONE)
            {
                RequestPubLogin();
                //AutoLoginPanel();

                // #if DEV
                //         RequestDevGameLoginData();
                // #else

                //LoadingCircle.Instance.StartSpin();
                //RequestPubLogin();
                // #endif

                Debug.Log("자동 로그인 성공..");
                return;
            }

            if (ServerConfigManager.Contains("telegram_login_url"))
            {
                telegramUrl = ServerConfigManager.GetConfig("telegram_login_url").ToString();
            }
            Debug.Log("자동 로그인 X ");
            SetLoginMode(true);
        }
#endif
    }

    public void AutoLoginPanel()
    {
        foreach (var typeImage in loginTypeImages)
        {
            if (typeImage.loginType == FirebaseManager.Instance.loginType)
            {
                loginTypeImage.sprite = typeImage.sprite;
                break;
            }
        }
        autologinWindow.SetActive(true);
    }

    public void OnClickTelegramButton()
    {
        Debug.Log("성인인증 ㄱ");
        LoadingCircle.Instance.StopSpin();
        isWebViewAdultCertComplete = false;
        //Screen.orientation = ScreenOrientation.Portrait;
        webView = new GameObject("webview").AddComponent<UniWebView>();
        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
        webView.OnOrientationChanged += (UniWebView webView, ScreenOrientation orientation) =>
        {
            webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
        };
        webView.OnMessageReceived += (view, message) =>
        {
            Debug.Log("딥링크 작동");
            if (message.Path == "login")
            {
                string need = message.Args["need"];
                view.InternalOnShouldClose();
            }

            if (message.Path == "webviewclose")
            {
                view.InternalOnShouldClose();
            }
        };
        webView.OnShouldClose += (view) =>
        {
            // if (Screen.autorotateToLandscapeRight)
            // {
            //     Screen.orientation = ScreenOrientation.LandscapeRight;
            // }
            // Screen.orientation = ScreenOrientation.AutoRotation;


            //if (isWebViewAdultCertComplete)
            //{
            //    PublisherApiManager.Instance.AdultCertCheck(
            //    () => {
            //        LoadingCircle.Instance.StartSpin();
            //        ConnectGameServer(RequestGameLoginData);
            //        DevManager.Instance.PubLogin = true;
            //    },
            //    () => {
            //        RequestPubLogin();
            //        Debug.Log("성인 인증 실패, 로그인 다시 시도");
            //    });
            //}
            //else
            //{
            //    RequestPubLogin();
            //    Debug.Log("성인 인증 실패, 로그인 다시 시도");
            //}
            return true;
        };
        webView.Load(telegramUrl);
        webView.Show();
        //webviewTop.SetActive(true);
    }

    private string webtoken;

    public async void WebLogin()
    {
        string[] args;
        string token;
        try
        {
            args = Environment.GetCommandLineArgs();
            token = args[1];
        }
        catch
        {
            ErrorMessageManager.Instance.AddNetworkError(
                422,
                "SYS_ERR_GET_TOKEN_FAILED",
                "SYS_CLIENT_GET_TOKEN_FAILED",
                ErrorHandlingType.SHUTDOWN
            );
            return;
        }

        Console.SpecialLog("Token : " + token);
        var idx = token.IndexOf("token=") + "token=".Length;
        token = token.Substring(idx);
        webtoken = token;
        LoadingCircle.Instance.StartSpin();
        //PublisherApiManager.Instance.LoginPubAPI(LoginPupCallBack, token);
        PublisherApiManager.Instance.token = token;
        ConnectGameServer(RequestGameLoginData);
    }

    public void OpenCreateAuthPanel()
    {
        ResetCreateAuthPanel();
        this.step2.SetActive(true);
        //this.step1.SetActive(false);
        this.loginPanel.SetActive(false);
    }

    public void ResetCreateAuthPanel()
    {
        // Form 초기화
        this.createAuthEmailText.text = "";
        this.createAuthPasswordText.text = "";
        this.createAuthPasswordConfirmText.text = "";
        this.nicknameText.text = "";

        createAuthEmailErrorText.text = "";
        createAuthPasswordErrorText.text = "";
        createAuthPasswordConfirmErrorText.text = "";
        nicknameErrorText.text = "";

        if (allAgreeConditionToggle)
            allAgreeConditionToggle.isOn = false;

        for (int i = 0; i < this.conditions.Count; i++)
        {
            this.conditions[i].toggle.isOn = false;
        }
    }

    public void CloseEmailLoginPanel()
    {
        ResetCreateAuthPanel();
        //this.step1.SetActive(false);
        this.step2.SetActive(false);
        this.loginPanel.SetActive(true);
        SetLoginMode(true);
        this.emailLoginPanel.SetActive(false);
    }

    public void ChangeLoginOrCreateAuthTab()
    {
        ResetCreateAuthPanel();
        this.FindPasswodPanel.SetActive(false);
        this.step2.SetActive(this.createAuthTab.isOn);
        this.loginPanel.SetActive(this.loginTab.isOn);
        // this.step1.SetActive(false);
        if (this.loginTab.isOn)
        {
            // TODO: 패널사이즈바꾸기 로그인은 높이 668
            loginBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 668);
        }

        if (this.createAuthTab.isOn)
        {
            // TODO: 패널사이즈바꾸기 약관동의는 높이 628
            loginBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 750);
        }
    }

    public void SetNicknameCheckNotification(string text, Color color)
    {
        nicknameCheckNotificationText.color = color;
        nicknameCheckNotificationText.text = text;
    }

    public void OnNicknameCheck()
    {
        int nickByte = System.Text.ASCIIEncoding.ASCII.GetByteCount(nicknameCheckText.text);
        if (nickByte < 3)
        {
            string message = LocalizeManager.GetLocalString("SYS_ERR_SHORT_NICKNAME");
            SetNicknameCheckNotification("message", Color.red);
            Console.Log("닉네임이 너무 짧습니다.");
            return;
        }
        else if (nickByte > 19)
        {
            string message = LocalizeManager.GetLocalString("SYS_ERR_LONG_NICKNAME");
            SetNicknameCheckNotification(message, Color.red);
            Console.Log("닉네임이 너무 깁니다.");
            return;
        }
        else
        {
            SetNicknameCheckNotification("", Color.red);
        }
    }

    public void OpenNicknameCheckPanel()
    {
        string message = LocalizeManager.GetLocalString("SYS_ERR_SHORT_NICKNAME");
        SetNicknameCheckNotification(message, Color.red);
        nicknameCheckText.text = "";
        nicknameCheckPanel.SetActive(true);
    }
    #endregion

    #region UI OnClickEvent

    public void OnClickGameStart()
    {
        if (isAutoLogin == true && FirebaseManager.Instance.loginType != LoginType.NONE)
        {
            LoadingCircle.Instance.StartSpin();

#if DEV
            RequestDevGameLoginData();
#else
            RequestPubLogin();
#endif

            Debug.Log("자동 로그인 성공.");
            return;
        }

        authServicePanel.SetActive(true);
    }

    public async void OnClickDevLoginButton()
    {
        LoadingCircle.Instance.StartSpin();
        ConnectGameServer(RequestDevLogin);

        //RequestDevLogin();
    }

    public void OnClickDev_ChangeId_Al()
    {
        devIdText.options.Clear();
        for (int i = 0; i < 9; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = "al000000" + (i + 1);
            devIdText.options.Add(option);
        }
        devIdText.value = 1;
        devIdText.value = 0;
    }

    public void OnClickDev_ChangeId_Jeckl()
    {
        devIdText.options.Clear();
        for (int i = 0; i < 14; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = "a" + (i + 1).ToString("D4");
            devIdText.options.Add(option);
        }
        devIdText.value = 1;
        devIdText.value = 0;
    }

    public void OnClickDev_ChangeId_hw()
    {
        devIdText.options.Clear();
        for (int i = 0; i < 9; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = "hw000000" + (i + 1);
            devIdText.options.Add(option);
        }
        devIdText.value = 1;
        devIdText.value = 0;
    }

    #region Email/Password

    public void OnClickEmailLoginButton()
    {
        loginBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 668);
        createAuthTab.isOn = false;
        SetLoginMode(false);
        loginTab.isOn = true;
        ChangeLoginOrCreateAuthTab();
        emailLoginPanel.SetActive(true);
    }

    public void SetLoginMode(bool active)
    {
        login.SetActive(active);
    }

    public void OnClickEmailPasswordLoginButton()
    {
        if (passwordText.text == "mode")
        {
            try
            {
                JObject temp = JObject.Parse(emailText.text);
                DevOptionsManager.devOptions = new DevOptions(temp);

                DevOptionsManager.SaveDevConfig(emailText.text);

                Debug.Log(emailText.text);

                NormalMessage.instance.AddSimpleMessage(emailText.text);

                CustomSceneManager.LoadLoginScene();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            return;
        }
        if (string.IsNullOrEmpty(emailText.text))
        {
            ErrorMessageManager.Instance.AddGameError(
                0,
                "SYS_ERR_LOGIN",
                "SYS_ERR_LOGIN_EMAIL_IS_EMPTY"
            );
            return;
        }
        if (string.IsNullOrEmpty(passwordText.text))
        {
            ErrorMessageManager.Instance.AddGameError(
                0,
                "SYS_ERR_LOGIN",
                "SYS_ERR_LOGIN_PASSWORD_IS_EMPTY"
            );
            return;
        }

        recvBlock = false;

        WaitCheck(() =>
        {
            FireBaseSignIn(emailText.text, passwordText.text);
        });
    }

    public async void WaitCheck(Action success)
    {
        LoadingCircle.Instance.StartSpin();

        bool isConnected = await PingCheckManager.CheckNetworkPing();

        if (!isConnected)
        {
            LoadingCircle.Instance.StopSpin();
            return;
        }

        isConnected = PingCheckManager.CheckNetworkConnection();

        if (!isConnected)
        {
            LoadingCircle.Instance.StopSpin();
            return;
        }

        success?.Invoke();
    }

    public void FireBaseSignIn(string id, string password)
    {
        FirebaseManager.Instance.SignInWithEmailAndPassword(
            id,
            password,
            (isSuccess) =>
            {
                if (isSuccess)
                {
#if DEV
                    RequestDevGameLoginData();
#else
                    PlayerPrefs.SetString("email_login_id", id);
                    PlayerPrefs.SetString("email_login_password", password);
                    PlayerPrefs.Save();
                    RequestPubLogin();
#endif
                }
                else
                {
                    //SystemMessageManager.Instance.InvokeSystemError(0);
                    //OnPopup("로그인 실패");
                    //LoadingCircle.Instance.StopSpin();
                    //Console.Log("로그인 실패");

                    LoadingCircle.Instance.StopSpin();
                }
            }
        );
    }

    /// <summary>
    /// 모두 동의 토글 이벤트 입니다.
    /// </summary>
    public void OnClickAllAgreeToggleButton()
    {
        if (isAllAgreeEventFromToggle == true)
        {
            if (allAgreeConditionToggle.isOn == true)
            {
                for (int i = 0; i < conditions.Count; i++)
                {
                    if (this.conditions[i].isEssential)
                    {
                        this.conditions[i].toggle.isOn = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < conditions.Count; i++)
                {
                    if (this.conditions[i].isEssential)
                    {
                        this.conditions[i].toggle.isOn = false;
                    }
                }
                isAllAgreeEventFromToggle = true;
            }
        }
        else
        {
            isAllAgreeEventFromToggle = true;
        }
    }

    /// <summary>
    /// 약관 동의 후 다음으로 진행할 수 있는지 확인합니다.
    /// </summary>
    public void OnActiveConfirmConditionButton()
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            if (conditions[i].isEssential == true && conditions[i].toggle.isOn == false)
            {
                confirmConditionButton.interactable = false;

                isAllAgreeEventFromToggle = false;
                allAgreeConditionToggle.isOn = false;

                return;
            }
        }

        isAllAgreeEventFromToggle = true;
        allAgreeConditionToggle.isOn = true;
        confirmConditionButton.interactable = true;
    }

    public void OnClickConditionAgreePanelConfirmButton()
    {
        bool ok = true;
        for (int i = 0; i < this.conditions.Count; i++)
        {
            if (this.conditions[i].isEssential && !this.conditions[i].toggle.isOn)
            {
                ok = false;
            }
        }
        if (ok)
        {
            step2.SetActive(true);
            //step1.SetActive(false);
            // TODO: 패널사이즈바꾸기 회원가입 높이는 982
            loginBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 750);
        }
        else
        {
            ErrorMessageManager.Instance.AddGameError(
                0,
                "SYS_ERR_JOIN_FAILED",
                "SYS_ERR_NEC_AGREE"
            );
            Console.Log("필수약관에 모두 동의해주세요.");
        }
    }

    /// <summary>
    /// 회원가입 이메일 형식을 체크합니다.
    /// </summary>
    public void OnEndEditCreateAuthEmail()
    {
        validCreateAuthEmail = Regex.IsMatch(
            createAuthEmailText.text,
            @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?"
        );
        if (validCreateAuthEmail)
        {
            createAuthEmailErrorText.text = "";
        }
        else
        {
            createAuthEmailErrorText.text = "올바르지 않은 이메일 형식입니다.";
        }

        OnActiveCreateAuthButton();
    }

    /// <summary>
    /// 회원가입 비밀번호를 체크합니다.
    /// </summary>
    public void OnEndEditCreateAuthPassword()
    {
        validCreateAuthPassword = Regex.IsMatch(
            createAuthPasswordText.text,
            @"[a-zA-Z0-9~`!@#$%^&*()_\-+={}[\]|\\;:'""<>,.?/]{8,16}$"
        );

        if (validCreateAuthPassword)
        {
            createAuthPasswordErrorText.text = "";
        }
        else
        {
            createAuthPasswordErrorText.text = "영문자, 숫자, 기호를 조합하여 8자 이상을 사용해야합니다.";
        }

        OnActiveCreateAuthButton();
    }

    /// <summary>
    /// 회원가입 비밀번호 확인을 체크합니다.
    /// </summary>
    public void OnEndEditCreateAuthConfirmPassword()
    {
        validCreateAuthConfirmPassword = createAuthPasswordText.text.Equals(
            createAuthPasswordConfirmText.text
        );

        if (validCreateAuthConfirmPassword)
        {
            createAuthPasswordConfirmErrorText.text = "";
        }
        else
        {
            createAuthPasswordConfirmErrorText.text = "비밀번호가 일치하지 않습니다.";
        }

        OnActiveCreateAuthButton();
    }

    /// <summary>
    /// 회원가입 닉네임을 체크합니다.
    /// </summary>
    public void OnEndEditCreateAuthNickanme()
    {
        validNickname = Regex.IsMatch(nicknameText.text, @"^[\w\Wㄱ-ㅎㅏ-ㅣ가-힣]{2,8}$");

        if (validNickname)
        {
            nicknameErrorText.text = "";
        }
        else
        {
            nicknameErrorText.text = "닉네임은 2~8자 사이여야 합니다.";
        }

        OnActiveCreateAuthButton();
    }

    /// <summary>
    /// 회원가입을 할 수 있는 상태인지 체크합니다.
    /// </summary>
    public void OnActiveCreateAuthButton()
    {
        if (validCreateAuthEmail && validCreateAuthPassword && validCreateAuthConfirmPassword) // && validNickname)
        {
            createAccountButton.interactable = true;
        }
        else
        {
            createAccountButton.interactable = false;
        }
    }

    public void OnClickCreateAuthButton()
    {
        if (createAuthPasswordText.text != createAuthPasswordConfirmText.text)
        {
            ErrorMessageManager.Instance.AddGameError(
                1,
                "SYS_ERR_JOIN_FAILED",
                "SYS_ERR_DEF_SECRET"
            );
            Console.Log("비밀번호와 비밀번호 확인란이 다릅니다.");
            return;
        }
        LoadingCircle.Instance.StartSpin();
        //int nickByte = System.Text.ASCIIEncoding.ASCII.GetByteCount(nicknameText.text);
        //if(nickByte < 3) {
        //    ErrorMessageManager.Instance.AddGameError(2, "SYS_ERR_JOIN_FAILED", "닉네임이 너무 짧습니다.");
        //    Console.Log("닉네임이 너무 짧습니다.");
        //    return;
        //} else if(nickByte > 19) {
        //    ErrorMessageManager.Instance.AddGameError(3, "SYS_ERR_JOIN_FAILED", "닉네임이 너무 깁니다.");
        //    Console.Log("닉네임이 너무 깁니다.");
        //    return;
        //}

        recvBlock = false;

        FirebaseManager.Instance.SignUpWithEmailAndPassword(
            createAuthEmailText.text,
            createAuthPasswordText.text,
            nicknameText.text,
            (isSuccess) =>
            {
                if (isSuccess)
                {
#if DEV
                    RequestDevGameLoginData();
#else
                    RequestPubLogin();
#endif
                }
                else
                {
                    LoadingCircle.Instance.StopSpin();
                    // ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_JOIN_FAILED", "SYS_CLIENT_JOIN_FAILED");
                }
            }
        );
    }

    public void OnClickFindPassword()
    {
        LoadingCircle.Instance.StartSpin();
        FirebaseManager.Instance.PasswordReset(
            FindPasswordEmail.text,
            (isSuccess, error) =>
            {
                LoadingCircle.Instance.StopSpin();
                if (isSuccess)
                {
                    ErrorMessageManager.Instance.AddGameError(
                        0,
                        "SYS_SECRET_INIT_EMAIL_SEND_SUCCESS",
                        "SYS_SECRET_INIT_EMAIL_SEND_SUCCESS_REQUEST_LOGIN",
                        ErrorHandlingType.RECALL,
                        null,
                        ChangeLoginOrCreateAuthTab
                    );
                }
                else if (!string.IsNullOrEmpty(error))
                {
                    // Firebase 가 준 이유를 그대로 (가입 안 된 이메일 / 형식 오류 / 요청 과다 …)
                    ErrorMessageManager.Instance.AddGameErrorBodyNotLocal(1, "SYS_SECRET_INIT_EMAIL_SEND_FAILED", error);
                }
                else
                {
                    ErrorMessageManager.Instance.AddGameError(
                        1,
                        "SYS_SECRET_INIT_EMAIL_SEND_FAILED",
                        "SYS_SECRET_INIT_EMAIL_SEND_FAILED_CHECK_EMAIL"
                    );
                }
            }
        );
    }

    #endregion

    #region Apple
    public void OnClickAppleLoginButton()
    {
        recvBlock = false;

        LoadingCircle.Instance.StartSpin();

        FirebaseManager.Instance.SignInWithApple(
            (isSuccess, error) =>
            {
                if (isSuccess)
                {
#if DEV
                    RequestDevGameLoginData();
#else
                    RequestPubLogin();
#endif
                }
                else
                {
                    LoadingCircle.Instance.StopSpin();
                    ErrorMessageManager.Instance.AddGameError(
                        9,
                        "SYS_ERR_APPLE_LOGIN_FAILED",
                        "SYS_ERR_APPLE_LOGIN_PLEASE_RETRY",
                        ErrorHandlingType.RECALL,
                        null,
                        () =>
                        {
                            CustomSceneManager.LoadLoginScene();
                        }
                    );
                }
            }
        );
    }
    #endregion

    #region Google

    public void OnClickGoogleLoginButton()
    {
        recvBlock = false;

        LoadingCircle.Instance.StartSpin();

        FirebaseManager.Instance.SignInWithGoogle(
            (isSuccess, error) =>
            {
                if (isSuccess)
                {
#if DEV
                    RequestDevGameLoginData();
#else
                    RequestPubLogin();
#endif
                }
                else
                {
                    LoadingCircle.Instance.StopSpin();
                    ErrorMessageManager.Instance.AddGameError(
                        9,
                        "SYS_ERR_GOOGLE_LOGIN_FAILED",
                        "SYS_ERR_GOOGLE_LOGIN_PLEASE_RETRY",
                        ErrorHandlingType.RECALL,
                        null,
                        () =>
                        {
                            CustomSceneManager.LoadLoginScene();
                        }
                    );
                }
            }
        );
    }

    #endregion


    #region Kakao

    public void OnClickKakaoLoginButton()
    {
        recvBlock = false;

        LoadingCircle.Instance.StartSpin();
        Kakao.Instance.Login(
            () =>
            {
                Debug.Log("로그인 성공");
                PublisherApiManager.Instance.KakaoCustomTokenAPI(
                    Kakao.Instance.GetToken(),
                    (string customToken) =>
                    {
                        Debug.Log("SignInWithCustomToken Start");
                        FirebaseManager.Instance.SignInWithCustomToken(
                            customToken,
                            LoginType.KAKAO,
                            (isSuccess) =>
                            {
                                Debug.Log("토큰 전달 성공");
                                if (isSuccess)
                                {
#if DEV
                                    RequestDevGameLoginData();
#else
                                    RequestPubLogin();
#endif
                                }
                                else
                                {
                                    //SystemMessageManager.Instance.InvokeSystemError(0);
                                    //OnPopup("로그인 실패");
                                    LoadingCircle.Instance.StopSpin();
                                    Console.Log("로그인 실패");
                                }
                            }
                        );
                    }
                );
            },
            (string reson) =>
            {
                LoadingCircle.Instance.StopSpin();
                Debug.Log("카카오 로그인 실패 : " + reson);
            }
        );
    }

    #endregion

    #region Naver

    // 네이버 로그인은 2026-10 에 제거됨 (SDK 미사용, dSYM 경고 유발). 씬의 버튼은 비활성이며 호환용 스텁만 남긴다.
    public void OnClickNaverLoginButton()
    {
        LoadingCircle.Instance.StopSpin();
        Console.Log("네이버 로그인은 더 이상 지원하지 않습니다.");
    }

    #endregion

    public void OnClickLogOut()
    {
        FirebaseManager.Instance.SignOut();
        //Kakao.Instance.UnLink(()=>{
        //    Debug.Log("카카오 탈퇴 성공");
        //},(err)=>{
        //    Debug.LogError("카카오 탈퇴 실패");
        //});
        //ErrorMessageManager.Instance.AddGameError(0, "로그아웃 성공", "로그아웃에 성공했습니다.");
    }

    public void OnClickNicknameCheckButton()
    {
        LoadingCircle.Instance.StartSpin();
        RequestPubNicknameCheck();
    }

    #endregion

    private void FailGameServerLogin()
    {
        WebSocketManager.defaultCli.OnMessage -= WebSocketOnMessage;
        DevManager.Instance.WsDelegate -= 1;
        WebSocketManager.defaultCli.OnExitOnce += (reson) =>
        {
            DevManager.Instance.WsConnect = false;
            DevManager.Instance.GsLogin = false;
            FirebaseManager.Instance.SignOut();
            DevManager.Instance.PubLogin = false;
        };
        WebSocketManager.defaultCli.Close();
    }

    /// <summary>
    /// Publisher API에게 로그인을 요청합니다.
    /// </summary>
    public async UniTask RequestPubLogin()
    {
        // 호출하는 곳 대부분이 기다리지 않아(fire-and-forget) 예외가 묻혀 화면이 멈췄다 → 잡아서 다시 시도 창
        try
        {
            await RequestPubLoginCore();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            ShowLoginRetry();
        }
    }

    private async UniTask RequestPubLoginCore()
    {

        
        var response = await PublisherApiManager.Instance.LoginPubAPI();

        if (recvBlock == true)
        {
            return;
        }

        if (response.code == 200) // 게임 로그인
        {
            Debug.Log("RequestPupLogin CallBack : " + response.json);
            JToken need = null;
            JObject check = null;
            JObject property = null;
            needTermsofuse = false;
            needAdultcert = false;
            InfoManager.canChangeNick = false;

            if (response.json.ContainsKey("property"))
            {
                property = response.json["property"].ToObject<JObject>();

                if (property.ContainsKey("check"))
                {
                    check = property["check"].ToObject<JObject>();
                    if (check.ContainsKey("can_chgnick"))
                    {
                        InfoManager.canChangeNick = check["can_chgnick"].ToObject<bool>();
                    }
                    if (check.ContainsKey("need_termsofuse"))
                    {
                        needTermsofuse = check["need_termsofuse"].ToObject<bool>();
                    }
                    if (check.ContainsKey("need_adultcert"))
                    {
                        needAdultcert = check["need_adultcert"].ToObject<bool>();
                    }
                }
            }

            if (needAdultcert)
            {
                if (response.json.ContainsKey("certurl"))
                {
                    certurl = response.json.GetValue("certurl").ToString();
                }

                JObject cert = response.json.ValueOrDefault<JObject>("cert", new JObject());
                reason = cert.ValueOrDefault<string>("reason", "");
                niceUrl = cert.ValueOrDefault("niceCerturl", "");
                //if (cert["certurl"] != null)
                //{
                //    kakaoUrl = cert["certurl"].ToObject<string>();
                //}
                //if (cert["niceCerturl"] != null)
                //{
                //    niceUrl = cert["niceCerturl"].ToObject<string>();
                //}
                //if (cert["danalCerturl"] != null)
                //{
                //    danalUrl = cert["danalCerturl"].ToObject<string>();
                //}

                //Debug.LogError(json.ToString());
#if UNITY_EDITOR || UNITY_STANDALONE
                //LoginCheck();
                Debug.Log("PC나 에디터에서는 성인인증 안함");
                //adultOkButton.interactable = false;
                //adultWindow.SetActive(true);
#elif UNITY_IOS || UNITY_ANDROID
                //adultWindow.SetActive(true);
                //adultOkButton.interactable = false;
                //RequestAdultCert(certurl);
#endif
            }
            else { }

            int state = response.json.ValueOrDefault<int>("state", 0);

            //TODO: 탈퇴계정 처리
            switch (state)
            {
                case 3:
                    {
                        LoadingCircle.Instance.StopSpin();
                        if (dropOutPopup)
                            dropOutPopup.SetActive(true);
                        string dropout_request_at = response.json.ValueOrDefault<string>(
                            "dropout_request_at",
                            null
                        );
                        string dropout_at = response.json.ValueOrDefault<string>(
                            "dropout_at",
                            null
                        );
                        string timeString = string.Empty;
                        if (dropout_at != null)
                        {
                            timeString =
                                $"\n취소 가능 기한 : {DateTimeParser.Parse(dropout_at).ToLocalTime().ToString("yyyy년 MM월 dd일 HH:mm")}";
                        }
                        string dropout_body =
                            $"이 계정은 탈퇴 처리중인 계정입니다.\n게임을 계속 하시려면 탈퇴 취소를 해 주세요.\n{timeString}";
                        if (dropOutText)
                            dropOutText.text = dropout_body;
                    }
                    break;
                default:
                    {
                        LoginCheck();
                    }
                    break;
            }
            //LoginCheck();
        }
        else if (
            response.code == 422
            && response.json != null
            && response.json.ValueOrDefault<string>("need", "") == "join"
        )
        {
            Debug.Log("회원가입");
            //LoadingCircle.Instance.StopSpin();
            nickTries = 0;
            registerTries = 0;
            RandomNicknameSet();
            //OpenNicknameCheckPanel();
        }
        else if (response.handled)
        {
            // 차단·손실 한도·탈퇴 등: 이유를 담은 안내창이 이미 떴고 닫으면 로그아웃된다 → 로그인 방법 버튼을 보인다
            LoadingCircle.Instance.StopSpin();
            SetLoginMode(true);
        }
        else // 에러: 네트워크·서버 오류·모르는 422(예전 422 는 join 이 아니면 아무것도 안 해 멈췄다)
        {
            ShowLoginRetry();
        }
    }

    #region 로그인 실패 처리 (다시 시도 / 로그인 화면으로)
    // 원본은 로그인 중 실패하면 오류만 띄우고 멈추는 곳이 많았다(자동 로그인이면 로그인 버튼도 안 보임) — 애플 심사 반려 원인.
    // 어느 단계에서 실패하든 이 창으로 모은다.
    //  다시 시도: 로그인 화면을 다시 연다 → 파이어베이스 로그인이 남아 있으면 자동 로그인부터 다시
    //  로그인 화면으로: 로그아웃 후 다시 연다 → 로그인 방법 버튼이 보인다
    private bool loginFailShown = false;
    private CancellationTokenSource loginWatchCts;
    private int nickTries = 0; // 랜덤 닉네임 중복 재시도
    private int registerTries = 0; // 회원가입 실패 재시도(예전에는 끝없이 반복)
    private const int MaxNickTries = 10;
    private const int MaxRegisterTries = 3;

    public void ShowLoginRetry(string bodyKey = "login_fail_retry_body")
    {
        if (loginFailShown || this == null)
            return;
        loginFailShown = true;
        StopLoginWatch();
        CloseLoginSocket();
        LoadingCircle.Instance.StopSpin();
        string title = LocalizeManager.GetLocalString("login_fail_title");
        string body = LocalizeManager.GetLocalString(bodyKey);
        if (NormalMessage.instance == null)
        {
            ErrorMessageManager.Instance.AddGameError(
                0,
                title,
                body,
                ErrorHandlingType.RECALL,
                null,
                ReloadLoginScene
            );
            return;
        }
        NormalMessage.instance.OnMessagePopup(
            title,
            body,
            () => CustomSceneManager.LoadLoginScene(),
            ReloadLoginScene,
            LocalizeManager.GetLocalString("login_retry"),
            LocalizeManager.GetLocalString("login_to_select")
        );
    }

    // 게임 서버 단계 감시: 정해진 시간 안에 다음 단계로 못 가면 다시 시도 창.
    // 원본은 소켓이 안 열리거나 로그인 패킷 하나가 빠지면 30초 로딩 뒤 '대기 시간 초과' 안내만 뜨고 멈췄다
    private void StartLoginWatch(int seconds, string bodyKey)
    {
        StopLoginWatch();
        loginWatchCts = new CancellationTokenSource();
        WatchLogin(seconds, bodyKey, loginWatchCts.Token).Forget();
    }

    private async UniTaskVoid WatchLogin(int seconds, string bodyKey, CancellationToken token)
    {
        bool canceled = await UniTask
            .Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: true, cancellationToken: token)
            .SuppressCancellationThrow();
        if (!canceled && this != null)
        {
            Debug.LogWarning($"로그인 단계 {seconds}초 초과 → 다시 시도 창");
            ShowLoginRetry(bodyKey);
        }
    }

    private void StopLoginWatch()
    {
        if (loginWatchCts == null)
            return;
        loginWatchCts.Cancel();
        loginWatchCts.Dispose();
        loginWatchCts = null;
    }

    // 로그인 도중의 소켓을 닫는다(다시 시도하면 새로 연결). 로그아웃은 하지 않는다
    private void CloseLoginSocket()
    {
        var cli = WebSocketManager.defaultCli;
        if (cli == null)
            return;
        cli.OnMessage -= WebSocketOnMessage;
        if (cli.isOpened)
            cli.Close();
    }

    private void OnDestroy()
    {
        StopLoginWatch();
    }

    // 본인인증 안내: 원본은 확인 버튼 하나뿐이고 본인인증 창에도 닫기가 없어, 인증을 못 하면 다른 로그인으로 갈 수 없었다.
    // 본인인증 진행 / 다른 계정으로 로그인(로그아웃 후 로그인 화면)
    private const string AdultCertExpireBody =
        "본인 인증 유효기간이 만료되었습니다.\n청소년보호법 제 17조와 여성가족부의 정책에 따라 연 1회 본인 인증을 진행하시면 청소년 유해매체물을 이용하실 수 있습니다.\n 인증 결과는 1년간 적용됩니다.\n\n확인 버튼을 누르시면 본인인증이 진행됩니다.";
    private const string AdultCertNewBody =
        "본인 인증을 하지 않은 계정입니다.\n청소년보호법 제 17조와 여성가족부의 정책에 따라 연 1회 본인 인증을 진행하시면 청소년 유해매체물을 이용하실 수 있습니다.\n 인증 결과는 1년간 적용됩니다.\n\n확인 버튼을 누르시면 본인인증이 진행됩니다.";

    private void ShowAdultCertNotice(string body)
    {
        LoadingCircle.Instance.StopSpin();
        Action open = () =>
        {
            adultOkButton.interactable = false;
            adultWindow.SetActive(true);
        };
        if (NormalMessage.instance == null)
        {
            ErrorMessageManager.Instance.AddGameError(
                0,
                "popup_adult_verification",
                body,
                ErrorHandlingType.RECALL,
                null,
                open
            );
            return;
        }
        NormalMessage.instance.OnMessagePopup(
            LocalizeManager.GetLocalString("popup_adult_verification"),
            body,
            () => open(),
            ReloadLoginScene,
            LocalizeManager.GetLocalString("adult_cert_go"),
            LocalizeManager.GetLocalString("login_other_account")
        );
    }

    /// 본인인증 창의 '다른 계정으로 로그인' 버튼용(씬의 VerificationWindow 에 버튼을 달아 이 함수를 연결)
    public void CancelAdultCert()
    {
        if (webView != null)
        {
            Destroy(webView.gameObject);
            webView = null;
        }
        if (adultWindow != null)
            adultWindow.SetActive(false);
        ReloadLoginScene();
    }

    private void Update()
    {
        // 안드로이드 뒤로가기: 본인인증 창이 떠 있고 웹뷰가 없으면 빠져나갈지 묻는다(원본은 아무 반응 없음)
        if (
            Input.GetKeyDown(KeyCode.Escape)
            && adultWindow != null
            && adultWindow.activeSelf
            && webView == null
        )
        {
            ShowAdultCertNotice(LocalizeManager.GetLocalString("adult_cert_incomplete_body"));
        }
    }
    #endregion

    public void ReloadLoginScene()
    {
        FirebaseManager.Instance.SignOut();
        CustomSceneManager.LoadLoginScene();
    }

    public void DropOutCancel()
    {
        PublisherApiManager.Instance.RequestDropoutCancel(DropUoptCancelCallback);
    }

    public void DropUoptCancelCallback(bool success, JObject json)
    {
        if (success)
        {
            LoginCheck();
        }
        else
        {
            ErrorMessageManager.Instance.AddNetworkError(
                0,
                "회원탈퇴 취소 실패",
                "회원탈퇴 취소에 실패하였습니다. 다시 시도 바랍니다.",
                ErrorHandlingType.RECALL,
                null,
                ReloadLoginScene
            );
        }
    }

    public void LoginCheck()
    {
        LoadingCircle.Instance.StartSpin();

        //인증 필요없을때 테스트용 코드입니다.
        //ConnectGameServer(PushTokenCheck);
        //DevManager.Instance.PubLogin = true;

        if (needTermsofuse)
        {
            LoadingCircle.Instance.StopSpin();
            SetLoginMode(true); // 자동 로그인 중이면 로그인 버튼이 없어 약관 창의 '뒤로'를 누르면 갇혔다
            agreePanel.SetActive(true);
            if (termsOpener && ServerConfigManager.Contains("terms"))
            {
                termsOpener.url = ServerConfigManager.GetConfig("terms").ToString();
            }
            if (privacyOpener && ServerConfigManager.Contains("privacy"))
            {
                privacyOpener.url = ServerConfigManager.GetConfig("privacy").ToString();
            }
        }
        else if (needAdultcert)
        {
            LoadingCircle.Instance.StopSpin();
#if UNITY_EDITOR || UNITY_STANDALONE
            needAdultcert = false;
            LoginCheck();

#elif UNITY_IOS || UNITY_ANDROID
            // reason 이 new·expire 가 아니면 예전에는 로그만 남기고 멈췄다 → new 처럼 안내
            if (reason != "new" && reason != "expire")
            {
                Debug.LogWarning($"needAdultCert 인데 reason 이 '{reason}' — new 로 안내");
            }
            ShowAdultCertNotice(
                reason == "expire"
                    ? AdultCertExpireBody
                    : LocalizeManager.GetLocalString("popup_adult_verification_message")
            );
#endif
        }
        else
        {
            LoadingCircle.Instance.StartSpin();

            // 예전: 소켓이 이미 열려 있으면 핑이 될 때 로딩만 멈추고 끝, 핑이 안 되면 '열려 있음'이라 아무것도 안 해 멈췄다
            // (게임 서버 로그인 실패 뒤 다시 시도하면 여기서 갇힘). 로그인 화면에서는 늘 새로 연결한다(Init 이 이전 소켓을 닫음)
            ConnectGameServer(PushTokenCheck);

            DevManager.Instance.PubLogin = true;
        }
    }

    public async void PushTokenCheck()
    {
        // 푸시 토큰은 로그인에 꼭 필요하지 않다: 아직 못 받았거나(TokenReceived 전) 요청이 실패하면 건너뛰고 로그인을 이어간다
        // (예전에는 토큰이 null 이면 등록 요청에서 예외가 나 그대로 멈췄다)
        if (string.IsNullOrEmpty(FirebaseManager.Instance.deviceToken))
        {
            RequestGameLoginData();
            return;
        }
        ResponseData response;
        try
        {
            response = await PublisherApiManager.Instance.CheckMyPushToken();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            RequestGameLoginData();
            return;
        }

        switch (response.code)
        {
            case 200:
                if (
                    response.json.ValueOrDefault<string>("token", "")
                    != FirebaseManager.Instance.deviceToken
                )
                {
                    RegisterPushToken();
                }
                else
                {
                    RequestGameLoginData();
                }
                break;
            default:
                RequestGameLoginData();
                break;
        }
    }

    public async UniTask RegisterPushToken()
    {
        try
        {
            await PublisherApiManager.Instance.SendCloudeMessesingToken(
                FirebaseManager.Instance.deviceToken
            );
        }
        catch (Exception e)
        {
            Debug.LogException(e); // 등록 실패는 로그인을 막지 않는다
        }

        RequestGameLoginData();
    }

    public void Agree()
    {
        LoadingCircle.Instance.StartSpin();
        PublisherApiManager.Instance.RequestUpdateCheck(
            "need_termsofuse",
            (statusCode) =>
            {
                LoadingCircle.Instance.StopSpin();
                if (statusCode != 200)
                {
                    ErrorMessageManager.Instance.AddNetworkError(
                        (int)statusCode,
                        "네트워크 에러",
                        "네트워크 문제로 동의 진행에 실패하셨습니다.",
                        ErrorHandlingType.RECALL,
                        null,
                        () =>
                        {
                            LoginCheck();
                        }
                    );
                }
                else
                {
                    needTermsofuse = false;
                }
                agreePanel.SetActive(false);
                LoginCheck();
            }
        );
    }

    public void NiceUrlGo()
    {
        if (!string.IsNullOrEmpty(niceUrl))
        {
            //var sefeWebView = UniWebViewSafeBrowsing.Create(niceUrl);
            //sefeWebView.Show();
            RequestAdultCert(niceUrl);
            //Application.OpenURL(niceUrl);
        }
    }

    public void DanalUrlGo()
    {
        if (!string.IsNullOrEmpty(danalUrl))
        {
            //var sefeWebView = UniWebViewSafeBrowsing.Create(niceUrl);
            //sefeWebView.Show();
            RequestAdultCert(danalUrl);
            //Application.OpenURL(niceUrl);
        }
    }

    public void KakaoUrlGo()
    {
        if (!string.IsNullOrEmpty(kakaoUrl))
        {
            //var sefeWebView = UniWebViewSafeBrowsing.Create(kakaoUrl);
            //sefeWebView.Show();
            RequestAdultCert(kakaoUrl);
            //Application.OpenURL(kakaoUrl);
        }
    }

    public void RequestAdultCert(string certurl)
    {
        Debug.Log("성인인증 ㄱ" + certurl);
        LoadingCircle.Instance.StopSpin();
        isWebViewAdultCertComplete = false;
        //Screen.orientation = ScreenOrientation.Portrait;
        webView = new GameObject("webview").AddComponent<UniWebView>();
        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
        webView.OnOrientationChanged += (UniWebView webView, ScreenOrientation orientation) =>
        {
            webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
        };
        webView.OnMessageReceived += (view, message) =>
        {
            Debug.Log("웹뷰 메세지 작동" + message.Path);
            Debug.Log("웹뷰 메세지 작동" + message.Args.ToString());

            if (message.Path == "kakaocert")
            {
                if (message.Args["complete"] == "true")
                {
                    isWebViewAdultCertComplete = true;
                    adultWindow.SetActive(false);
                    Debug.Log("카카오인증 웹뷰 성인인증 성공");
                }
                else
                {
                    isWebViewAdultCertComplete = false;
                    Debug.Log("카카오인증 웹뷰 성인인증 실패");
                }
            }
            else if (message.Path == "nicecert")
            {
                if (message.Args["complete"] == "true")
                {
                    isWebViewAdultCertComplete = true;
                    adultWindow.SetActive(false);
                    Debug.Log("나이스인증 웹뷰 성인인증 인증 성공");
                }
                else
                {
                    isWebViewAdultCertComplete = false;
                    Debug.Log("나이스인증 웹뷰 성인인증 인증 실패");
                }
            }
            else if (message.Path == "danalcert")
            {
                if (message.Args["complete"] == "true")
                {
                    isWebViewAdultCertComplete = true;
                    adultWindow.SetActive(false);
                    Debug.Log("다날인증 웹뷰 성인인증 인증 성공");
                }
                else
                {
                    isWebViewAdultCertComplete = false;
                    Debug.Log("다날인증 웹뷰 성인인증 인증 실패");
                }
            }
            else if (message.Path == "webviewclose") //
            {
                isWebViewAdultCertComplete = false;
                Debug.Log("웹뷰 닫기");
            }
            else
            {
                isWebViewAdultCertComplete = false;
                Debug.Log("정의되지 않은 웹뷰 메세지");
            }

            view.InternalOnShouldClose();

            //if(message.Path == "webviewclose")
            //{
            //    view.InternalOnShouldClose();
            //}
        };
        webView.OnShouldClose += (view) =>
        {
            // if (Screen.autorotateToLandscapeRight)
            // {
            //     Screen.orientation = ScreenOrientation.LandscapeRight;
            // }
            // Screen.orientation = ScreenOrientation.AutoRotation;


            if (isWebViewAdultCertComplete)
            {
                PublisherApiManager.Instance.AdultCertCheck(
                    () =>
                    {
                        LoadingCircle.Instance.StartSpin();
                        ConnectGameServer(RequestGameLoginData);
                        DevManager.Instance.PubLogin = true;
                    },
                    () =>
                    {
                        // 예전: 소켓도 없이 RequestGameLoginData 를 불러 조용히 멈췄다(인증 창은 이미 닫힘)
                        Debug.Log("성인 인증 확인 실패 → 다시 시도 창");
                        ShowLoginRetry();
                    }
                );
            }
            else
            {
                // 인증을 끝내지 않고 닫음: 서버에 다시 물어 아직 필요하면 안내창(본인인증 진행 / 다른 계정으로 로그인)이 뜬다.
                // 예전에는 같은 안내·창이 다시 떠 빠져나갈 수 없었다
                LoadingCircle.Instance.StartSpin();
                RequestPubLogin();
                Debug.Log("성인 인증 성공 or 실패, 로그인 다시 시도");
            }
            return true;
        };
        webView.Load(certurl);
        webView.Show();
        //webviewTop.SetActive(true);
    }

    public void GetRefQuest()
    {
        //currentState.text = "출석 데이터 호출 중....";
        PublisherApiManager.Instance.RequestGetRefQuest(GetRefQuestCallback);
    }

    public void GetRefQuestCallback(JObject data)
    {
        Console.Log(data);
        InfoManager.Instance.SetRefQuest(data);
#if ONE_STORE
        OnestoreIAPManager.Init();
#else
        IAPManager.Instance.Initialize();
#endif
    }

    private void RandomNicknameSet()
    {
        var ranNick = string.Format("User{0:0000000}", UnityEngine.Random.Range(1, 9999999));
        Debug.Log(string.Format("Random nick : {0}", ranNick));
        PublisherApiManager.Instance.CheckNickname(
            ranNick,
            (statusCode) =>
            {
                if (recvBlock == true)
                {
                    return;
                }

                Debug.Log(string.Format("RandomNick CallBack Code : {0}", statusCode));
                if (statusCode == 200)
                {
                    SetNicknameCheckNotification("", Color.red);
                    RequestPubRegister(ranNick);
                }
                else if (statusCode == 422 && ++nickTries < MaxNickTries)
                {
                    //LoadingCircle.Instance.StopSpin();
                    //SetNicknameCheckNotification("중복된 닉네임입니다.", Color.red);
                    RandomNicknameSet(); // 중복: 다른 번호로 다시(끝없이 돌지 않게 횟수 제한)
                }
                else
                {
                    // 예전: 오류 창(확인)만 띄우고 로딩이 돌던 채로 멈췄다
                    ShowLoginRetry();
                }
            }
        );
    }

    /// <summary>
    /// Publisher API에게 닉네임 중복체크를 요청합니다.
    /// </summary>
    private void RequestPubNicknameCheck()
    {
        PublisherApiManager.Instance.CheckNickname(
            nicknameCheckText.text,
            (statusCode) =>
            {
                if (recvBlock == true)
                {
                    return;
                }

                if (statusCode == 200)
                {
                    SetNicknameCheckNotification("", Color.red);
                    RequestPubRegister();
                }
                else if (statusCode == 422)
                {
                    LoadingCircle.Instance.StopSpin();
                    SetNicknameCheckNotification("중복된 닉네임입니다.", Color.red);
                }
                else
                {
                    LoadingCircle.Instance.StopSpin();
                    ErrorMessageManager.Instance.AddGameError(
                        (int)statusCode,
                        "SYS_ERR_NICK_CHECKING",
                        "SYS_ERR_NICK_CHECKING_NOT_DEFINED"
                    );
                }
            }
        );
    }

    /// <summary>
    /// Publisher API에게 회원가입을 요청합니다.
    /// </summary>
    private void RequestPubRegister()
    {
        PublisherApiManager.Instance.RegisterPubAPI(
            nicknameCheckText.text,
            async (statusCode, json) =>
            {
                if (recvBlock == true)
                {
                    return;
                }

                if (statusCode == 200)
                {
                    LoadingCircle.Instance.StartSpin();
                    ConnectGameServer(RequestGameLoginData);
                    //RequestGameLoginData();
                }
                else
                {
                    LoadingCircle.Instance.StopSpin();
                    ErrorMessageManager.Instance.AddGameError(
                        (int)statusCode,
                        "SYS_ERR_JOIN",
                        "SYS_ERR_JOIN_NOT_DEFINED"
                    );
                }
#if UNITY_STANDALONE
            },
            webtoken
        );
#else
            }
        );
#endif
    }

    private void RequestPubRegister(string nickName)
    {
        PublisherApiManager.Instance.RegisterPubAPI(
            nickName,
            async (statusCode, json) =>
            {
                if (recvBlock == true)
                {
                    return;
                }

                if (statusCode == 200)
                {
                    Debug.Log("RequestPubRegister: " + json);

                    JToken need = null;
                    JToken reason = null;
                    if (json.TryGetValue("need", out need))
                    {
                        if (need.ToString() == "adultCert")
                        {
                            certurl = json.ValueOrDefault<string>("certurl", ""); // 없으면 예전에는 예외로 멈췄다
                            if (json["certurl"] != null)
                            {
                                kakaoUrl = json["certurl"].ToObject<string>();
                            }
                            if (json["niceCerturl"] != null)
                            {
                                niceUrl = json["niceCerturl"].ToObject<string>();
                            }
                            if (json["danalCerturl"] != null)
                            {
                                danalUrl = json["danalCerturl"].ToObject<string>();
                            }

                            // 안내: 본인인증 진행 / 다른 계정으로 로그인(예전에는 확인뿐, reason 이 new·expire 가 아니면 아무것도 안 해 멈춤)
                            string why = json.TryGetValue("reason", out reason) ? reason.ToObject<string>() : "";
                            ShowAdultCertNotice(why == "expire" ? AdultCertExpireBody : AdultCertNewBody);

#if UNITY_EDITOR || UNITY_STANDALONE
                            //LoadingCircle.Instance.StartSpin();
                            //ConnectGameServer(RequestGameLoginData);
                            //DevManager.Instance.PubLogin = true;
                            //Debug.Log("PC나 에디터에서는 성인인증 안함");
                            //adultOkButton.interactable = false;
                            //adultWindow.SetActive(true);
#elif UNITY_IOS || UNITY_ANDROID
                            //adultWindow.SetActive(true);
                            //adultOkButton.interactable = false;
                            //RequestAdultCert(certurl);
#endif
                        }
                        else
                        {
                            ShowLoginRetry(); // 모르는 need: 예전에는 아무것도 안 해 멈췄다
                        }
                    }
                    else
                    {
                        ConnectGameServer(RequestGameLoginData);
                        DevManager.Instance.PubLogin = true;
                        Debug.Log("성인인증 필요없음");
                    }
                }
                else if (++registerTries < MaxRegisterTries)
                {
                    RandomNicknameSet(); // 다른 닉네임으로 다시(예전에는 오류 창을 쌓으며 끝없이 반복)
                }
                else
                {
                    ShowLoginRetry();
                }
#if UNITY_STANDALONE
            },
            webtoken
        );
#else
            }
        );
#endif
    }

    private bool initializeComplete = false;

    public void ConnectGameServer(System.Action callback)
    {
        //string url = (string)ServerConfigManager.GetConfig("gameServer_url");
        //int port = (int)ServerConfigManager.GetConfig("gameServer_port");
        WebSocketManager.Init((string)ServerConfigManager.GetConfig("gs_url"));
        InfoManager.Init();
        WebSocketManager.defaultCli.OnOpen -= DefaultWebSocketProcessing.OnOpen;
        WebSocketManager.defaultCli.OnMessage -= DefaultWebSocketProcessing.OnMessage;
        WebSocketManager.defaultCli.OnClose -= DefaultWebSocketProcessing.OnClose;
        WebSocketManager.defaultCli.OnError -= DefaultWebSocketProcessing.OnError;
        WebSocketManager.defaultCli.OnOpen += DefaultWebSocketProcessing.OnOpen;
        WebSocketManager.defaultCli.OnMessage += DefaultWebSocketProcessing.OnMessage;
        WebSocketManager.defaultCli.OnClose += DefaultWebSocketProcessing.OnClose;
        WebSocketManager.defaultCli.OnError += DefaultWebSocketProcessing.OnError;
        //WebSocketManager.defaultCli.OnReconnect += DefaultWebSocketProcessing.OnReconnecting;
        //WebSocketManager.defaultCli.OnReconnectFail+= DefaultWebSocketProcessing.OnReconnectingFail;
        //WebSocketManager.defaultCli.OnReconnectSuccess += DefaultWebSocketProcessing.OnReconnectingSuccess;
        WebSocketManager.defaultCli.OnMessage -= WebSocketOnMessage;
        WebSocketManager.defaultCli.OnMessage += WebSocketOnMessage;
        WebSocketManager.defaultCli.OnOpenOnce += () =>
        {
            Console.Log("웹소켓 오픈됨 - Instance ID : " + WebSocketManager.defaultCli.InstanceID);

            DevManager.Instance.WsConnect = true;
            DevManager.Instance.WsDelegate += 1;

            // 열린 뒤부터 로비까지(푸시 토큰 → 로그인 정보 → 로그인 패킷 → 유저 정보…) 25초 안에 못 가면 다시 시도 창
            StartLoginWatch(25, "login_fail_retry_body");
            callback?.Invoke();
        };
        // 소켓이 15초 안에 안 열리면 다시 시도 창(예전에는 30초 로딩 뒤 안내만 뜨고 멈춤)
        StartLoginWatch(15, "login_fail_server_body");

        //var cts = new CancellationTokenSource();
        //cts.CancelAfterSlim(TimeSpan.FromSeconds(30f));

        //await UniTask.WaitUntil(() => WebSocketManager.defaultCli.isOpened).AttachExternalCancellation(cts.Token);
    }

    /// <summary>
    /// Publisher API에게 받은 로그인데이터로 게임서버에 로그인을 요청합니다.
    /// </summary>
    private async void RequestGameLoginData()
    {
        try
        {
            var response = await PublisherApiManager.Instance.GetGameLoginDataPubAPI();

            if (recvBlock == true)
            {
                return;
            }

            if (response.code == 200)
            {
                string uid = response.json.GetValue("userid").ToString();
                string pw = response.json.GetValue("password").ToString();

                //LoadingCircle.Instance.StartSpin();
                RequestLogin(uid, pw);
            }
            else if (!response.handled)
            {
                // 예전: 오류 창(확인)만 띄우고 소켓은 열린 채로 멈췄다
                ShowLoginRetry();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            ShowLoginRetry();
        }
    }

    /// <summary>
    /// 재혁로컬api서버용
    /// </summary>
    private void RequestDevGameLoginData()
    {
        string url = (string)ServerConfigManager.GetConfig("gameServer_url");

        // 해당 게임의 서버주소와 로그인데이터(id/pw)를 받아오고 게임서버와 소켓연결을 하고 로그인패킷 보냄 "http://192.168.0.222:11300/api/firebase/verifyToken"
        HTTPRequest request = new HTTPRequest(
            new Uri(
                string.Format(
                    "{0}{1}",
                    PublisherApiManager.Instance.url,
                    "api/firebase/verifyToken"
                )
            ),
            HTTPMethods.Post,
            (req, res) =>
            {
                Debug.Log(res.DataAsText);
                JObject json = JObject.Parse(res.DataAsText);
                RequestLogin(json.GetValue("uid").ToString(), json.GetValue("password").ToString());
            }
        );
        request.SetHeader(
            "Authorization",
            string.Format("Bearer {0}", FirebaseManager.Instance.token)
        );
        request.Send();
    }

    /// <summary>
    /// GameServer에 다이렉트로 로그인 요청하는 함수
    /// </summary>
    private void RequestDevLogin()
    {
        isLoginFirebase = false;

        Packet p = new Packet((int)CPProtocol.CP_CONSOLE_LOGIN);
        p.Add("os", "android");
        p.Add("browser", "android");
        p.Add("client_type", "basic");
        p.Add("token", "dev");
        p.Add("acc", devIdText.options[devIdText.value].text);
        p.Add("pass", devPasswordText.text);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private void RequestLogin(string uid, string password)
    {
        //Debug.Log(string.Format("{{\"p\":{0}, \"c\":{{ \"os\":\"{1}\", \"browser\":\"{2}\", \"client_type\":\"{3}\", \"token\":\"{4}\" }}}}", 20, "android", "test", "unity", FirebaseManager.Instance.token));
        //WebSocketManager.defaultCli.Send(string.Format("{{\"p\":{0}, \"c\":{{ \"os\":\"{1}\", \"browser\":\"{2}\", \"client_type\":\"{3}\", \"token\":\"{4}\" }}}}", 20, "android", "test", "unity", FirebaseManager.Instance.token));
        isLoginFirebase = true;

        Packet p = new Packet((int)CPProtocol.CP_CONSOLE_LOGIN);
        p.Add("os", "android");
        p.Add("browser", "android");
        p.Add("client_type", "basic");
        p.Add("acc", uid); //Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId);
        p.Add("pass", password);

        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private void RequestUserInfo()
    {
        Packet p = new Packet((int)CPProtocol.CP_CONSOLE_USERINFO);

        //if(isLoginFirebase)
        //{
        //    p.Add("token", FirebaseManager.Instance.token);
        //}
        //else
        //{
        //    p.Add("token", "dev");
        //    p.Add("uid", MyStatus.gid);
        //}

        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private void RequestRoomOptionList()
    {
        Packet p = new Packet((int)CPProtocol.CP_ROOM_CREATE_OPTION_LIST);

        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private void RequestMyTournament()
    {
        Packet p = new Packet((int)CPProtocol.CP_TNMT_MY);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    private void RequestRewardTable()
    {
        Packet p = new Packet((int)CPProtocol.CP_TNMT_REWARD_TABLE);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void OnClickWebViewClose()
    {
        if (webView != null)
        {
            webView.InternalOnShouldClose();
        }
    }

    UniWebView webView = null;

    private void WebSocketOnMessage(string msg)
    {
        JObject json = JObject.Parse(msg);
        Packet p = new Packet((int)json["p"]);
        p.c = json["c"] as JObject;
        ReceivePacket(p);
    }

    protected void ReceivePacket(Packet packet)
    {
        PCProtocol p = (PCProtocol)packet.p;
        JObject c = packet.c;

        if (WebSocketManager.defaultCli.recvProtocolBlock == true)
        {
            return;
        }

        switch (p)
        {
            case PCProtocol.PC_CONSOLE_LOGIN_SUCCESS:
                MyStatus.loginSuccess = (bool)c["success"];
                MyStatus.token = (string)c["token"];
                MyStatus.gid = (string)c["uid"];
                MyStatus.nick = (string)c["nick"];
                MyStatus.status = (int)c["status"];
                MyStatus.push = c["push"] as JObject;

                DevManager.Instance.GsLogin = true;

                PingCheckManager.RepeatWebSocketPing();

#if UNITY_IOS || UNITY_ANDROID
                var newUser = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
                if (newUser != null && newUser.PhotoUrl != null)
                {
                    MyStatus.photoURL = newUser.PhotoUrl.ToString();
                }
#endif
                GetRefQuest();
#if UNITY_EDITOR || UNITY_STANDALONE
                RequestUserInfo();
#elif UNITY_IOS || UNITY_ANDROID
                RequestUserInfo();
                /*
                LoadingCircle.Instance.StopSpin();
                webView = new GameObject("webview").AddComponent<UniWebView>();
                webView.Frame = new Rect(0,100,Screen.width, Screen.height-100);
                webView.OnShouldClose += (view ) => {RequestUserInfo(); LoadingCircle.Instance.StartSpin(); return true;};
                webView.Load("http://gtest.fromthered.com/jackpot/test.html");
                webView.Show();
                webviewTop.SetActive(true);
                PublisherApiManager.Instance.SendCloudeMessesingToken(FirebaseManager.Instance.deviceToken, PushTokenSendCallBack);
                */
#endif

                break;

            case PCProtocol.PC_CONSOLE_USERINFO:
                MyStatus.type = (int)c["type"];
                MyStatus.zc = (long)c["zc"];
                MyStatus.dc = (long)c["dc"];
                try
                {
                    var rank = c["rank"].ToObject<JObject>();
                    var d = rank["daily"].ToObject<JObject>();
                    var w = rank["weekly"].ToObject<JObject>();
                    MyStatus.d_rank = d["rank"].ToObject<int>();
                    MyStatus.w_rank = w["rank"].ToObject<int>();
                    MyStatus.d_exp = d["exp"].ToObject<int>();
                    MyStatus.w_exp = w["exp"].ToObject<int>();
                }
                catch { }

                RequestRewardTable();
                break;

            case PCProtocol.PC_TNMT_MY: // PC_TNMT_MY

                JArray arr = c["tournament"] as JArray;
                for (int i = 0; i < arr.Count; i++)
                {
                    TnmtData data = MyStatus.AddTnmt((int)arr[i]["tn"]);
                    data.gtn = (int)arr[i]["gtn"];
                    //data.cafeIdx = (int)arr[i]["cafeIdx"];
                }
                JObject eliminate = c["eliminate"] as JObject;
                if (MyStatus.eliminates == null)
                {
                    MyStatus.eliminates = new List<Eliminate>();
                }

                MyStatus.eliminates.Clear();
                if (eliminate != null)
                {
                    for (int i = 0; i < eliminate.Count; i++)
                    {
                        var el = new Eliminate();
                        el.tn = (int)eliminate[i]["tn"];
                        el.rank = (int)eliminate[i]["rank"];
                        MyStatus.eliminates.Add(el);
                    }
                }
                RequestGetQuestList();
                break;
            case PCProtocol.PC_QUEST_LIST:
                QuestData.SetQuestData(c);
                RequestGetTournamentList();
                break;

            case PCProtocol.PC_TNMT_LIST:
                RequestRoomOptionList();
                break;
            case PCProtocol.PC_ROOM_CREATE_OPTION_LIST:
                Debug.Log(118);
                RoomOptions.rules.Clear();
                RoomOptions.rules.Add("nlh_list", c["holdem_list"]["item"] as JArray);
                // RoomOptions.rules.Add("badugi_list", c["badugi_list"]["item"] as JArray);
                // RoomOptions.rules.Add("poker7_list", c["poker7_list"]["item"] as JArray);
                LoadingCircle.Instance.StopSpin();
                LoadLobby();
                break;
            case PCProtocol.PC_CONSOLE_LOGIN_FAIL:
                //WebSocketManager.defaultCli.Close();
                StopLoginWatch();
                DevManager.Instance.GsLogin = false;
                FailGameServerLogin();

                ERR reason = (ERR)((int)c["reason"]);

                if (reason == ERR.UNDER_MAINTENANCE)
                {
                    ErrorMessageManager.Instance.AddNetworkError(
                        (int)reason,
                        "SYS_ERR_LOGIN",
                        "SYS_MAINTENANCE_TITLE",
                        ErrorHandlingType.SHUTDOWN
                    );
                }
                else
                {
                    // 예전: 확인만 있는 창 + 로그아웃 → 자동 로그인 중이면 로그인 버튼이 없어 멈췄다. 닫으면 로그인 화면을 다시 연다
                    ErrorMessageManager.Instance.AddGameError(
                        (int)reason,
                        "SYS_ERR_LOGIN",
                        reason.ToString(),
                        ErrorHandlingType.RECALL,
                        null,
                        ReloadLoginScene
                    );
                }

                break;
            case PCProtocol.PC_TNMT_REWARD_TABLE:
                HoldemRewardTable.SetTnmtRewardTable(c);
                RequestMyTournament();
                break;
        }
    }

    public void RequestGetTournamentList()
    {
        Packet p = new Packet((int)CPProtocol.CP_TNMT_LIST);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void RequestGetQuestList()
    {
        Packet p = new Packet((int)CPProtocol.CP_QUEST_LIST);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }

    public void PushTokenSendCallBack(long statusCode)
    {
        switch (statusCode)
        {
            case 200:
                Console.Log("푸시 토큰 전달 성공");
                break;
            default:
                Console.Error(string.Format("{0} : 푸시 토큰 전달 실패.", statusCode));
                break;
        }
    }

    public void LoadLobby()
    {
        StopLoginWatch();
        if (WebSocketManager.defaultCli != null)
        {
            WebSocketManager.defaultCli.OnMessage -= WebSocketOnMessage;
        }
        if (CustomSceneManager.GetCurrentSceneName() == "Lobby")
        {
            //ReconnectFlow();
        }
        else
        {
            CustomSceneManager.LoadScene("Lobby");
        }
    }

    public void ReconnectFlow()
    {
        LobbyManager.Instance.Initialize();
        RequestCafeList();
    }

    public void RequestCafeList()
    {
        Packet p = new Packet((int)CPProtocol.CP_CAFE_LIST);
        WebSocketManager.defaultCli.Send(p.ToJson());
    }
}

[Serializable]
public class LoginTypeImage
{
    public LoginType loginType;
    public Sprite sprite;
}
