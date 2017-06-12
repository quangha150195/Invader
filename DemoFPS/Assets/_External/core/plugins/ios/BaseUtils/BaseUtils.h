#ifndef _OT_PLUGIN_BASEUTILS_
#define _OT_PLUGIN_BASEUTILS_
#include <stdio.h>
#import <UIKit/UIKit.h>
class BaseUtils
{
public:
    static UIView *getRootView();
	static float getDeviceDensity();
	static float getDeviceScale();
    static const char* getAppVersion();
	
	//FILE
	static void savePhotoToAlbum(const char* imagePath);

	static void sendMessage(const char* message);
    
    static void setStatusBarHidden(bool isHide);
    
    static void showUIView(int r, int g, int b, int a);
    static void hideUIView();
};
#endif
