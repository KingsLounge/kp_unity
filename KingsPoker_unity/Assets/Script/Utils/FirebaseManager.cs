using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using Google;
using System.Threading.Tasks;
using System;
using AppleAuth;
using AppleAuth.Native;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using System.Text;
using System.Security.Cryptography;
using AppleAuth.Interfaces;
using Newtonsoft.Json.Linq;
using System.Linq;
using Firebase.Auth;

public enum LoginType
{
    NONE,
    EMAIL,
    GOOGLE,
    KAKAO,
    NAVER,
    APPLE

}
public class FirebaseManager : MonoBehaviour
{
    #region Singleton Initialize

    private static FirebaseManager sInstance;

    public static FirebaseManager Instance
    {
        get
        {
            sInstance = FindObjectOfType(typeof(FirebaseManager)) as FirebaseManager;
            if (sInstance == null)
            {
                GameObject newGameObject = new GameObject("FirebaseManager");
                sInstance = newGameObject.AddComponent<FirebaseManager>();
            }

            return sInstance;
        }
    }



    #endregion

    public string token;
    public string deviceToken;

    public string email;

    public LoginType loginType
    {
        get
        {
            return (LoginType)PlayerPrefs.GetInt("LoginType", (int)LoginType.NONE);
        }

        set
        {
            PlayerPrefs.SetInt("LoginType", (int)value);
            PlayerPrefs.Save();

        }
    }

#if UNITY_IOS || UNITY_ANDROID
    private GoogleSignInConfiguration configuration;
#endif
#if UNITY_IOS
    private IAppleAuthManager _appleAuthManager;
#endif

    private void Awake()
    {
        //configuration = new GoogleSignInConfiguration
        //{
        //    WebClientId = webClientId,
        //    RequestEmail = true,
        //    RequestProfile = true,
        //    RequestIdToken = true,
        //    UseGameSignIn = false
        //};
        //Debug.Log("구글 로그인 설정 완료");

        // Firebase.Auth.FirebaseAuth.DefaultInstance.IdTokenChanged += OnIdTokenChanged;
#if UNITY_IOS || UNITY_ANDROID
        Firebase.Auth.FirebaseAuth.DefaultInstance.StateChanged += OnStateChanged;
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
#endif
    }


    private void Update()
    {
#if UNITY_IOS
        if (_appleAuthManager != null)
            _appleAuthManager.Update();
#endif
    }

#if UNITY_IOS || UNITY_ANDROID
    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
        deviceToken = token.Token;

    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }
#endif
    private void OnStateChanged(object sender, System.EventArgs e)
    {
        Debug.Log("StateChanged");
    }

    private async void OnIdTokenChanged(object sender, System.EventArgs e)
    {
#if UNITY_IOS || UNITY_ANDROID
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;

        if (senderAuth == Firebase.Auth.FirebaseAuth.DefaultInstance && senderAuth.CurrentUser != null)
        {
            await senderAuth.CurrentUser.TokenAsync(true).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("IdTokenChanged TokenAsync is canceled");
                }
                if (task.IsFaulted)
                {
                    for (int i = 0; i < task.Exception.InnerExceptions.Count; i++)
                    {
                        Debug.Log(task.Exception.InnerExceptions[i].Message);
                    }
                }
                if (task.Result != null || task.Result != "")
                {
                    token = task.Result;
                }
            });
        }
        else
        {
            if (senderAuth != Firebase.Auth.FirebaseAuth.DefaultInstance)
            {
                Debug.Log("Wrong Auth");
            }
            else if (senderAuth.CurrentUser == null)
            {
                Debug.Log("Empty Current User");
            }
        }
#endif
    }

    public async Task<bool> CheckAutoLoginAsync()
    {
        bool isSuccess = false;
#if UNITY_IOS || UNITY_ANDROID
        if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.Log("CurrentUser == null");
            return isSuccess;
        }
        //----------------------------token을 PubLogin 호출 시 받음
        //if (token == null)
        //{
        //    Debug.Log("token == null  " + token);
        //    return isSuccess;
        //}
        //if (token == "")
        //{
        //    Debug.Log("token == '' " + token);
        //    return isSuccess;
        //}

        if (loginType == LoginType.GOOGLE)
        {
            isSuccess = await SignInWithGoogleSignInSilentlyAsync();
            Debug.Log("loginType == LoginType.GOOGLE, isSuccess: " + isSuccess);
        }
        else
        {
            isSuccess = true;
        }
