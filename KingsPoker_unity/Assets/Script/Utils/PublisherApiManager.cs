using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BestHTTP;
using BestHTTP.Forms;
using Constant;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneStore.Purchasing;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;

public class ResponseData
{
    public long code = 0;
    public JObject json = null;
    public bool isOK = false;

    public ResponseData(long code, JObject json = null, bool isOK = true)
    {
        this.code = code;
        this.json = json;
        this.isOK = isOK;
    }
}

public class PublisherApiManager : MonoBehaviour
{
    #region Singleton Initialize

    private static PublisherApiManager sInstance;

    public static PublisherApiManager Instance
    {
        get
        {
            sInstance = FindObjectOfType(typeof(PublisherApiManager)) as PublisherApiManager;
            if (sInstance == null)
            {
                GameObject newGameObject = new GameObject("PublisherApiManager");
                sInstance = newGameObject.AddComponent<PublisherApiManager>();
            }

            return sInstance;
        }
    }

    #endregion

    // Swagger Docs : http://54.180.10.2:3000/docs/pub/

    //#if DEV
    //    public string url = "http://192.168.0.222:11300/";
    //#else
    //    public string url = "http://54.180.10.2:3000/";
    //#endif

    public string url = "";

    public string token = "";

    public void SetURL(string url, int port)
    {
        this.url = string.Format("http://{0}:{1}/", url, port);
        Debug.Log(this.url);
    }

    public void SetURL(string url)
    {
        this.url = url;
        Debug.Log(this.url);
    }

    //RequestLogin(json.GetValue("uid").ToString(), json.GetValue("password").ToString());
    //request.SetHeader("authorization", FirebaseManager.Instance.token);

    private IEnumerator sendWebRequest(UnityWebRequest www, string url)
    {
        yield return www.SendWebRequest();
        _Log(url, www);
    }

    private void _Log(string url, UnityWebRequest www)
    {
        if (DevOptionsManager.devOptions.mode != MODE.dev)
            return;

        if (www.downloadHandler == null)
            return;

#if UNITY_EDITOR
        string jsonStr = "";
        if (www.uploadHandler == null) { }
        else
        {
            var uploadedBodyJsonString = Encoding.UTF8.GetString(www.uploadHandler.data);
            if (uploadedBodyJsonString != null)
            {
                var dict = new Dictionary<string, string>();
                foreach (var item in uploadedBodyJsonString.Split('&'))
                {
                    var pair = item.Split('=');
                    if (pair.Length == 2)
                        dict[pair[0]] = pair[1];
                }
                jsonStr = JsonConvert.SerializeObject(dict, Formatting.None);
            }
        }
        InfoManager.PushApiAndProtocol("req: " + url + ",,,", jsonStr);
        InfoManager.PushApiAndProtocol(",res: " + url + ",,", www.downloadHandler.text);
#endif
        string debug = url + " \n -->  " + www.downloadHandler.text;
        Console.Log(debug);
        // LogExport.Write("\n\n" + debug);
    }

    public void UploadCafeLogo(int cafeIdx, string base64Str, Action<long, JObject> callback)
    {
        TryUploadCafeLogo(
            string.Format("{0}{1}", url, API.Post.UPLOAD_CAFE_LOGO),
            cafeIdx,
            base64Str,
            callback
        );
    }

