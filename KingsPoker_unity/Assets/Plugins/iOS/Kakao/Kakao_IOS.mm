#import <KakaoOpenSDK/KakaoOpenSDK.h>

extern "C" void KakaoLogin_IOS() {
    [[KOSession sharedSession] close];

    [[KOSession sharedSession] openWithCompletionHandler:^(NSError *error) {
        if (error) {
            UnitySendMessage("KakaoManager", "OnLoginFail", (char *)[error.localizedDescription UTF8String]);
        }
        else {
            UnitySendMessage("KakaoManager", "`", "");
        }
    }];
}

extern "C" void KakaoLogout_IOS() {
    [[KOSession sharedSession] logoutAndCloseWithCompletionHandler:^(BOOL success, NSError *error) {
        if (error) {
        }
        else {
            UnitySendMessage("KakaoManager", "OnLogout", "");
        }
    }];
}

extern "C" void KakaoUnlink_IOS() {
    [KOSessionTask unlinkTaskWithCompletionHandler:^(BOOL success, NSError *error) {
        if (error) {
            UnitySendMessage("KakaoManager", "OnUnLinkFail", (char *)[error.localizedDescription UTF8String]);
        }
        else {
            UnitySendMessage("KakaoManager", "OnUnLinkSuccess", "");
        }
    }];
}

extern "C" char * KakaoGetToken_IOS() {
    NSString * token = [KOSession sharedSession].token.accessToken;
    return strdup([token UTF8String]);
}
