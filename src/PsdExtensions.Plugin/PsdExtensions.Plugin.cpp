// PsdExtensions.Plugin.cpp: DLL 导出的实现。

#include "pch.h"
#include "framework.h"
#include "resource.h"
#include "PsdExtensionsPlugin_i.h"
#include "dllmain.h"
#include <Windows.h>
#include <atlcomcli.h>

using namespace ATL;

// 用于确定 DLL 是否可由 OLE 卸载。
_Use_decl_annotations_
STDAPI DllCanUnloadNow(void)
{
	return _AtlModule.DllCanUnloadNow();
}

// 返回一个类工厂以创建所请求类型的对象。
_Use_decl_annotations_
STDAPI DllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _Outptr_ LPVOID* ppv)
{
	return _AtlModule.DllGetClassObject(rclsid, riid, ppv);
}

PCWSTR PsdFullDetails = L"prop:System.PropGroup.Image;*System.Image.HorizontalResolution;*System.Image.VerticalResolution;System.PropGroup.FileSystem;System.ItemNameDisplay;System.ItemTypeText;System.ItemFolderPathDisplay;System.Size;System.DateCreated;System.DateModified;System.FileAttributes;*System.StorageProviderState;*System.OfflineAvailability;*System.OfflineStatus;*System.SharedWith;*System.FileOwner;*System.ComputerName";
PCWSTR PsdInfoTip = L"prop:System.ItemTypeText;System.Size;System.DateModified;*System.Image.HorizontalResolution;*System.Image.VerticalResolution";
PCWSTR PsdPreviewDetails = L"prop:System.DateModified;System.Size;System.DateCreated;*System.Image.HorizontalResolution;*System.Image.VerticalResolution;*System.StorageProviderState;*System.OfflineAvailability;*System.OfflineStatus;*System.SharedWith";
PCWSTR PsdContentViewModeForBrowse = L"prop:~System.ItemNameDisplay;*System.Image.HorizontalResolution;~System.LayoutPattern.PlaceHolder;*System.Image.VerticalResolution;System.DateModified;System.Size";
PCWSTR PsdContentViewModeForSearch = L"prop:~System.ItemNameDisplay;~System.ItemFolderPathDisplay;*System.Image.HorizontalResolution;*System.Image.VerticalResolution;System.DateModified;System.Size";
PCWSTR PsdContentViewModeLayoutPattern = L"delta";

static DWORD CalcPCWSTRBytes(PCWSTR str)
{
	if (str == nullptr)
	{
		return 0;
	}
	return (lstrlenW(str) + 1) * sizeof(WCHAR);
}

static HRESULT SetPsdPropertyListValue(HKEY psdProgIDKey, PCWSTR type, PCWSTR str)
{
	return AtlHresultFromWin32(RegSetValueExW(psdProgIDKey, type, 0, REG_SZ, (LPBYTE)str, CalcPCWSTRBytes(str)));
}

static HRESULT RegisterPropertyList()
{
	HRESULT hr = E_FAIL;

	HKEY psdKey = {};
	hr = AtlHresultFromWin32(RegOpenKeyExW(HKEY_CLASSES_ROOT, L".psd", 0, KEY_READ, &psdKey));
	if (SUCCEEDED(hr))
	{
		DWORD bufBytes = 0;
		hr = AtlHresultFromWin32(RegGetValueW(psdKey, NULL, NULL, RRF_RT_REG_SZ, NULL, NULL, &bufBytes));
		if (SUCCEEDED(hr))
		{
			WCHAR* currentPsdProgID = new WCHAR[(bufBytes / sizeof(WCHAR)) + 1];
			hr = AtlHresultFromWin32(RegGetValueW(psdKey, NULL, NULL, RRF_RT_REG_SZ, NULL, currentPsdProgID, &bufBytes));
			if (SUCCEEDED(hr))
			{
				HKEY psdProgIDKey = {};
				hr = AtlHresultFromWin32(RegOpenKeyExW(HKEY_CLASSES_ROOT, currentPsdProgID, 0, KEY_SET_VALUE, &psdProgIDKey));
				if (SUCCEEDED(hr))
				{
					hr = SetPsdPropertyListValue(psdProgIDKey, L"FullDetails", PsdFullDetails);
					hr = SetPsdPropertyListValue(psdProgIDKey, L"InfoTip", PsdInfoTip);
					hr = SetPsdPropertyListValue(psdProgIDKey, L"PreviewDetails", PsdPreviewDetails);
					hr = SetPsdPropertyListValue(psdProgIDKey, L"ContentViewModeForBrowse", PsdContentViewModeForBrowse);
					hr = SetPsdPropertyListValue(psdProgIDKey, L"ContentViewModeForSearch", PsdContentViewModeForSearch);
					hr = SetPsdPropertyListValue(psdProgIDKey, L"ContentViewModeLayoutPatternForBrowse", PsdContentViewModeLayoutPattern);
					hr = SetPsdPropertyListValue(psdProgIDKey, L"ContentViewModeLayoutPatternForSearch", PsdContentViewModeLayoutPattern);

					RegCloseKey(psdProgIDKey);
				}
			}

			delete[] currentPsdProgID;
		}

		RegCloseKey(psdKey);
	}

	return hr;
}

