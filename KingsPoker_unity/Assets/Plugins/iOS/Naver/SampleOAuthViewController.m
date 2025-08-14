
//
//  SampleOAuthViewController.m
//  NaverOAuthSample
//
//  Created by suejinv on 12. 3. 28..
//  Copyright 2014 Naver Corp. All rights reserved.
//

#import "SampleOAuthViewController.h"
#import <SafariServices/SafariServices.h>
#import <NaverThirdPartyLogin/NaverThirdPartyLogin.h>

@interface SampleOAuthViewController (PrivateAPI)
- (BOOL)isStartsWithString:(NSString *)originString prefix:(NSString *)prefix;
- (BOOL)isContainWithString:(NSString *)originString string:(NSString *)string;
- (NSString *)parameterValueWithUrl:(NSString *)url paramName:(NSString *)name;
- (NSDictionary *)makeHeaderDictionary:(NSString *)headerString;
@end

API_AVAILABLE(ios(11.0))
@interface SampleOAuthViewController ()
@property(nonatomic,strong) SFAuthenticationSession *authSession;
@end

@implementation SampleOAuthViewController
static SampleOAuthViewController * _sharedInstance;

- (id)init {
    if ((self = [super init])) {
        _thirdPartyLoginConn = [NaverThirdPartyLoginConnection getSharedInstance];
        _thirdPartyLoginConn.delegate = self;
#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 70000
        int version = [[[UIDevice currentDevice] systemVersion] intValue];
        if (7 <= version) {
            //self.automaticallyAdjustsScrollViewInsets = NO;
        }
#endif
    }
    return self;
}

+ (nullable SampleOAuthViewController *) sharedInstance
{
    if(_sharedInstance == nil)
    {
        _sharedInstance = SampleOAuthViewController.alloc;
    }
    return _sharedInstance;
}

- (void)dealloc
{
    _thirdPartyLoginConn.delegate = nil;
}

- (void)requestThirdpartyLogin
{
    // NaverThirdPartyLoginConnection의 인스턴스에 서비스앱의 url scheme와 consumer key, consumer secret, 그리고 appName을 파라미터로 전달하여 3rd party OAuth 인증을 요청한다.
    
    NaverThirdPartyLoginConnection *tlogin = [NaverThirdPartyLoginConnection getSharedInstance];
    [tlogin setConsumerKey:kConsumerKey];
    [tlogin setConsumerSecret:kConsumerSecret];
    [tlogin setAppName:kServiceAppName];

    [tlogin setServiceUrlScheme:kServiceAppUrlScheme];
    [tlogin requestThirdPartyLogin];
}

- (void)requestThirdpartyLogin : (char*) consumerKey : (char*) consumerSecret : (char *) appName : (char *) urlScheme 
{
    // NaverThirdPartyLoginConnection의 인스턴스에 서비스앱의 url scheme와 consumer key, consumer secret, 그리고 appName을 파라미터로 전달하여 3rd party OAuth 인증을 요청한다.
    
    NaverThirdPartyLoginConnection *tlogin = [NaverThirdPartyLoginConnection getSharedInstance];
    [tlogin resetToken];
    [tlogin setConsumerKey:kConsumerKey];
    [tlogin setConsumerSecret:kConsumerSecret];
    [tlogin setAppName:kServiceAppName];

    [tlogin setServiceUrlScheme:kServiceAppUrlScheme];
    [tlogin requestThirdPartyLogin];
}


- (void)requestAccessTokenWithRefreshToken
{
    NaverThirdPartyLoginConnection *tlogin = [NaverThirdPartyLoginConnection getSharedInstance];
    [tlogin setConsumerKey:@"key"];
    [tlogin setConsumerSecret:@"Secret"];
    [tlogin requestAccessTokenWithRefreshToken];
}

- (void)resetToken
{
    [_thirdPartyLoginConn resetToken];
}

