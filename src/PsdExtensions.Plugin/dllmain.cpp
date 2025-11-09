// dllmain.cpp: DllMain 的实现。

#include "pch.h"
#include "framework.h"
#include "resource.h"
#include "PsdExtensionsPlugin_i.h"
#include "dllmain.h"

CPsdExtensionsPluginModule _AtlModule;

// DLL 入口点
extern "C" BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	hInstance;
	_AtlModule.CurrentInstance = hInstance;
	return _AtlModule.DllMain(dwReason, lpReserved);
}
