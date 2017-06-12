#include "SNSModule.h"
#import "Social/Social.h"
#import "Line.h"
#import "MGInstagram.h"
#include <string>

void shareImageToTwiter(const char* message,const char *url, const char* pathImage)
{
    SLComposeViewController  *mySocialComposer;
    UIViewController *myViewController;
    
    myViewController = [UIViewController alloc];
    UIViewController *rootViewController =  [UIApplication sharedApplication].keyWindow.rootViewController;
    [[rootViewController view] addSubview:(myViewController.view)];
    
    mySocialComposer = [SLComposeViewController composeViewControllerForServiceType:SLServiceTypeTwitter];
    
    NSString *customMessage = [NSString stringWithFormat:@"%@ %@", [NSString stringWithUTF8String:message],[NSString stringWithUTF8String:url]
                               ];
    
    [mySocialComposer setInitialText:customMessage];
    
    [mySocialComposer addImage:[UIImage imageWithContentsOfFile:[NSString stringWithUTF8String:pathImage]]];
    
    [myViewController presentViewController:mySocialComposer animated:YES completion:nil];
    
    mySocialComposer.completionHandler = ^(SLComposeViewControllerResult result)
    {
        [myViewController.view setMultipleTouchEnabled:YES];

        if(result==SLComposeViewControllerResultDone)
        {
            SNSModule::OnShared(SNSModule::SNSType::SNS_Twitter, true);
        }
        else
        {
            SNSModule::OnShared(SNSModule::SNSType::SNS_Twitter, false);
        }
        [myViewController dismissViewControllerAnimated:true completion:^{
            [myViewController.view removeFromSuperview];
        }];
    };
}

void shareImageToFacebook(const char *message, const char *pathImage)
{
    SLComposeViewController  *mySocialComposer;
    UIViewController *myViewController;
    
    myViewController = [UIViewController alloc];
    UIViewController *rootViewController =  [UIApplication sharedApplication].keyWindow.rootViewController;
    [[rootViewController view] addSubview:(myViewController.view)];
    
    mySocialComposer = [SLComposeViewController composeViewControllerForServiceType:SLServiceTypeFacebook];
    
    NSString *customMessage = [NSString stringWithUTF8String:message];
    
    [mySocialComposer setInitialText:customMessage];
    
    [mySocialComposer addImage:[UIImage imageWithContentsOfFile:[NSString stringWithUTF8String:pathImage]]];
    
    [myViewController presentViewController:mySocialComposer animated:YES completion:^{
        
    }];
    mySocialComposer.completionHandler = ^(SLComposeViewControllerResult result)
    {
        [myViewController.view setMultipleTouchEnabled:YES];
        if(result==SLComposeViewControllerResultDone)
        {
            SNSModule::OnShared(SNSModule::SNSType::SNS_Facebook,true);
        }
        else
        {
            SNSModule::OnShared(SNSModule::SNSType::SNS_Facebook,false);
        }
        [myViewController dismissViewControllerAnimated:true completion:^{
            [myViewController.view removeFromSuperview];
        }];
    };
}

void shareImageToLine(const char *message, const char *pathImage)
{
    if (![Line isLineInstalled]) {
        [Line openLineInAppStore];
        SNSModule::OnShared(SNSModule::SNSType::SNS_Line,false);
    }
    BOOL ret = [Line shareImage:[UIImage imageWithContentsOfFile:[NSString stringWithUTF8String:pathImage]]];
   SNSModule::OnShared(SNSModule::SNSType::SNS_Line,ret);
}

void shareImageToInstagram(const char *message, const char* pathImage) {
    if ([MGInstagram isAppInstalled]) {
        UIViewController *rootViewController =  [UIApplication sharedApplication].keyWindow.rootViewController;
        NSString *caption = [NSString stringWithUTF8String:message];
        UIImage *image = [UIImage imageWithContentsOfFile:[NSString stringWithUTF8String:pathImage]];
        
        MGInstagram *instagram = [[MGInstagram alloc] init];
        [instagram postImage:image withCaption:caption inView:rootViewController.view];
	SNSModule::OnShared(SNSModule::SNSType::SNS_Instagram,true);
    } else {
       SNSModule::OnShared(SNSModule::SNSType::SNS_Instagram,false);
    }
}


void SNSModule::Share(SNSType snsType, const char* message,const char* url, const char* pathImage)
{
    switch (snsType) {
        case SNSType::SNS_Facebook:
        {
            shareImageToFacebook((std::string(message)+url).c_str(), pathImage);
        }
            break;
        case SNSType::SNS_Twitter:
        {
            shareImageToTwiter(message,url,pathImage);
        }
            break;
        case SNSType::SNS_Line:
        {
            shareImageToLine((std::string(message)+url).c_str(),pathImage);
        }
            break;
        case SNSType::SNS_Instagram:
        {
            shareImageToInstagram((std::string(message)+url).c_str(),pathImage);
        }
            break;
        default:
            break;
    }
}

//NATIVE FOR UNITY
extern "C" void UnitySendMessage(const char*, const char*, const char*);

extern "C" {
    void SNSModule_Share(int snsType,const char* message, const char* url,const char* pathImage)
    {
        SNSModule::Share((SNSModule::SNSType)snsType,message,url,pathImage);
    }
}
void SNSModule::OnShared(int snsType, bool isSuccess)
{
    char result[100];
    sprintf(result,"%d,%s",snsType,isSuccess?"true":"false");
    UnitySendMessage("SNSListener","OnShared", result);
}