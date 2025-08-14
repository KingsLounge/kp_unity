//
//  SampleOAuthViewController.h
//  NaverOAuthSample
//
//  Created by suejinv on 12. 3. 28..
//  Modified by TY Kim on 14. 8. 20..
//  Copyright 2014 Naver Corp. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <NaverThirdPartyLogin/NaverThirdPartyLogin.h>


@interface SampleOAuthViewController : NSObject <NaverThirdPartyLoginConnectionDelegate>{
    NaverThirdPartyLoginConnection *_thirdPartyLoginConn;

}


- (void)requestThirdpartyLogin : (char*_Nullable) consumerKey : (char*_Nullable) consumerSecret : (char*_Nullable) appName : (char*_Nullable) urlScheme;

+ (nullable SampleOAuthViewController *) sharedInstance;

- (id _Nullable ) init ;

- (void) requestThirdpartyLogin;

- (void) requestDeleteToken;


@end