static HRESULT UnregisterPropertyList()
{
	HRESULT hr = E_FAIL;

	HKEY psdKey = {};
	hr = AtlHresultFromWin32(RegOpenKeyExW(HKEY_CLASSES_ROOT, L".psd", 0, KEY_READ, &psdKey));
	if (SUCCEEDED(hr))
	{
		DWORD bufBytes = 0;
		hr = AtlHresultFromWin32(RegGetValueW(psdKey, NULL, NULL, RRF_RT_REG_SZ, NULL, NULL, &bufBytes));
		if (SUCCEEDED(hr))
		{
			WCHAR* currentPsdProgID = new WCHAR[(bufBytes / sizeof(WCHAR)) + 1];
			hr = AtlHresultFromWin32(RegGetValueW(psdKey, NULL, NULL, RRF_RT_REG_SZ, NULL, currentPsdProgID, &bufBytes));
			if (SUCCEEDED(hr))
			{
				HKEY psdProgIDKey = {};
				hr = AtlHresultFromWin32(RegOpenKeyExW(HKEY_CLASSES_ROOT, currentPsdProgID, 0, KEY_SET_VALUE, &psdProgIDKey));
				if (SUCCEEDED(hr))
				{
					hr = AtlHresultFromWin32(RegDeleteValueW(psdProgIDKey, L"FullDetails"));
					hr = AtlHresultFromWin32(RegDeleteValueW(psdProgIDKey, L"InfoTip"));
					hr = AtlHresultFromWin32(RegDeleteValueW(psdProgIDKey, L"PreviewDetails"));
					hr = AtlHresultFromWin32(RegDeleteValueW(psdProgIDKey, L"ContentViewModeForBrowse"));
					hr = AtlHresultFromWin32(RegDeleteValueW(psdProgIDKey, L"ContentViewModeForSearch"));
					hr = AtlHresultFromWin32(RegDeleteValueW(psdProgIDKey, L"ContentViewModeLayoutPatternForBrowse"));
					hr = AtlHresultFromWin32(RegDeleteValueW(psdProgIDKey, L"ContentViewModeLayoutPatternForSearch"));

					RegCloseKey(psdProgIDKey);
				}
			}

			delete[] currentPsdProgID;
		}

		RegCloseKey(psdKey);
	}

	return hr;
}

// DllRegisterServer - 向系统注册表中添加项。
STDAPI DllRegisterServer(void)
{
	// 注册对象、类型库和类型库中的所有接口
	HRESULT hr = _AtlModule.DllRegisterServer();
	RegisterPropertyList();
	return hr;
}

// DllUnregisterServer - 移除系统注册表中的项。
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = _AtlModule.DllUnregisterServer();
	UnregisterPropertyList();
	return hr;
}

// DllInstall - 按用户和计算机在系统注册表中逐一添加/移除项。
STDAPI DllInstall(BOOL bInstall, _In_opt_  LPCWSTR pszCmdLine)
{
	HRESULT hr = E_FAIL;
	static const wchar_t szUserSwitch[] = L"user";

	if (pszCmdLine != nullptr)
	{
		if (_wcsnicmp(pszCmdLine, szUserSwitch, _countof(szUserSwitch)) == 0)
		{
			ATL::AtlSetPerUserRegistration(true);
		}
	}

	if (bInstall)
	{
		hr = DllRegisterServer();
		if (FAILED(hr))
		{
			DllUnregisterServer();
		}
	}
	else
	{
		hr = DllUnregisterServer();
	}

	return hr;
}