#endif
        return isSuccess;
    }

    #region Email/Password
    public async void SignUpWithEmailAndPassword(string email, string password, string nickname, System.Action<bool> callback)
    {
        bool isSuccess = false;
        string error = null;
#if UNITY_IOS || UNITY_ANDROID
        await Firebase.Auth.FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith((Task<Firebase.Auth.AuthResult> task) =>
        {
            if (task.IsCanceled)
            {
                error = task.Exception.ToString();
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                Debug.LogError(error);
                callback(isSuccess);
                return;
            }
            if (task.IsFaulted)
            {
                error = task.Exception.ToString();
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                Debug.LogError(error);
                AggregateException ex = task.Exception;
                Debug.Log(ex.HResult);
                callback(isSuccess);
                return;
            }

            // Firebase user has been created.
            isSuccess = true;
            Firebase.Auth.FirebaseUser newUser = task.Result.User;
            LoginManager.instance.email = task.Result.User.Email;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
        });
        if (isSuccess)
        {
            await Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UpdateUserProfileAsync(new Firebase.Auth.UserProfile
            {
                DisplayName = nickname
            });
            await Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.ReloadAsync();
            await Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.SendEmailVerificationAsync();
            callback(isSuccess);
        }
        else
        {
            if (error.Contains("The email address is already in use by another account."))
            {
                ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_JOIN_FAILED", "SYS_ERR_JOIN_FAIL1");
            }
            else if (error.Contains("The email address is badly formatted."))
            {
                ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_JOIN_FAILED", "SYS_ERR_JOIN_FAIL2");
            }
            else if (error.Contains("The given password is invalid."))
            {
                ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_JOIN_FAILED", "SYS_ERR_JOIN_FAIL3");
            }
            else if (error.Contains("An email address must be provided."))
            {
                ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_JOIN_FAILED", "SYS_ERR_JOIN_FAIL2");
            }
            else if (error.Contains("A password must be provided."))
            {
                ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_JOIN_FAILED", "SYS_ERR_JOIN_FAIL3");
            }
            else
            {
                ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_JOIN_FAILED", error);
                // ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_JOIN_FAILED", "SYS_CLIENT_JOIN_FAILED");
            }
            callback(isSuccess);
        }
#endif
    }

    public async void SignInWithEmailAndPassword(string email, string password, System.Action<bool> callback)
    {
        bool isSuccess = false;
        string error = null;
#if UNITY_IOS || UNITY_ANDROID
        await Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log(string.Format("{0} / {1}", email, password));
                error = task.Exception.ToString();
                LoadingCircle.Instance.StopSpin();
                Debug.LogError(error);
                Debug.Log("로그인 실패");
            }
            else
            {
                Firebase.Auth.FirebaseUser newUser = task.Result.User;
                LoginManager.instance.email = task.Result.User.Email;



                isSuccess = true;
                Debug.Log("로그인 성공");
            }
        });

        if (isSuccess)
        {
            loginType = LoginType.EMAIL;
            await Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(true).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("IdTokenChanged TokenAsync is canceled");
                }
                if (task.IsFaulted)
                {
                    for (int i = 0; i < task.Exception.InnerExceptions.Count; i++)
                    {
                        Debug.Log(task.Exception.InnerExceptions[i].Message);
                    }
                }
                if (task.Result != null || task.Result != "")
                {
                    token = task.Result;
                }
            });

            callback(isSuccess);
            return;
        }
        else
        {
            Debug.Log("하하호호");
            if (error.Contains("There is no user record corresponding to this identifier. The user may have been deleted") || error.Contains("The email address is badly formatted"))
            {
                ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_LOGIN_FAILED", "SYS_ERR_LOGIN_FAILED_REAS_ID");
            }
            else if (error.Contains("The password is invalid or the user does not have a password") || error.Contains("A password must be provided."))
            {
                ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_LOGIN_FAILED", "SYS_ERR_LOGIN_FAILED_REAS_PW");
            }
            else if (error.Contains("An email address must be provided."))
            {
                ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_LOGIN_FAILED", "SYS_ERR_LOGIN_FAILED_REAS_ID");
            }
            else
            {
                ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_LOGIN_FAILED", error);
            }

            callback(isSuccess);
            return;
        }
