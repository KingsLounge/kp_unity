using System;

namespace Constant
{
    public class API
    {
        public class Get
        {
            public const string PING = "/api/pub/u/ping";
            public const string INFO = "/api/pub/u/info";
            public const string LOGOUT = "/api/pub/u/logout";
            public const string CHECK_KAKAO_CERT = "/api/pub/u/kcert/check";
            public const string GAME_IN = "/api/pub/u/gamein";
            public const string GAME_OUT = "/api/pub/u/gameout";
            public const string CONFIG = "/api/pub/config";

            public const string MY_ITEM_LIST = "/api/pub/u/item";

            public const string SHOP_LIST = "/api/pub/shop";
            public const string REF_ITEM_LIST = "/api/pub/refitem";

            public const string CHECK_PENDING_RECEIPT = "/api/pub/u/iap/receipt/pending";
            public const string TOTAL_MONTHLY_PAYMENT = "/api/pub/u/iap/payment";

            public const string REF_QUEST_LIST = "/api/pub/refquest";

            public const string MY_SKILL_LIST = "/api/pub/u/skill";

            public const string FRIEND_LIST = "/api/pub/u/friend";
            public const string FRIEND_REQUEST_LIST = "/api/pub/u/friend/request";
        }

        public class Post
        {
            public const string LOGIN = "/api/pub/login";
            public const string KAKAO_LOGIN = "/api/pub/klogin";
            public const string NAVER_LOGIN = "/api/pub/nlogin";
            public const string CHECK_NICKNAME = "/api/pub/chknick";
            public const string REGISTER = "/api/pub/join";

            public const string POINT_IN = "/api/pub/u/point/in";
            public const string POINT_OUT = "/api/pub/u/point/out";
            public const string SAFE_IN = "/api/pub/u/safe/in";
            public const string SAFE_OUT = "/api/pub/u/safe/out";

            public const string USE_ITEM = "/api/pub/u/item/use";
            public const string BUY_ITEM = "/api/pub/u/item/buy";
            public const string EQUIP_ITEM = "/api/pub/u/item/equip";

            public const string VALIDATE_RECEIPT = "/api/pub​/u/iap/receipt/validate";

            public const string REGISTER_PUSH_TOKEN = "/api/pub/u/push/register";
            public const string CHECK_MY_PUSH_TOKEN = "/api/pub/u/push/token";

            public const string MY_MAIL_LIST = "/api/pub/u/mail";
            public const string READ_MAIL = "/api/pub/u/mail/read";

            public const string QUEST_CLEAR = "/api/pub/u/quest/clear";

            public const string USE_SKILL = "/api/pub/u/skill/use";

            public const string REQUEST_FRIEND = "/api/pub/u/friend/request";
            public const string REQUEST_ACCEPT_FRIEND = "/api/pub/u/friend/request/accept";
            public const string REQUEST_DECLINE_FRIEND = "/api/pub/u/friend/request/decline";
            public const string SELF_BAN = "/api/pub/u/self/ban";
            public const string INVITE_ROOM = "/api/pub/u/mail/inviteroom";
            public const string REQUEST_CHIP = "/api/pub/u/chip/request";
            public const string UPLOAD_CAFE_LOGO = "/api/pub/u/cafe/icon/upload";

            public const string REQUEST_USER_SCORE = "/api/pub/game/score";
            public const string TAG_MY_GET = "/api/pub/u/tag/my/get";
            public const string TAG_MY_SET = "/api/pub/u/tag/my/set";
            public const string TAG_SET = "/api/pub/u/tag/set";
            public const string TAG_GET = "/api/pub/u/tag/get";
        }

        public class Put
        {
            public const string UPDATE_NICK = "/api/pub/u/update_nick";
            public const string UPDATE_CHECK = "/api/pub/u/update_check";

            public const string DROP_OUT = "/api/pub/u/dropout";
            public const string DROP_OUT_CANCEL = "/api/pub/u/dropout/cancel";
        }
    }

    public class URL
    {
        public const string CAFE_INVITE = "https://kingspoker.app/invite";
        public const string ANDROID_UPDATE_URL =
            "https://kingspoker.s3.ap-northeast-2.amazonaws.com/apks/index.html";
        public const string ONE_STORE_UPDATE_URL = "onestore://common/product/0000778358";
        public const string IOS_UPDATE_URL = "https://apps.apple.com/app/id6469089202";
    }

    public class GameConfig
    {
        public const bool USE_PHOTO_URL = false; // 프로필 사진 사용

        public const bool SHOW_FIRST_CAFE = false; // 첫번째 카페 보이기
        public const string defaltGameOption =
            "{\"personnel\":9,\"action_time\":15,"
            + "\"tb2\":false,\"tb2_time\":30,\"tb2_price\":1,"
            + "\"small_blind\":10,\"blind\":20,\"ante\":0,"
            + "\"rake\":true,\"rake_2\":4,\"rakecap\":10,\"jackpot_fund\":0.1,"
            + "\"passive_rake\":true,\"passive_rake_select\":0,\"pr1\":1,\"pr2\":10,\"pr3\":1,\"pr4\":10,\"game_length\":12,"
            + "\"insurance\":false,\"insurance_house_commission\":1,"
            + "\"autostart_condition1\":2,"
            + "\"stack_removal\":false,\"stack_removal_select\":0,\"stack_removal_bb1\":50,"
            + "\"stack_removal_option1\":10,\"stack_removal_bb2\":50,\"stack_removal_option2\":10,"
            + "\"straddle\":false,"
            + "\"run_it_multi\":false,"
            + "\"random_sit_in\":false,"
            + "\"anonymous\":false,\"community_ban\":false,\"spectator_ban\":false,"
            + "\"game_type\":1,"
            + "\"buyin_min\":400,\"buyin_max\":20000000000,"
            + "\"cafeIdx\":1}";
        public const bool CAFE_BOTTOM_MENU = false;
        public const bool TNMT = false;
        public const bool POINT = false;
        public const long BUY_LIMIT_PRICE = 700000; // 월 구매 한도 제한
        public const bool FOLD_WIDTH_CHECK = false;
        public const string bannerAPILink = "/api/getBannerList";
        public const string onestoreAppKey =
            "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCuLEw3Y9raOL76/RLe/OEmhM0iozBBeHFoYs8gPwZCA/Zq225TzpMzggR+btPSHJyOUvHiGFrupszQNKMwB2P8CuhkU/TYG7Z9fGz4SpPGlKC72FnJK+blaFsQy/f+r/oBPxICb0Ghdvsp6ya6xmFcX9+1Vz6WaPa1Dle99yVlgQIDAQAB";

        public static string kingshillUrl = "http://www.kingshill.co.kr";
        public static string KingshillUrl
        {
            get
            {
                var hillUrl = ServerConfigManager.GetConfig("kinshillUrl");
                if (hillUrl != null)
                {
                    return hillUrl.ToObject<string>();
                }
                else
                {
                    return kingshillUrl;
                }
            }
        }
        public const string bannerAPi = "/api/getBannerList";
        public const float bannerDuration = 5f;
        public const string appleAppId = "6469089202";
    }
}
