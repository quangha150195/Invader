#include "BaseUtils.h"

@interface SavePhoToDelegate : NSObject
{
    
}
@end

@implementation SavePhoToDelegate
- (void)imageSavedToPhotosAlbum:(UIImage *)image didFinishSavingWithError:(NSError *)error contextInfo:(void *)contextInfo {
    if (!error)
    {
        BaseUtils::sendMessage("BaseUtils.savePhotoToAlbum,ok");
    }
    else
    {
    }
}
@end

UIView * BaseUtils::getRootView()
{
    return UnityGetGLView();
}
static float sDensity=0;
float BaseUtils::getDeviceDensity()
{
    if(sDensity==0)
    {
         sDensity=[[UIScreen mainScreen] nativeScale];
    }
    return sDensity;
}
static float sScale=0;
float BaseUtils::getDeviceScale()
{
    if(sScale==0)
    {
        sScale=[[UIScreen mainScreen] nativeScale];
    }
    return sScale;
}

const char* BaseUtils::getAppVersion()
{
    NSString *appVersion = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleShortVersionString"];
    return [appVersion UTF8String];
}
static SavePhoToDelegate *savePhotoDelegate=nil;
void BaseUtils::savePhotoToAlbum(const char* imagePath)
{
	UIImage *image = [UIImage imageWithContentsOfFile:[NSString stringWithUTF8String:imagePath]];
    if(image!=nil)
    {
        if(savePhotoDelegate==nil)
        {
            savePhotoDelegate=[SavePhoToDelegate alloc];
        }
        UIImageWriteToSavedPhotosAlbum(image, savePhotoDelegate, @selector(imageSavedToPhotosAlbum: didFinishSavingWithError: contextInfo:),nil);
    }
}
void BaseUtils::sendMessage(const char* message)
{
    char *res=(char*)malloc(strlen(message)+1);
    strcpy(res,message);
	UnitySendMessage("[NativeListener]","receiveMessage",res);
}

void BaseUtils::setStatusBarHidden(bool isHide)
{
    [[UIApplication sharedApplication] setStatusBarHidden: isHide
                                            withAnimation: UIStatusBarAnimationSlide];
    [UnityGetGLViewController() setNeedsStatusBarAppearanceUpdate];
}

static UIWindow *coverWindow = nil;

void BaseUtils::showUIView(int r, int g, int b, int a)
{
    if (coverWindow == nil) {
        coverWindow = [[UIWindow alloc] initWithFrame:[UIScreen mainScreen].bounds];
        coverWindow.windowLevel = UIWindowLevelAlert;
    }else {
        [coverWindow setHidden:NO];
    }
    
    coverWindow.backgroundColor = [UIColor colorWithRed:r green:g blue:b alpha:a];
    [coverWindow makeKeyAndVisible];
}

void BaseUtils::hideUIView()
{
    if (coverWindow != nil) {
        [coverWindow setHidden:YES];
        [UnityGetMainWindow() makeKeyAndVisible];
    }
}
//NATIVE FOR UNITY
extern "C" void UnitySendMessage(const char*, const char*, const char*);

extern "C" {
    char *CreateStringPointer(const char* text)
    {
        char *res=(char*)malloc(strlen(text)+1);
        strcpy(res,text);
        return res;
    }
    float BaseUtils_getDeviceDensity()
    {
        return BaseUtils::getDeviceDensity();
    }
    char* BaseUtils_getAppVersion()
    {
        return CreateStringPointer(BaseUtils::getAppVersion());
    }
    void BaseUtils_savePhotoToAlbum(const char* imagePath)
    {
        BaseUtils::savePhotoToAlbum(imagePath);
    }
    void BaseUtils_setStatusBarHidden(bool isHide)
    {
        BaseUtils::setStatusBarHidden(isHide);
    }
    
    void BaseUtils_showUIView(int r, int g, int b, int a)
    {
        BaseUtils::showUIView(r, g, b, a);
    }
    
    void BaseUtils_hideUIView()
    {
        BaseUtils::hideUIView();
    }
}