#endif
    }


    public async void SignInWithCustomToken(string customToken, LoginType type, System.Action<bool> callback)
    {
        bool isSuccess = false;
        string error = null;
#if UNITY_IOS || UNITY_ANDROID
        await Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithCustomTokenAsync(customToken).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                error = task.Exception.ToString();
                Console.Log("로그인 실패");
            }
            else
            {
                Firebase.Auth.FirebaseUser newUser = task.Result.User;
                LoginManager.instance.email = task.Result.User.Email;
                isSuccess = true;
                Console.Log("로그인 성공");
            }
        });

        if (isSuccess)
        {
            loginType = type;
            await Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(true).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("IdTokenChanged TokenAsync is canceled");
                }
                if (task.IsFaulted)
                {
                    for (int i = 0; i < task.Exception.InnerExceptions.Count; i++)
                    {
                        Debug.Log(task.Exception.InnerExceptions[i].Message);
                    }
                }
                if (task.Result != null || task.Result != "")
                {
                    token = task.Result;
                }
            });

            callback(isSuccess);
            return;
        }
        else
        {
            Debug.Log("하하호호");
            ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_LOGIN_FAILED", error);
            callback(isSuccess);
            return;
        }
#endif
    }


    public async void PasswordReset(string email, System.Action<bool> callback)
    {
#if UNITY_IOS || UNITY_ANDROID
        await Firebase.Auth.FirebaseAuth.DefaultInstance.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                if (task.Exception != null)
                {
                    Console.Error(task.Exception);
                }
                callback(false);
            }
            else
            {
                callback(true);
            }
        });
        return;
#endif
    }

    #endregion

    #region Apple



    public void SignInWithApple(System.Action<bool, string> callback)
    {
        // 로더 켜기
#if UNITY_ANDROID
        var providerData = new Firebase.Auth.FederatedOAuthProviderData();
        providerData.ProviderId = "apple.com";
        var scopes = new List<string>() { "email", "name" };
        providerData.Scopes = scopes;

        var provider = new Firebase.Auth.FederatedOAuthProvider(providerData);

        bool isSuccess = false;

        FirebaseAuth.DefaultInstance.SignInWithProviderAsync(provider).ContinueWith(async task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback?.Invoke(false, task.Exception.ToString());
            }
            else
            {
                Firebase.Auth.FirebaseUser newUser = task.Result.User;
                LoginManager.instance.email = task.Result.User.Email;

                await Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(true).ContinueWith(task =>
                {
                    if (task.IsCanceled)
                    {
                        Debug.Log("IdTokenChanged TokenAsync is canceled");
                    }
                    if (task.IsFaulted)
                    {
                        for (int i = 0; i < task.Exception.InnerExceptions.Count; i++)
                        {
                            Debug.Log(task.Exception.InnerExceptions[i].Message);
                        }
                    }

                    if (task.Result != null || task.Result != "")
                    {
                        token = task.Result;
                        loginType = LoginType.APPLE;
                        callback?.Invoke(true, "Success");
                    }
                });

            }
        });

