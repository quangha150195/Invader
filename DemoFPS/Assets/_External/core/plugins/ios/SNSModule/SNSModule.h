
#ifndef __SNS_MODULE_H_
#define __SNS_MODULE_H_
#include <stdio.h>

class SNSModule
{
public:
	enum SNSType{
		SNS_Facebook = 0,
		SNS_Twitter=1,
        SNS_Line=2,
        SNS_Instagram=3
	};
public:
	static void OnShared(int snsType, bool isSuccess);
	static void Share(SNSType snsType, const char* message, const char* url, const char* pathImage);
};

#endif /* __SNS_MODULE_H_ */
