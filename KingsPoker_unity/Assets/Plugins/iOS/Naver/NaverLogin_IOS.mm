#import <NaverThirdPartyLogin/NaverThirdPartyLogin.h>
#import "SampleOAuthViewController.h"



extern "C" void NaverInit()
{
    [[SampleOAuthViewController sharedInstance] init];
    
}

extern "C" void RequestThirdPartyLoginWithDatas(char*  key  , char * secret, char * url, char * name)
{
    [[SampleOAuthViewController sharedInstance] requestThirdpartyLogin : key : secret : url : name];
}

extern "C" void RequestThirdPartyLogin()
{
    [[SampleOAuthViewController sharedInstance] requestThirdpartyLogin];
}

extern "C" void ReQuestDeleteToken()
{
    [[SampleOAuthViewController sharedInstance]  requestDeleteToken];
}