#elif UNITY_IOS
        if (!AppleAuthManager.IsCurrentPlatformSupported)
        {
            string msg = "this platform is not supported Apple Login";
            Debug.Log(msg);
            callback?.Invoke(false, msg);
            return;
        }
        if (_appleAuthManager == null)
        {
            var deserializer = new PayloadDeserializer();
            _appleAuthManager = new AppleAuthManager(deserializer);
        }
        var rawNonce = GenerateRandomString(32);
        var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce);
        _appleAuthManager.LoginWithAppleId(
            loginArgs,
            async credential => {
                try
                {
                    var appleIdCredential = credential as IAppleIDCredential;
                    var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                    var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                    Debug.Log("AppleID token - " + identityToken);
                    Debug.Log("AppleAccess token - " + authorizationCode);
                    Debug.Log("rawNonce - " + rawNonce);
                    var firebaseCredential = Firebase.Auth.OAuthProvider.GetCredential(
                        "apple.com",
                        identityToken,
                        rawNonce,
                        authorizationCode);

                    // 파이어베이스와 계정 연동
                    Debug.Log("Try Firebase SignInWithCredential");
                    await Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(firebaseCredential);
                    Debug.Log("Success Firebase SignInWithCredential");
                    // 처음 Apple로 로그인시 이름이 있으면 파이어베이스 유저 업데이트
                    if (appleIdCredential.FullName != null)
                    {
                        var userName = appleIdCredential.FullName.ToLocalizedString();

                        var profile = new Firebase.Auth.UserProfile();
                        profile.DisplayName = userName;
                        Debug.Log("Try Firebase UpdateUserProfile");
                        await Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UpdateUserProfileAsync(profile);
                        Debug.Log("Success Firebase UpdateUserProfile");
                    }
                    loginType = LoginType.APPLE;
                    Debug.Log("Success AppleID Login");
                    callback.Invoke(true, "Success");
                }
                catch (AggregateException ex)
                {
                    Debug.Log("Fail AppleID Login");
                    Debug.Log(ex.ToString());
                    callback.Invoke(false, ex.ToString());
                    // 로그인 실패 토스트 메세지
                }
                catch (Exception ex)
                {
                    Debug.Log("Fail AppleID Login");
                    Debug.Log(ex.ToString());
                    callback.Invoke(false, ex.ToString());
                    // 로그인 실패 토스트 메세지
                }
                finally
                {
                    Debug.Log("Complete Process AppleID Login");
                    // 로더 끄기
                }
            },
            error => {
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                switch (authorizationErrorCode)
                {
                    case AuthorizationErrorCode.Canceled:
                        LoadingCircle.Instance.StopSpin();
                        break;
                    case AuthorizationErrorCode.Unknown:
                    case AuthorizationErrorCode.InvalidResponse:
                    case AuthorizationErrorCode.NotHandled:
                    case AuthorizationErrorCode.Failed:
                        // 로그인 실패 토스트 메세지
                        callback.Invoke(false, authorizationErrorCode.ToString());
                        break;
                }
                // 로더 끄기
                LoadingCircle.Instance.StopSpin();
            });