    private async void TryUploadCafeLogo(
        string url,
        int cafeIdx,
        string base64Str,
        Action<long, JObject> callback
    )
    {
        WWWForm form = new WWWForm();

        form.AddField("file", base64Str);
        form.AddField("cafeIdx", cafeIdx);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);
            await sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Web Error : " + www.error);
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);
                callback?.Invoke(www.responseCode, json);
                Debug.Log("Web Success : " + www.downloadHandler.text);
            }
        }
    }

    /// <summary>
    /// POST : PubAPI에게 로그인을 요청합니다.
    /// </summary>
    /// <param name="idToken">파이어베이스 ID 토큰</param>
    /// <param name="callback">요청 결과 콜백 / int statusCode, JObject jsonData</param>
    public async UniTask<ResponseData> LoginPubAPI()
    {
        //HTTPRequest request = new HTTPRequest(new Uri(string.Format("{0}{1}", url, "api/pub/login")), HTTPMethods.Post, (req, res) =>
        //{
        //    Console.Log(res.DataAsText);
        //    JObject json = JObject.Parse(res.DataAsText);

        //    if(res.StatusCode == 200)
        //    {
        //        token = json.GetValue("pubtoken").ToString();
        //    }

        //    callback(res.StatusCode, json);
        //});
        //HTTPUrlEncodedForm form = new HTTPUrlEncodedForm();
        //form.AddField("idToken", FirebaseManager.Instance.token);
        //request.SetForm(form);
        //request.Send();

        //var to = token == null? FirebaseManager.Instance.token : token;
        Console.Log("파이어베이스 토근 발급 받는 중...");

        //var to = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(true).ContinueWith(task =>
        //{
        //    FirebaseManager.Instance.token = task.Result;
        //    return task.Result;
        //});

        var to = await FirebaseManager.TokenAsync();
        FirebaseManager.Instance.token = to;

        Console.Log(string.Format("파이어베이스 토근 발급: {0}", FirebaseManager.Instance.token));

        WWWForm form = new WWWForm();
        form.AddField("idToken", FirebaseManager.Instance.token);
        return (await TryLoginPubAPI(string.Format("{0}{1}", url, API.Post.LOGIN), form));
    }

    public void IdLoginPubAPI(string email, string password, Action<long, JObject> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("account", email);
        form.AddField("passwd", password);
        //TryLoginPubAPI(string.Format("{0}{1}", url, API.Post.LOGIN2), form, callback);
    }

    private async UniTask<ResponseData> TryLoginPubAPI(string url, WWWForm form)
    {
        url = RemoveWhiteSpace(url);

        //Debug.LogError(url);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            await sendWebRequest(www, url);

            if (
                www.result == UnityWebRequest.Result.ConnectionError
                || www.result == UnityWebRequest.Result.ProtocolError
            )
            {
                JObject json = null;
                try
                {
                    json = JObject.Parse(www.downloadHandler.text);
                }
                catch
                {
                    return new ResponseData(0, null, false);
                }

                Debug.LogError(www.downloadHandler.text);
                Debug.LogError(www.error);
                //JObject err = json["error"].ToObject<JObject>();


                if (www.responseCode == 422)
                {
                    string error = json["error"].ToObject<string>();
                    string need = "";
                    if (json["need"] != null)
                    {
                        need = json["need"].ToObject<string>();
                    }

                    //Debug.LogError("err : " + err);
                    //Debug.LogError("need : " + need);
                    if (error == "DAILY_LOSS_LIMIT_OVER")
                    {
                        string time = json["blockOffTime"].ToObject<string>();
                        var span = DateTimeParser.Parse(time) - (DateTime.UtcNow); // + new TimeSpan(9, 0, 0));
                        var spanString = string.Format(
                            "{1}시간 {2}분",
                            span.Days,
                            span.Hours,
                            span.Minutes
                        );
                        ErrorMessageManager.Instance.AddNetworkErrorNotLocal(
                            0,
                            "SYS_ERR_OVER_LIMIT_LOSS_DAILY",
                            string.Format(
                                LocalizeManager.GetLocalString(
                                    "SYS_ERR_COZ_CANT_CONNECT_OVER_LIMIT_LOSS_DAILEY"
                                ),
                                spanString
                            ),
                            ErrorHandlingType.RECALL,
                            null,
                            () =>
                            {
                                FirebaseManager.Instance.SignOut();
                            }
                        );
                    }
                    else if (error == "SELF_BLOCK_USER")
                    {
                        string time = json["selfBanOffTime"].ToObject<string>();
                        var span = DateTimeParser.Parse(time) - (DateTime.UtcNow);
                        var spanString = string.Format(
                            "{1}시간 {2}분",
                            span.Days,
                            span.Hours,
                            span.Minutes
                        );
                        ErrorMessageManager.Instance.AddNetworkErrorNotLocal(
                            0,
                            "SYS_SELT_GAME_BLOCK",
                            string.Format(
                                LocalizeManager.GetLocalString(
                                    "SYS_ERR_LOGIN_FAIL_SELF_GAME_BLOCK"
                                ),
                                spanString
                            ),
                            ErrorHandlingType.RECALL,
                            null,
                            () =>
                            {
                                FirebaseManager.Instance.SignOut();
                            }
                        );
                    }
                    else if (error == "UNKNOW_USER")
                    {
                        ErrorMessageManager.Instance.AddNetworkError(
                            422,
                            "SYS_ERR_LOGIN_FAILED",
                            "SYS_ERR_LOGIN_FAIL_UNKNOW_USER",
                            ErrorHandlingType.RECALL,
                            null,
                            () =>
                            {
                                FirebaseManager.Instance.SignOut();
                            }
                        );
                    }
                    else if (error == "BLOCK_USER")
                    {
                        ErrorMessageManager.Instance.AddNetworkError(
                            422,
                            "SYS_ERR_LOGIN_FAILED",
                            "SYS_ERR_LOGIN_FAIL_BLOCK_USER",
                            ErrorHandlingType.RECALL,
                            null,
                            () =>
                            {
                                FirebaseManager.Instance.SignOut();
                            }
                        );
                    }
                    else if (error == "WITHDRAW_USER")
                    {
                        ErrorMessageManager.Instance.AddNetworkError(
                            422,
                            "SYS_ERR_LOGIN_FAILED",
                            "SYS_ERR_LOGIN_FAIL_WITHDRAW_USER",
                            ErrorHandlingType.RECALL,
                            null,
                            () =>
                            {
                                FirebaseManager.Instance.SignOut();
                            }
                        );
                    }
                    else if (error == "NEED_EMAIL_VERIFY")
                    {
                        ErrorMessageManager.Instance.AddNetworkError(
                            422,
                            "SYS_MAIL_CONFIRM",
                            "SYS_ERR_NEED_EMAIL_VERIFY",
                            ErrorHandlingType.RECALL,
                            null,
                            () =>
                            {
                                FirebaseManager.Instance.SignOut();
                            }
                        );
                    }
                    else if (need == "join")
                    {
                        return new ResponseData(www.responseCode, json);
                    }
                    else
                    {
                        ErrorMessageManager.Instance.AddNetworkError(
                            0,
                            www.error,
                            www.downloadHandler.text,
                            ErrorHandlingType.RECALL,
                            null,
                            () =>
                            {
                                CustomSceneManager.LoadLoginScene();
                            }
                        );
                    }
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text,
                        ErrorHandlingType.RECALL,
                        null,
                        () =>
                        {
                            CustomSceneManager.LoadLoginScene();
                        }
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    token = json.GetValue("pubtoken").ToString();
                    //CancelInvoke("PingCheck");
                    //Invoke("PingCheck",600);
                    CancelInvoke("PingCheck");
                    InvokeRepeating("PingCheck", 10f, 10f);

                    return new ResponseData(www.responseCode, json);
                }
            }

            return new ResponseData(0, null, false);
        }
    }

    public void RequestDropout(Action<bool, JObject> callback)
    {
        StartCoroutine(_Request_Dropout(callback));
    }

    public IEnumerator _Request_Dropout(Action<bool, JObject> callback)
    {
        JObject json = null;
        string apiURL = string.Format("{0}{1}", url, API.Put.DROP_OUT);
        JObject dd = new JObject();
        var body = dd.ToString();
        apiURL = RemoveWhiteSpace(apiURL);
        UnityWebRequest www = UnityWebRequest.Put(apiURL, body);
        www.SetRequestHeader("Authorization", token);
        yield return www.SendWebRequest();
        _Log(apiURL, www);
        try
        {
            json = JObject.Parse(www.downloadHandler.text);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                ApiError(json, "회원 탈퇴 실패.");
                callback.Invoke(false, json);
                Debug.LogError(
                    $"DropuptFail width message : {json.ToString()}\nCode : {www.responseCode}"
                );
            }
            else
            {
                json = JObject.Parse(www.downloadHandler.text);
                callback.Invoke(true, json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"DropuptFail width message : {e.Message}");
        }
    }

    public async void RequestUserScore(Action<bool, JObject> callback, string gid, int cafeIdx = 0)
    {
        WWWForm form = new WWWForm();
        form.AddField("gid", gid);
        form.AddField("cafeIdx", cafeIdx);
        string apiURL = string.Format("{0}{1}", url, API.Post.REQUEST_USER_SCORE);

        apiURL = RemoveWhiteSpace(apiURL);
        UnityWebRequest www = UnityWebRequest.Post(apiURL, form);
        www.SetRequestHeader("Authorization", token);
        await sendWebRequest(www, apiURL);
        JObject json = null;
        try
        {
            json = JObject.Parse(www.downloadHandler.text);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                ApiError(json, "유저 정보 가져오기 실패");
                callback.Invoke(false, json);
                Debug.LogError(
                    $"DropOutCancel fail width message : {json.ToString()}\nCode : {www.responseCode}"
                );
            }
            else
            {
                json = JObject.Parse(www.downloadHandler.text);
                callback.Invoke(true, json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"DropOutCancel fail width message : {e.Message}");
        }
    }
    public async void RequestTagMyGet(Action<bool, JObject> callback)
    {
        WWWForm form = new WWWForm();
        string apiURL = string.Format("{0}{1}", url, API.Post.TAG_MY_GET);

        apiURL = RemoveWhiteSpace(apiURL);
        UnityWebRequest www = UnityWebRequest.Post(apiURL, form);
        www.SetRequestHeader("Authorization", token);
        await sendWebRequest(www, apiURL);
        JObject json = null;
        try
        {
            json = JObject.Parse(www.downloadHandler.text);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                ApiError(json, "태그 정보 가져오기 실패");
                callback.Invoke(false, json);
                Debug.LogError(
                    $"DropOutCancel fail width message : {json.ToString()}\nCode : {www.responseCode}"
                );
            }
            else
            {
                json = JObject.Parse(www.downloadHandler.text);
                callback.Invoke(true, json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"DropOutCancel fail width message : {e.Message}");
        }
    }

    public async void RequestTagMySet(Action<bool, JObject> callback, JObject data)
    {
        WWWForm form = new WWWForm();
        form.AddField("tagData", data.ToString());
        string apiURL = string.Format("{0}{1}", url, API.Post.TAG_MY_SET);

        apiURL = RemoveWhiteSpace(apiURL);
        UnityWebRequest www = UnityWebRequest.Post(apiURL, form);
        www.SetRequestHeader("Authorization", token);
        await sendWebRequest(www, apiURL);
        JObject json = null;
        try
        {
            json = JObject.Parse(www.downloadHandler.text);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                ApiError(json, "태그 정보 설정 실패");
                callback.Invoke(false, json);
                Debug.LogError(
                    $"DropOutCancel fail width message : {json.ToString()}\nCode : {www.responseCode}"
                );
            }
            else
            {
                json = JObject.Parse(www.downloadHandler.text);
                callback.Invoke(true, json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"DropOutCancel fail width message : {e.Message}");
        }
    }

    public async UniTask RequestTagGet(Action<bool, JObject> callback, string gid)
    {
        WWWForm form = new WWWForm();
        form.AddField("gid", gid);
        string apiURL = string.Format("{0}{1}", url, API.Post.TAG_GET);

        apiURL = RemoveWhiteSpace(apiURL);
        UnityWebRequest www = UnityWebRequest.Post(apiURL, form);
        www.SetRequestHeader("Authorization", token);
        await sendWebRequest(www, apiURL);
        JObject json = null;
        try
        {
            json = JObject.Parse(www.downloadHandler.text);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                ApiError(json, "태그 정보 설정 실패");
                callback.Invoke(false, json);
                Debug.LogError(
                    $"DropOutCancel fail width message : {json.ToString()}\nCode : {www.responseCode}"
                );
            }
            else
            {
                json = JObject.Parse(www.downloadHandler.text);
                callback.Invoke(true, json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"DropOutCancel fail width message : {e.Message}");
        }
    }

    public async void RequestTagSet(Action<bool, JObject> callback, string gid, int tag, string memo)
    {
        WWWForm form = new WWWForm();
        form.AddField("gid", gid);
        form.AddField("tag", tag);
        form.AddField("memo", memo);
        string apiURL = string.Format("{0}{1}", url, API.Post.TAG_SET);

        apiURL = RemoveWhiteSpace(apiURL);
        UnityWebRequest www = UnityWebRequest.Post(apiURL, form);
        www.SetRequestHeader("Authorization", token);
        await sendWebRequest(www, apiURL);
        JObject json = null;
        try
        {
            json = JObject.Parse(www.downloadHandler.text);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                ApiError(json, "태그 정보 설정 실패");
                callback.Invoke(false, json);
                Debug.LogError(
                    $"DropOutCancel fail width message : {json.ToString()}\nCode : {www.responseCode}"
                );
            }
            else
            {
                json = JObject.Parse(www.downloadHandler.text);
                callback?.Invoke(true, json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"DropOutCancel fail width message : {e.Message}");
        }
    }

    public async UniTask<UnityWebRequest> RequestChip(string chip_type)
    {
        Console.SpecialLog(chip_type + " 요청 전송");
        var form = new WWWForm();
        form.AddField("type", chip_type);
        var www = await (TryRequestChip($"{url}{API.Post.REQUEST_CHIP}", form));
        return www;
    }

    public void RequestDropoutCancel(Action<bool, JObject> callback)
    {
        StartCoroutine(RequestCropoutCancelCo(callback));
    }

    public IEnumerator RequestCropoutCancelCo(Action<bool, JObject> callback)
    {
        JObject json = null;
        string apiURL = string.Format("{0}{1}", url, API.Put.DROP_OUT_CANCEL);
        JObject dd = new JObject();
        var body = dd.ToString();
        apiURL = RemoveWhiteSpace(apiURL);
        UnityWebRequest www = UnityWebRequest.Put(apiURL, body);
        www.SetRequestHeader("Authorization", token);
        yield return www.SendWebRequest();
        _Log(apiURL, www);
        try
        {
            json = JObject.Parse(www.downloadHandler.text);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                ApiError(json, "회원 탈퇴 취소 실패.");
                callback.Invoke(false, json);
                Debug.LogError(
                    $"DropOutCancel fail width message : {json.ToString()}\nCode : {www.responseCode}"
                );
            }
            else
            {
                json = JObject.Parse(www.downloadHandler.text);
                callback.Invoke(true, json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"DropOutCancel fail width message : {e.Message}");
        }
    }

    public async UniTask<UnityWebRequest> TryRequestChip(string url, WWWForm form)
    {
        url = RemoveWhiteSpace(url);
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        {
            www.SetRequestHeader("Authorization", token);
            await sendWebRequest(www, url);
            return www;
        }
    }

    public async UniTask<UnityWebRequest> RequestDailyClear(int id)
    {
        WWWForm form = new WWWForm();

        form.AddField("refquestId", id);

        string apiURL = string.Format("{0}{1}", url, API.Post.QUEST_CLEAR); /*"api/pub/u/quest/clear"*/
        apiURL = RemoveWhiteSpace(apiURL);
        UnityWebRequest www = UnityWebRequest.Post(apiURL, form);
        www.SetRequestHeader("Authorization", token);

        await www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            JObject json = null;
            try
            {
                json = JObject.Parse(www.downloadHandler.text);
            }
            catch
            {
                Debug.LogError($"{url} handlerText is not json");
            }
            ApiError(json, "DAILY_CLEAR_FAIL_TITLE");
            return null;
        }
        return www;
    }

    public void RequestInviteFriend(int idx, int passward, Action<long, JObject> callback)
    {
        var form = new WWWForm();
        form.AddField("friend_user_idx", idx);
        form.AddField("password", passward);
        StartCoroutine(TryInviteFriend($"{url}{API.Post.INVITE_ROOM}", form, callback));
    }

    public IEnumerator TryInviteFriend(string url, WWWForm form, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);
            yield return sendWebRequest(www, url);
            JObject json = JObject.Parse(www.downloadHandler.text);
            callback(www.responseCode, json);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(string.Format("{0} : {1}", www.error, www.downloadHandler.text));
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                if (www.responseCode == 200) { }
            }
        }
    }

    public void RequestSafeIn(string type, long point, Action<long> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("type", type);
        form.AddField("chip", point.ToString());

        StartCoroutine(
            TryRequestSafeIn(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.SAFE_IN /*"api/pub/u/safe/in"*/
                ),
                form,
                callback
            )
        );
        //Console.Error(string.Format("{0}{1}", url, "api/pub/u/safe/in"));
    }

    public void RequestSafeOut(string type, long point, Action<long> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("type", type);
        form.AddField("chip", point.ToString());

        StartCoroutine(
            TryRequestSafeIn(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.SAFE_OUT /*"api/pub/u/safe/out"*/
                ),
                form,
                callback
            )
        );
        //Console.Error(string.Format("{0}{1}", url, "api/pub/u/safe/out"));
    }

    public async UniTask<bool> Logout()
    {
        var url = string.Format(
            "{0}{1}",
            this.url,
            API.Get.LOGOUT /*"/api/pub/u/logout"*/
        );
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", token);
            await sendWebRequest(www, url);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(string.Format("{0} : {1}", www.error, www.downloadHandler.text));
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                if (www.responseCode == 200)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public IEnumerator TryRequestSafeIn(string url, WWWForm form, Action<long> callback)
    {
        url = RemoveWhiteSpace(url);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);
            yield return sendWebRequest(www, url);
            callback(www.responseCode);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(string.Format("{0} : {1}", www.error, www.downloadHandler.text));
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                if (www.responseCode == 200) { }
            }
        }
    }

    public async UniTask<ResponseData> CheckMyPushToken()
    {
        WWWForm form = new WWWForm();
#if UNITY_ANDROID
        form.AddField("os", "android");
#elif UNITY_IOS
        form.AddField("os", "ios");
#elif UNITY_WEBGL
        form.AddField("os", "web");
#else
#endif
        form.AddField("type", "fcm");
        return await TryCheckCloudMessesingToken(
            string.Format(
                "{0}{1}",
                url,
                API.Post.CHECK_MY_PUSH_TOKEN /*"api/pub/u/push/register"*/
            ),
            form
        );
    }

    public async UniTask<ResponseData> TryCheckCloudMessesingToken(string url, WWWForm form)
    {
        url = RemoveWhiteSpace(url);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);
            await sendWebRequest(www, url);
            JObject json = null;
            try
            {
                json = JObject.Parse(www.downloadHandler.text);
            }
            catch
            {
                return new ResponseData(0, null, false);
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    ecode = json["ecode"].ToObject<int>();
                }
                catch
                {
                    return new ResponseData(0, null, false);
                }

                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(string.Format("{0} : {1}", www.error, www.downloadHandler.text));
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                if (www.responseCode == 200) { }
            }

            return new ResponseData(www.responseCode, json);
        }
    }

    public async UniTask<ResponseData> SendCloudeMessesingToken(string token)
    {
        WWWForm form = new WWWForm();
#if UNITY_ANDROID
        form.AddField("os", "android");
#elif UNITY_IOS
        form.AddField("os", "ios");
#else
#endif
        form.AddField("type", "fcm");
        form.AddField("token", token);
        form.AddField("block", 0);

        return await TrySendCloudMessesingToken(
            string.Format(
                "{0}{1}",
                url,
                API.Post.REGISTER_PUSH_TOKEN /*"api/pub/u/push/register"*/
            ),
            form
        );
    }

    public async UniTask<ResponseData> TrySendCloudMessesingToken(string url, WWWForm form)
    {
        url = RemoveWhiteSpace(url);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);
            await sendWebRequest(www, url);
            _Log(url, www);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(string.Format("{0} : {1}", www.error, www.downloadHandler.text));
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);
                if (www.responseCode == 200) { }
            }

            return new ResponseData(www.responseCode);
        }
    }

    public void RequdstDailyClear(int refId, Action callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("refquestId", refId);
        StartCoroutine(
            TryDailyClear(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.QUEST_CLEAR /*"api/pub/u/quest/clear"*/
                ),
                form,
                callback
            )
        );
    }

    public IEnumerator TryDailyClear(string url, WWWForm form, Action callback)
    {
        url = RemoveWhiteSpace(url);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    callback();
                }
            }
        }
    }

    public async UniTask<UnityWebRequest> RequdstDailyClear(int refId)
    {
        WWWForm form = new WWWForm();
        form.AddField("refquestId", refId);
        return await TryDailyClear(
            string.Format(
                "{0}{1}",
                url,
                API.Post.QUEST_CLEAR /*"api/pub/u/quest/clear"*/
            ),
            form
        );
    }

    public async UniTask<UnityWebRequest> TryDailyClear(string url, WWWForm form)
    {
        url = RemoveWhiteSpace(url);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            await sendWebRequest(www, url);

            return www;
        }
    }

    public void KakaoCustomTokenAPI(string idToken, Action<string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("access_token", Kakao.Instance.GetToken());

        StartCoroutine(
            TryKakaoCustomTokenAPI(
                string.Format("{0}{1}", url, Constant.API.Post.KAKAO_LOGIN),
                form,
                callback
            )
        );
    }

    public void NaverCustomTokenAPI(string idToken, Action<string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("access_token", idToken);

        StartCoroutine(
            TryKakaoCustomTokenAPI(
                string.Format("{0}{1}", url, Constant.API.Post.NAVER_LOGIN),
                form,
                callback
            )
        );
    }

    private IEnumerator TryKakaoCustomTokenAPI(string url, WWWForm form, Action<string> callback)
    {
        url = RemoveWhiteSpace(url);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return sendWebRequest(www, url);

            string customToken = "";

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(string.Format("{0} : {1}", www.error, www.downloadHandler.text));
                ErrorMessageManager.Instance.AddNetworkError(
                    0,
                    www.error,
                    www.downloadHandler.text
                );
                FirebaseManager.Instance.loginType = LoginType.KAKAO;
                FirebaseManager.Instance.SignOut();
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    customToken = json.GetValue("customToken").ToString();
                    callback(customToken);
                }
            }
        }
    }

    /// <summary>
    /// POST : PubAPI에게 생성할 수 있는 닉네임인지 체크를 요청합니다.
    /// </summary>
    /// <param name="nickname">닉네임</param>
    /// <param name="callback">요청 결과 콜백 / int statusCode, JObject jsonData</param>
    public void CheckNickname(string nickname, Action<long> callback)
    {
        //HTTPRequest request = new HTTPRequest(new Uri(string.Format("{0}{1}", url, "api/pub/chknick")), HTTPMethods.Post, (req, res) =>
        //{
        //    Console.Log(res.DataAsText);
        //    JObject json = JObject.Parse(res.DataAsText);

        //    callback(res.StatusCode, json);
        //});
        //HTTPUrlEncodedForm form = new HTTPUrlEncodedForm();
        //form.AddField("nick", nickname);
        //request.SetForm(form);
        //request.Send();

        WWWForm form = new WWWForm();
        form.AddField("nick", nickname);

        StartCoroutine(
            TryCheckNickname(
                string.Format("{0}{1}", url, Constant.API.Post.CHECK_NICKNAME),
                form,
                callback
            )
        );
    }

    private IEnumerator TryCheckNickname(string url, WWWForm form, Action<long> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode != 422)
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }

            callback(www.responseCode);
        }
    }

    /// <summary>
    /// POST : PubAPI에게 회원가입을 요청합니다.
    /// </summary>
    /// <param name="nickname">체크 완료된 닉네임</param>
    /// <param name="callback">요청 결과 콜백 / int statusCode, JObject jsonData</param>
    public void UseItemApi(int refitemidx, Action<long, JObject> callback)
    {
        var form = new WWWForm();
        form.AddField("refitemidx", refitemidx);
        StartCoroutine(
            TryUseItem(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.USE_ITEM /*"api/pub/u/item/use"*/
                ),
                form,
                callback
            )
        );
    }

    private IEnumerator TryUseItem(string url, WWWForm form, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    //token = json.GetValue("pubtoken").ToString();
                    Console.Log("아이템 사용 성공");
                    callback(www.responseCode, json);
                }
            }
        }
    }

    public void GetPayMent(Action<long, JObject> callback)
    {
        StartCoroutine(
            TryGetPayMent(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Get.TOTAL_MONTHLY_PAYMENT /*"api/pub/u/iap/payment"*/
                ),
                callback
            )
        );
    }

    public IEnumerator TryGetPayMent(string url, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                callback(www.responseCode, json);
            }
        }
    }

    public void UpdateNickname(string nickname, Action<long> callback)
    {
        JObject dd = new JObject();
        dd.Add("nick", nickname);
        var st = dd.ToString();

        StartCoroutine(
            TryUpdateNickname(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Put.UPDATE_NICK /*"api/pub/u/update_nick"*/
                ),
                st,
                callback
            )
        );
    }

    private IEnumerator TryUpdateNickname(string url, string jsonString, Action<long> callback)
    {
        url = RemoveWhiteSpace(url);

        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
        using (UnityWebRequest www = UnityWebRequest.Put(url, bytes))
        {
            www.SetRequestHeader("Authorization", token);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                if (www.responseCode == 200)
                {
                    //token = json.GetValue("pubtoken").ToString();
                    Debug.Log("닉네임 변경 성공");
                }
            }

            callback(www.responseCode);
        }
    }

    public void GetGameConfigPubApi(Action<long, JObject> callback)
    {
        Console.Log("Start GetGameConfig PupAPI");
        StartCoroutine(
            TryGetGameConfig(
                url
                    + API.Get.CONFIG /*"api/pub/config"*/
                ,
                callback
            )
        );
    }

    private IEnumerator TryGetGameConfig(string url, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            //www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
                ErrorMessageManager.Instance.AddNetworkError(
                    0,
                    www.error,
                    www.downloadHandler.text
                );
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    callback(www.responseCode, json);
                }
            }
        }
    }

    public void RequestGetRefQuest(Action<JObject> callback)
    {
        StartCoroutine(
            TryGetRefQuest(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Get.REF_QUEST_LIST /*"api/pub/refquest"*/
                ),
                callback
            )
        );
    }

    private IEnumerator TryGetRefQuest(string url, Action<JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
                Debug.Log(www.downloadHandler.text);
                ErrorMessageManager.Instance.AddNetworkError(
                    0,
                    www.error,
                    www.downloadHandler.text
                );
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    callback(json);
                }
            }
        }
    }

    public void RegisterPubAPI(string nickname, Action<long, JObject> callback, string token = null)
    {
        //HTTPRequest request = new HTTPRequest(new Uri(string.Format("{0}{1}", url, "api/pub/join")), HTTPMethods.Post, (req, res) =>
        //{
        //    Console.Log(res.DataAsText);
        //    JObject json = JObject.Parse(res.DataAsText);

        //    if (res.StatusCode == 200)
        //    {
        //        token = json.GetValue("pubtoken").ToString();
        //    }

        //    callback(res.StatusCode, json);
        //});
        //HTTPUrlEncodedForm form = new HTTPUrlEncodedForm();
        //form.AddField("nick", nickname);
        //form.AddField("idToken", FirebaseManager.Instance.token);
        //request.SetForm(form);
        //request.Send();

        WWWForm form = new WWWForm();
        form.AddField("nick", nickname);
        var to = string.IsNullOrEmpty(token) ? FirebaseManager.Instance.token : token;
        form.AddField("idToken", to);

        StartCoroutine(
            TryRegisterPubAPI(
                string.Format("{0}{1}", url, Constant.API.Post.REGISTER),
                form,
                callback
            )
        );
    }

    private IEnumerator TryRegisterPubAPI(string url, WWWForm form, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return sendWebRequest(www, url);

            JObject json = null;
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
                Debug.Log(www.downloadHandler.text);
                ErrorMessageManager.Instance.AddNetworkError(
                    0,
                    www.error,
                    www.downloadHandler.text
                );
            }
            else
            {
                json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    token = json.GetValue("pubtoken").ToString();
                }
            }

            callback(www.responseCode, json);
        }
    }

    /// <summary>
    /// GET : PubAPI에게 게임서버 로그인에 필요한 데이터를 요청합니다.
    /// </summary>
    /// <param name="callback">요청 결과 콜백 / int statusCode, JObject jsonData</param>
    public async UniTask<ResponseData> GetGameLoginDataPubAPI()
    {
        //HTTPRequest request = new HTTPRequest(new Uri(string.Format("{0}{1}", url, "api/pub/u/gamein")), HTTPMethods.Get, (req, res) =>
        //{
        //    Console.Log(res.DataAsText);
        //    JObject json = JObject.Parse(res.DataAsText);

        //    callback(res.StatusCode, json);
        //});
        //request.SetHeader("Authorization", string.Format("{0}", token));
        //request.Send();
        return await TryGetGameLoginDataPubAPI(url + Constant.API.Get.GAME_IN);
    }

    private async UniTask<ResponseData> TryGetGameLoginDataPubAPI(string url)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", token);

            await sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                JObject json = JObject.Parse(www.downloadHandler.text);
                int ecode = 0;
                try
                {
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    return new ResponseData(www.responseCode, json);
                }
            }

            return new ResponseData(0, null, false);
        }
    }

    public void GetShopListPubAPI(Action<long, JObject> callback)
    {
        StartCoroutine(
            TryGetShopListPubAPI(
                url
                    + API.Get.SHOP_LIST /*"api/pub/shop"*/
                ,
                callback
            )
        );
    }

    public void GetItemListPubApi(Action<long, JObject> callback)
    {
        StartCoroutine(
            TryGetShopListPubAPI(
                url
                    + API.Get.REF_ITEM_LIST /*"api/pub/refitem"*/
                ,
                callback
            )
        );
    }

    public async UniTask<UnityWebRequest> GetMyItemApi()
    {
        return await TryGetApi(
            url + API.Get.MY_ITEM_LIST /*"api/pub/refitem"*/
        );
    }

    private async UniTask<UnityWebRequest> TryGetApi(string url)
    {
        url = RemoveWhiteSpace(url);

        UnityWebRequest www = UnityWebRequest.Get(url);
        {
            www.SetRequestHeader("Authorization", token);

            await sendWebRequest(www, url);
            return www;
        }
    }

    private IEnumerator TryGetShopListPubAPI(string url, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            //www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
                ErrorMessageManager.Instance.AddNetworkError(
                    0,
                    www.error,
                    www.downloadHandler.text
                );
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    callback(www.responseCode, json);
                }
            }
        }
    }

    public void RequestSkillList(Action<long, JObject> callback)
    {
        StartCoroutine(
            TryRequestSkillList(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Get.MY_SKILL_LIST
                ) /*"api/pub/u/skill"*/
                ,
                callback
            )
        );
    }

    private IEnumerator TryRequestSkillList(string url, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;

                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                callback(www.responseCode, json);
            }
        }
    }

    public void UseSkill(int idx, Action<long, JObject> callback)
    {
        var form = new WWWForm();
        form.AddField("refId", idx);
        StartCoroutine(
            TryUseSkill(
                url
                    + API.Post.USE_SKILL /*"api/pub/u/skill/use"*/
                ,
                form,
                callback
            )
        );
    }

    private IEnumerator TryUseSkill(string url, WWWForm form, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                callback(www.responseCode, json);
            }
        }
    }

    public void ReadMailPubAPI(int idx, Action<long, JObject> callback)
    {
        var form = new WWWForm();
        form.AddField("mailIdx", idx);
        StartCoroutine(
            TryReadMailPubAPI(
                url
                    + API.Post.READ_MAIL /*"api/pub/u/mail/read"*/
                ,
                form,
                callback
            )
        );
    }

    public void GetInvenListPubAPI(Action<long, JObject> callback)
    {
        StartCoroutine(
            TryGetInvenListPubAPI(
                url
                    + API.Get.MY_ITEM_LIST /*"api/pub/u/item"*/
                ,
                callback
            )
        );
    }

    private IEnumerator TryReadMailPubAPI(string url, WWWForm form, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);
                if (www.responseCode == 200)
                {
                    callback(www.responseCode, json);
                }
            }
        }
    }

    public void GetMailListPubApi(Action<long, JObject> callback, int page = 1, int per = 20)
    {
        var form = new WWWForm();
        form.AddField("page", page);
        form.AddField("per", per);
        StartCoroutine(
            TryGetMailListPubAPI(
                url
                    + API.Post.MY_MAIL_LIST /*"api/pub/u/mail"*/
                ,
                form,
                callback
            )
        );
    }

    private IEnumerator TryGetMailListPubAPI(
        string url,
        WWWForm form,
        Action<long, JObject> callback
    )
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    callback(www.responseCode, json);
                }
            }
        }
    }

    private IEnumerator TryGetInvenListPubAPI(string url, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                callback(www.responseCode, json);
            }
        }
    }

    /// <summary>
    /// GET : PubAPI에게 유저정보를 요청합니다.
    /// </summary>
    /// <param name="callback">요청 결과 콜백 / int statusCode, JObject jsonData</param>
    public void GetUserInfoPubAPI(Action<long, JObject> callback)
    {
        Console.Log("Start GetUserInfo PupAPI");
        StartCoroutine(
            TryGetUserInfoPubAPI(
                url
                    + API.Get.INFO /*"api/pub/u/info"*/
                ,
                callback
            )
        );
    }

    private IEnumerator TryGetUserInfoPubAPI(string url, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);
            Console.Log("GetUserInfo Web Load Success");
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        ecode,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);
                Debug.Log("PubUserInfo : " + json.ToString());

                if (www.responseCode == 200)
                {
                    callback(www.responseCode, json);
                }
            }
        }
    }

    public async UniTask<UnityWebRequest> TryGetAPI(string url)
    {
        url = RemoveWhiteSpace(url);

        UnityWebRequest www = UnityWebRequest.Get(url);

        www.SetRequestHeader("Authorization", token);

        await sendWebRequest(www, url);
        return www;
    }

    // /  /////////////////////

    public async UniTask<UnityWebRequest> GetUserInfoPubAPIAsync(string url)
    {
        url = RemoveWhiteSpace(url);

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", token);

        await www.SendWebRequest();
        Console.Log("GetUserInfo Web Load Success");
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            int ecode = 0;
            try
            {
                JObject json = JObject.Parse(www.downloadHandler.text);
                ecode = json["ecode"].ToObject<int>();
            }
            catch { }
            if (ecode == 402)
            {
                NetWorkErrorLogOut();
            }
            else
            {
                Debug.Log(www.error);
                Debug.Log(www.downloadHandler.text);
                ErrorMessageManager.Instance.AddNetworkError(
                    ecode,
                    www.error,
                    www.downloadHandler.text
                );
            }
        }
        return www;
    }

    public async UniTask PointInAPI(string type, long chip, Action<long, JObject> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("type", type);
        form.AddField("chip", chip.ToString());

        await TryPointInAPI(
            string.Format(
                "{0}{1}",
                url,
                API.Post.POINT_IN /*"api/pub/u/point/in"*/
            ),
            form,
            callback
        );
    }

    private async UniTask TryPointInAPI(string url, WWWForm form, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            await sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                callback?.Invoke(www.responseCode, json);
            }
        }
    }

    public async UniTask<UnityWebRequest> PointInAPIAsync(string type, long chip)
    {
        WWWForm form = new WWWForm();
        form.AddField("type", type);
        form.AddField("chip", chip.ToString());

        string apiURL = string.Format(
            "{0}{1}",
            url,
            API.Post.POINT_IN /*"api/pub/u/point/in"*/
        );
        apiURL = RemoveWhiteSpace(apiURL);
        UnityWebRequest www = UnityWebRequest.Post(apiURL, form);
        www.SetRequestHeader("Authorization", token);

        await www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            int ecode = 0;
            try
            {
                JObject json = JObject.Parse(www.downloadHandler.text);
                ecode = json["ecode"].ToObject<int>();
            }
            catch { }
            if (ecode == 402)
            {
                NetWorkErrorLogOut();
            }
            else
            {
                Debug.Log(www.error);
                Debug.Log(www.downloadHandler.text);
                ErrorMessageManager.Instance.AddNetworkError(
                    0,
                    www.error,
                    www.downloadHandler.text
                );
            }
        }
        return www;
    }

    public void PurchasingVerificationAPI(Product product, Action<long, JObject> callback)
    {
        Console.Log("결제 API호출");

        WWWForm form = new WWWForm();
        form.AddField("receipt", product.receipt);
        form.AddField("product_id", product.definition.id);
        form.AddField(
            "subscription",
            (product.definition.type == UnityEngine.Purchasing.ProductType.Subscription).ToString()
        );

        Debug.Log(product.receipt);

        StartCoroutine(
            TryPurchasingVerificationAPI(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.VALIDATE_RECEIPT /*"api/pub/u/iap/receipt/validate"*/
                ),
                form,
                callback
            )
        );
    }

    public void PurchasingVerificationAPI(PurchaseData product, Action<long, JObject> callback)
    {
        Console.Log("결제 API호출");
        WWWForm form = new WWWForm();
        var json = JObject.Parse(product.JsonReceipt);
        json.Add("Store", "OneStore");
        form.AddField("receipt", json.ToString());
        form.AddField("product_id", product.ProductId);
        form.AddField("subscription", false.ToString());

        Debug.Log(product.JsonReceipt);

        StartCoroutine(
            TryPurchasingVerificationAPI(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.VALIDATE_RECEIPT /*"api/pub/u/iap/receipt/validate"*/
                ),
                form,
                callback
            )
        );
    }

    private IEnumerator TryPurchasingVerificationAPI(
        string url,
        WWWForm form,
        Action<long, JObject> callback
    )
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                    callback?.Invoke(ecode, json);
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    //ErrorMessageManager.Instance.AddNetworkError(0, www.error, www.downloadHandler.text);
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                callback(www.responseCode, json);
            }
        }
    }

    public void EquipItemAPI(int idx, Action<long> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("refIdx", idx);

        StartCoroutine(
            TryEquipItem(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.EQUIP_ITEM /*"api/pub/u/item/equip"*/
                ),
                form,
                callback
            )
        );
    }

    private IEnumerator TryEquipItem(string url, WWWForm form, Action<long> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                callback(www.responseCode);
            }
        }
    }

    public void GetFriendListAPI(Action<long, JObject> callback)
    {
        StartCoroutine(
            TryGetFriendListAPI(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Get.FRIEND_LIST /*"api/pub/u/friend"*/
                ),
                callback
            )
        );
    }

    public void GetFriendListAPI(int page, Action<long, JObject> callback)
    {
        StartCoroutine(
            TryGetFriendListAPI(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Get.FRIEND_LIST + "?page=" + page.ToString() /*"api/pub/u/friend"*/
                ),
                callback
            )
        );
    }

    public void GetFriendsRequestListAPI(Action<long, JObject> callback)
    {
        StartCoroutine(
            TryGetFriendListAPI(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Get.FRIEND_REQUEST_LIST /*"api/pub/u/friend/request"*/
                ),
                callback
            )
        );
    }

    private IEnumerator TryGetFriendListAPI(string url, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    callback(www.responseCode, json);
                }
            }
        }
    }

    public void FriendDeclineAPI(int idx, Action<long, JObject> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("requestIdx", idx);

        StartCoroutine(
            TryPost(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.REQUEST_DECLINE_FRIEND /*"api/pub/u/friend/request/decline"*/
                ),
                form,
                callback
            )
        );
    }

    public void FriendAcceptAPI(int idx, Action<long, JObject> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("requestIdx", idx);

        StartCoroutine(
            TryPost(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.REQUEST_ACCEPT_FRIEND /*"api/pub/u/friend/request/accept"*/
                ),
                form,
                callback
            )
        );
    }

    public void FriendRequestAPI(string nick, Action<long, JObject> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("friendNick", nick);

        StartCoroutine(
            TryPost(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.REQUEST_FRIEND /*"api/pub/u/friend/request"*/
                ),
                form,
                callback
            )
        );
    }

    private IEnumerator TryPost(string url, WWWForm form, Action<long, JObject> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                JObject json = null;
                int ecode = 0;
                try
                {
                    json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch
                {
                    Console.Log("is not json or not Contain ecode");
                }

                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    callback(www.responseCode, json);
                }
            }
        }
    }

    public void PingCheck()
    {
        Console.Log("API - PING");
        StartCoroutine(
            TryPing(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Get.PING /*"api/pub/u/ping"*/
                )
            )
        );
    }

    public void PingCheckCancel()
    {
        CancelInvoke("PingCheck");
        Console.Log("API - CANCEL");
    }

    public async UniTask<UnityWebRequest> PingCheckAsync()
    {
        string url = RemoveWhiteSpace(
            string.Format(
                "{0}{1}",
                this.url,
                API.Get.PING /*"api/pub/u/ping"*/
            )
        );
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", token);

        await www.SendWebRequest();
        return www;
    }

    private IEnumerator TryPing(string url, Action<long> callback = null)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                //if (ecode == 402)
                //{
                //    NetWorkErrorLogOut();
                //}
                //else
                //{
                //    Debug.Log(www.error);
                //    Debug.Log(www.downloadHandler.text);
                //    ErrorMessageManager.Instance.AddNetworkError(0, www.error, www.downloadHandler.text);
                //}

                GameManager.instance.VersionCheck();
                callback?.Invoke(ecode);
            }
            else
            {
                if (www.responseCode == 200)
                {
                    //CancelInvoke("PingCheck");
                    //Invoke("PingCheck", 600);
                    Console.Log("API - PONG");
                    callback?.Invoke(www.responseCode);
                }
            }
        }
    }

    public void AdultCertCheck(Action successCallback, Action failCallback)
    {
        StartCoroutine(
            TryAdultCertCheck(
                string.Format("{0}{1}", url, Constant.API.Get.CHECK_KAKAO_CERT),
                successCallback,
                failCallback
            )
        );
    }

    private IEnumerator TryAdultCertCheck(string url, Action successCallback, Action failCallback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                //JObject json = JObject.Parse(www.downloadHandler.text);
                //int ecode = 0;
                //try
                //{
                //    ecode = json["ecode"].ToObject<int>();
                //}
                //catch { }
                //if (ecode == 402)
                //{
                //    NetWorkErrorLogOut();
                //}
                //else
                //{
                //    Debug.Log(www.error);
                //    Debug.Log(www.downloadHandler.text);
                //    ErrorMessageManager.Instance.InvokeNetworkError(0, www.error, www.downloadHandler.text);
                //}
                failCallback.Invoke();
                Debug.Log(www.error);
                Debug.Log(www.downloadHandler.text);
            }
            else
            {
                if (www.responseCode == 200)
                {
                    successCallback.Invoke();
                }
            }
        }
    }

    public void SelfBlock(double time, Action<long> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("banTime", (int)time);
        StartCoroutine(
            TrySelfBlock(string.Format("{0}{1}", url, API.Post.SELF_BAN), form, callback)
        );
    }

    private IEnumerator TrySelfBlock(string url, WWWForm form, Action<long> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                JObject json = null;
                int ecode = 0;
                try
                {
                    json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch
                {
                    Console.Log("is not json or not Contain ecode");
                }

                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                JObject json = JObject.Parse(www.downloadHandler.text);

                if (www.responseCode == 200)
                {
                    callback(www.responseCode);
                }
            }
        }
    }

    public void RequestUpdateCheck(string check, Action<long> callback)
    {
        JObject json = new JObject();
        json.Add("kind", check);

        StartCoroutine(
            TryUpdateCheck(
                string.Format("{0}{1}", url, API.Put.UPDATE_CHECK),
                json.ToString(),
                callback
            )
        );
        ;
    }

    public IEnumerator TryUpdateCheck(string url, string jsonString, Action<long> callback)
    {
        url = RemoveWhiteSpace(url);

        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
        using (UnityWebRequest www = UnityWebRequest.Put(url, bytes))
        {
            www.SetRequestHeader("Authorization", token);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return sendWebRequest(www, url);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                int ecode = 0;
                try
                {
                    JObject json = JObject.Parse(www.downloadHandler.text);
                    ecode = json["ecode"].ToObject<int>();
                }
                catch { }
                if (ecode == 402)
                {
                    NetWorkErrorLogOut();
                }
                else
                {
                    Debug.Log(www.error);
                    Debug.Log(www.downloadHandler.text);
                    ErrorMessageManager.Instance.AddNetworkError(
                        0,
                        www.error,
                        www.downloadHandler.text
                    );
                }
            }
            else
            {
                if (www.responseCode == 200)
                {
                    //token = json.GetValue("pubtoken").ToString();
                    Debug.Log("업데이트 성공");
                }
            }

            callback(www.responseCode);
        }
    }

    public async UniTask<UnityWebRequest> PostWithoutToken(string url, WWWForm form = null)
    {
        url = RemoveWhiteSpace(url);
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        {
            await sendWebRequest(www, url);

            return www;
        }
    }

    public string RemoveWhiteSpace(string str)
    {
        return str.Replace("\u200B", "");
    }

    public void NetWorkErrorLogOut()
    {
        WebSocketManager.defaultCli.OnExitOnce += (reson) =>
        {
            DevManager.Instance.GsLogin = false;
            DevManager.Instance.WsDelegate -= 1;
            DevManager.Instance.WsConnect = false;
            FirebaseManager.Instance.SignOut();
            ErrorMessageManager.Instance.AddGameError(
                402,
                "SYS_ERR_USER_TOKEN_EXPIRE",
                "SYS_ERR_USER_TOKEN_EXPIRE_GO_LOGIN",
                ErrorHandlingType.RECALL,
                null,
                () =>
                {
                    CustomSceneManager.LoadLoginScene();
                }
            );
        };
        WebSocketManager.defaultCli.Close();
    }

    public void UseItemAPI(int idx, Action<long, string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("itemIdx", idx);

        StartCoroutine(
            TryUseItem(
                string.Format(
                    "{0}{1}",
                    url,
                    API.Post.USE_ITEM /*"api/pub/u/item/use"*/
                ),
                form,
                callback
            )
        );
    }

    private IEnumerator TryUseItem(string url, WWWForm form, Action<long, string> callback)
    {
        url = RemoveWhiteSpace(url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", token);

            yield return www.SendWebRequest();
            _Log(url, www);

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                JObject json = null;
                try
                {
                    json = JObject.Parse(www.downloadHandler.text);
                }
                catch
                {
                    Debug.LogError($"{url} handlerText is not json");
                }
                ApiError(json, "USE_ITEM_FAIL_TITLE");
            }
            else
            {
                callback(www.responseCode, www.downloadHandler.text);
            }
        }
    }

    private void ApiError(
        JObject json,
        string title = "",
        ErrorHandlingType handlerType = ErrorHandlingType.NONE,
        Action action = null,
        Action callback = null
    )
    {
        string error = json.ValueOrDefault<string>("error", "");
        int ecode = json.ValueOrDefault<int>("ecode", (int)ERR.OK);

        if (ecode == 402)
        {
            NetWorkErrorLogOut();
        }
        else
        {
            Debug.Log(string.Format("{0} : {1}", title, error));
            ErrorMessageManager.Instance.AddNetworkError(
                ecode,
                title,
                error,
                handlerType,
                action,
                callback
            );
        }
    }
}