- (void)requestDeleteToken
{
    NaverThirdPartyLoginConnection *tlogin = [NaverThirdPartyLoginConnection getSharedInstance];
    [tlogin requestDeleteToken];
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    if ([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPad) {
        return YES;
    } if ([[NaverThirdPartyLoginConnection getSharedInstance] isOnlyPortraitSupportedInIphone]) {
        return interfaceOrientation == UIInterfaceOrientationMaskPortrait;
    } else {
        return (interfaceOrientation == UIInterfaceOrientationPortrait) |
        (interfaceOrientation == UIInterfaceOrientationLandscapeLeft) |
        (interfaceOrientation ==UIInterfaceOrientationLandscapeRight);
    }
}

- (BOOL)shouldAutorotate {
    return YES;
}

- (UIInterfaceOrientationMask)supportedInterfaceOrientations
{
    if ([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPad) {
        return UIInterfaceOrientationMaskAll;
    } else if ([[NaverThirdPartyLoginConnection getSharedInstance] isOnlyPortraitSupportedInIphone]){
        return UIInterfaceOrientationMaskPortrait;
    } else {
        return UIInterfaceOrientationMaskAllButUpsideDown;
    }
}


#pragma mark - SampleOAuthViewDelegate

- (void)sendRequestWithUrlString:(NSString *)urlString {
    NSMutableURLRequest *urlRequest = [NSMutableURLRequest requestWithURL:[NSURL URLWithString:urlString]];

    NSString *authValue = [NSString stringWithFormat:@"Bearer %@", _thirdPartyLoginConn.accessToken];

    [urlRequest setValue:authValue forHTTPHeaderField:@"Authorization"];

    [[[NSURLSession sharedSession] dataTaskWithRequest:urlRequest completionHandler:^(NSData * _Nullable data, NSURLResponse * _Nullable response, NSError * _Nullable error) {
        NSString *decodingString = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
        dispatch_async(dispatch_get_main_queue(), ^{
            if (error) {
                NSLog(@"Error happened - %@", [error description]);
               // [_mainView setResultLabelText:[error description]];
            } else {
                NSLog(@"recevied data - %@", decodingString);
                //[_mainView setResultLabelText:decodingString];
            }
        });
    }] resume];
}




#pragma mark - OAuth20 deleagate

- (void)oauth20Connection:(NaverThirdPartyLoginConnection *)oauthConnection didFailWithError:(NSError *)error {
    //    NSLog(@"%s=[%@]", __FUNCTION__, error);
    UnitySendMessage("NaverManager", "OnLoginFaild", (char *)[error.localizedDescription UTF8String]);
    //[_mainView setResultLabelText:[NSString stringWithFormat:@"%@", error]];
}

- (void)oauth20ConnectionDidFinishRequestACTokenWithAuthCode {
        UnitySendMessage("NaverManager", "OnLoginSuccess", (char *) [_thirdPartyLoginConn.accessToken UTF8String]);
        //[_mainView setResultLabelText:[NSString stringWithFormat:@"OAuth Success!\n\nAccess Token - %@\n\nAccess Token Expire Date- %@\n\nRefresh Token - %@", _thirdPartyLoginConn.accessToken, _thirdPartyLoginConn.accessTokenExpireDate, _thirdPartyLoginConn.refreshToken]];
}

- (void)oauth20ConnectionDidFinishRequestACTokenWithRefreshToken {
    //[_mainView setResultLabelText:[NSString stringWithFormat:@"Refresh Success!\n\nAccess Token - %@\n\nAccess sToken ExpireDate- %@", _thirdPartyLoginConn.accessToken, _thirdPartyLoginConn.accessTokenExpireDate ]];
    
}
- (void)oauth20ConnectionDidFinishDeleteToken {
    //[_mainView setResultLabelText:[NSString stringWithFormat:@"인증해제 완료"]];
}

- (void)oauth20Connection:(NaverThirdPartyLoginConnection *)oauthConnection didFinishAuthorizationWithResult:(THIRDPARTYLOGIN_RECEIVE_TYPE)recieveType
{
    NSLog(@"Getting auth code from NaverApp success!");
}

- (void)oauth20Connection:(NaverThirdPartyLoginConnection *)oauthConnection didFailAuthorizationWithRecieveType:(THIRDPARTYLOGIN_RECEIVE_TYPE)recieveType
{
    NSLog(@"NaverApp login fail handler");
}



@end