#endif
    }

    private string GenerateRandomString(int length)
    {
        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
        var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
        var result = string.Empty;
        var remainingLength = length;

        var randomNumberHolder = new byte[1];
        while (remainingLength > 0)
        {
            var randomNumbers = new List<int>(16);
            for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
            {
                cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                randomNumbers.Add(randomNumberHolder[0]);
            }

            for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
            {
                if (remainingLength == 0)
                {
                    break;
                }

                var randomNumber = randomNumbers[randomNumberIndex];
                if (randomNumber < charset.Length)
                {
                    result += charset[randomNumber];
                    remainingLength--;
                }
            }
        }

        return result;
    }

    private string GenerateSHA256NonceFromRawNonce(string rawNonce)
    {
        var sha = new SHA256Managed();
        var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
        var hash = sha.ComputeHash(utf8RawNonce);

        var result = string.Empty;
        for (var i = 0; i < hash.Length; i++)
        {
            result += hash[i].ToString("x2");
        }

        return result;
    }

    #endregion

    #region Google
    public async void SignInWithGoogle(System.Action<bool, string> callback)
    {
#if UNITY_IOS || UNITY_ANDROID
        bool isSuccess = false;
        string errorMessage = null;

        string idToken = null;

        //GoogleSignIn.Configuration = configuration;

        if (GoogleSignIn.Configuration == null)
        {
            if (configuration == null)
            {
                configuration = new GoogleSignInConfiguration
                {
                    WebClientId = LinkOptionConstant.webClientId,
                    RequestEmail = true,
                    RequestProfile = true,
                    RequestIdToken = true,
                    UseGameSignIn = false
                };
                Debug.Log("구글 로그인 설정 생성");
            }
            else
            {
                configuration.WebClientId = LinkOptionConstant.webClientId;
                configuration.RequestEmail = true;
                configuration.RequestProfile = true;
                configuration.RequestIdToken = true;
                configuration.UseGameSignIn = false;
                Debug.Log("구글 로그인 재설정");
            }

            GoogleSignIn.Configuration = configuration;
            Debug.Log("구글 로그인 설정 등록");
        }


        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;


        await GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                using (IEnumerator<System.Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                        Debug.LogError("Got Error: " + error.Status + " " + error.Message);
                        errorMessage = string.Format("Google : Got Error: {0} {1}", error.Status, error.Message);
                        isSuccess = false;
                    }
                    else
                    {
                        Debug.LogError("Got Unexpected Exception?!?" + task.Exception);
                        errorMessage = string.Format("Google : Got Unexpected Exception?!? {0}", task.Exception);
                        isSuccess = false;
                    }
                }
            }
            else if (task.IsCanceled)
            {
                Debug.LogError("Google Canceled");
                errorMessage = "Google Canceled";
                isSuccess = false;
            }
            else
            {
                isSuccess = true;
                idToken = task.Result.IdToken;
                LoginManager.instance.email = task.Result.Email;
                Debug.Log(string.Format("DisplayName : {0}", task.Result.DisplayName));
                Debug.Log(string.Format("IdToken : {0}", task.Result.IdToken));
                Debug.Log(string.Format("ImageUrl : {0}", task.Result.ImageUrl));
                Debug.Log(string.Format("Email : {0}", task.Result.Email));
                Debug.Log(string.Format("task.Result : {0}", task.Result.ToString()));
            }
        });

        if (isSuccess == true)
        {
            Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(idToken, null);

            await Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    isSuccess = false;
                    errorMessage = task.Exception.ToString();
                    callback(isSuccess, errorMessage);
                }
                else
                {
                    isSuccess = true;
                    Firebase.Auth.FirebaseUser newUser = task.Result;
                    LoginManager.instance.email = task.Result.Email;
                    Console.Log("로그인 성공");
                }
            });
        }
        else
        {
            callback(isSuccess, errorMessage);
            return;
        }

        if (isSuccess)
        {
            loginType = LoginType.GOOGLE;
            await Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(true).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("IdTokenChanged TokenAsync is canceled");
                }

                if (task.IsFaulted)
                {
                    for (int i = 0; i < task.Exception.InnerExceptions.Count; i++)
                    {
                        Debug.Log(task.Exception.InnerExceptions[i].Message);
                    }
                    errorMessage = string.Format("Google : Got Error: {0}", task.Exception.Message);
                    callback(isSuccess, errorMessage);
                    return;
                }

                if (task.Result != null || task.Result != "")
                {
                    token = task.Result;
                    callback(isSuccess, errorMessage);
                }
            });

            return;
        }
        else
        {
            ErrorMessageManager.Instance.AddGameError(1, "SYS_ERR_LOGIN_FAILED", errorMessage);
            callback(isSuccess, errorMessage);
            return;
        }
#endif
    }

    public static async Task<string> TokenAsync()
    {
#if UNITY_IOS || UNITY_ANDROID
        return await Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(true);
#else
        return "";
#endif
    }
#if UNITY_IOS || UNITY_ANDROID
    public Firebase.Auth.FirebaseUser GetCurrentUser()
    {
        return Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
    }
#endif
    private async Task<bool> SignInWithGoogleSignInSilentlyAsync()
    {
        bool isSuccess = false;
        string errorMessage = null;
#if UNITY_IOS || UNITY_ANDROID
        //GoogleSignIn.Configuration = configuration;
        //GoogleSignIn.Configuration.UseGameSignIn = false;
        //GoogleSignIn.Configuration.RequestIdToken = true;
        if (GoogleSignIn.Configuration == null)
        {
            if (configuration == null)
            {
                configuration = new GoogleSignInConfiguration
                {
                    WebClientId = LinkOptionConstant.webClientId,
                    RequestEmail = true,
                    RequestProfile = true,
                    RequestIdToken = true,
                    UseGameSignIn = false
                };
                Debug.Log("구글 로그인 설정 생성");
            }
            else
            {
                configuration.WebClientId = LinkOptionConstant.webClientId;
                configuration.RequestEmail = true;
                configuration.RequestProfile = true;
                configuration.RequestIdToken = true;
                configuration.UseGameSignIn = false;
                Debug.Log("구글 로그인 재설정");
            }

            GoogleSignIn.Configuration = configuration;
            Debug.Log("구글 로그인 설정 등록");
        }


        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;



        Debug.Log("Start");


        await GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(task =>
        {
            Debug.Log("Do...");
            if (task.IsFaulted)
            {
                using (IEnumerator<System.Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                        errorMessage = string.Format("Got Error: {0} {1}", error.Status, error.Message);
                        Debug.LogError(errorMessage);
                    }
                    else
                    {
                        errorMessage = string.Format("Got Unexpected Exception?!? {0}", task.Exception);
                        Debug.LogError(errorMessage);
                    }
                }
            }
            else if (task.IsCanceled)
            {
                errorMessage = "Canceled";
                Debug.Log(errorMessage);
            }
            else
            {
                if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.Email != task.Result.Email)
                {
                    //error
                    errorMessage = "AutoLogin False";
                    Debug.Log(errorMessage);
                }
                else
                {
                    isSuccess = true;
                    LoginManager.instance.email = task.Result.Email;
                    Debug.Log(string.Format("IdToken : {0}", task.Result.IdToken));
                    Debug.Log(string.Format("ImageUrl : {0}", task.Result.ImageUrl));
                    Debug.Log(string.Format("Email : {0}", task.Result.Email));
                }
            }
        });


        Debug.Log("End");
#endif
        return isSuccess;
    }

    private void OnGoogleDisconnect()
    {
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    private void OnGoogleSignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
        Debug.Log("구글 로그아웃");
    }

    //public async void OnGoogleSignIn()
    //{
    //    GoogleSignIn.Configuration = configuration;
    //    GoogleSignIn.Configuration.UseGameSignIn = false;
    //    GoogleSignIn.Configuration.RequestIdToken = true;
    //    Debug.Log("Calling SignIn");

    //    await GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticationFinished);
    //}

    //public void OnGoogleSignInSilently()
    //{
    //    GoogleSignIn.Configuration = configuration;
    //    GoogleSignIn.Configuration.UseGameSignIn = false;
    //    GoogleSignIn.Configuration.RequestIdToken = true;
    //    Debug.Log("Calling SignIn Silently");

    //    GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnGoogleAuthenticationFinished);
    //}

    //internal void OnGoogleAuthenticationFinished(Task<GoogleSignInUser> task)
    //{
    //    if (task.IsFaulted)
    //    {
    //        using (IEnumerator<System.Exception> enumerator =
    //                task.Exception.InnerExceptions.GetEnumerator())
    //        {
    //            if (enumerator.MoveNext())
    //            {
    //                GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
    //                Debug.LogError("Got Error: " + error.Status + " " + error.Message);
    //            }
    //            else
    //            {
    //                Debug.LogError("Got Unexpected Exception?!?" + task.Exception);
    //            }
    //        }
    //    }
    //    else if (task.IsCanceled)
    //    {
    //        Debug.LogError("Canceled");
    //    }
    //    else
    //    {
    //        Debug.Log(string.Format("DisplayName : {0}", task.Result.DisplayName));
    //        Debug.Log(string.Format("IdToken : {0}", task.Result.IdToken));
    //        Debug.Log(string.Format("ImageUrl : {0}", task.Result.ImageUrl));
    //        Debug.Log(string.Format("Email : {0}", task.Result.Email));
    //    }
    //}


    #endregion

    #region Firebase

    public void OnFirebaseSignOut()
    {
        Debug.Log("1token = '';" + token);
        token = "";
        Debug.Log("2token = '';");
#if UNITY_IOS || UNITY_ANDROID
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
#endif
    }

    #endregion

    [ContextMenu("로그아웃")]
    public void SignOut()
    {
        DevManager.Instance.PubLogin = false;
        token = "";
        Debug.Log("token = ''..;");
        switch (loginType)
        {
            case LoginType.GOOGLE:
                OnGoogleSignOut();
                OnGoogleDisconnect();
                break;
            case LoginType.KAKAO:
                Kakao.Instance.Logout(() =>
                {
                    Debug.Log("카카오 로그아웃 성공");
                });

                Kakao.Instance.UnLink(() =>
                {
                    Debug.Log("카카오 탈퇴 성공");
                }, (err) =>
                {
                    Debug.LogError("카카오 탈퇴 실패");
                });
                break;
            case LoginType.EMAIL:
                break;
            case LoginType.NAVER:
                NaverLogin.Instance.Logout();
                break;
            case LoginType.APPLE:
                break;
            case LoginType.NONE:
                break;
        }

        PublisherApiManager.Instance.token = string.Empty;

        MyStatus.Init();

        OnFirebaseSignOut();
        loginType = LoginType.NONE;
    }
}
