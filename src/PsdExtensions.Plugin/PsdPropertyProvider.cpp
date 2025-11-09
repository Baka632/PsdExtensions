// PsdPropertyProvider.cpp: CPsdPropertyProvider 的实现

#include "pch.h"
#include "PsdPropertyProvider.h"
#include "dllmain.h"
#include <PathCch.h>

constexpr auto EXTENDED_PATH = 32767;

typedef HRESULT(*getPsdProperties)(void*, DWORD, double*, double*, short*);

HINSTANCE CPsdPropertyProvider::psdExtensionsCSharpLibrary = NULL;

const PROPERTYKEY keys[] = {
	PKEY_Image_HorizontalResolution,
	PKEY_Image_VerticalResolution,
	PKEY_Image_ResolutionUnit
};
const PCWSTR CSharpLibraryName = L"PsdExtensions.CSharp.dll";
const PCSTR GetPsdPropertiesFuncName = "GetPsdProperties";

// CPsdPropertyProvider
HRESULT CPsdPropertyProvider::Initialize(IStream* pStream, DWORD grfMode)
{
	HRESULT hr = E_UNEXPECTED;

	if (pStream == NULL)
	{
		return hr;
	}

	double x = 0;
	double y = 0;
	short unit = 0;
	
	TCHAR* currentDllPath = new TCHAR[EXTENDED_PATH];
	if (!GetModuleFileName(_AtlModule.CurrentInstance, currentDllPath, EXTENDED_PATH))
	{
		delete[] currentDllPath;
		return AtlHresultFromWin32(GetLastError());
	}

	hr = PathCchRemoveFileSpec(currentDllPath, EXTENDED_PATH);
	if (FAILED(hr))
	{
		delete[] currentDllPath;
		return hr;
	}
	
	hr = PathCchCombineEx(currentDllPath, EXTENDED_PATH, currentDllPath, CSharpLibraryName, PATHCCH_ALLOW_LONG_PATHS);
	if (FAILED(hr))
	{
		delete[] currentDllPath;
		return hr;
	}

	// NativeAOT 库无法被卸载，所以不需要关心释放问题。
	if (psdExtensionsCSharpLibrary == NULL)
	{
		HINSTANCE handle = LoadLibrary(currentDllPath);
		delete[] currentDllPath;

		if (handle == NULL)
		{
			return AtlHresultFromWin32(GetLastError());
		}

		psdExtensionsCSharpLibrary = handle;
	}

	getPsdProperties GetPsdProperties = (getPsdProperties)GetProcAddress(psdExtensionsCSharpLibrary, GetPsdPropertiesFuncName);

	if (GetPsdProperties == NULL)
	{
		return AtlHresultFromWin32(GetLastError());
	}

	hr = GetPsdProperties(pStream, grfMode, &x, &y, &unit);

	if (SUCCEEDED(hr))
	{
		psdX = x;
		psdY = y;
		psdUnit = unit;
	}
	
	if (pStream != NULL)
	{
		pStream->Release();
	}

	return hr;
}

HRESULT CPsdPropertyProvider::GetAt(DWORD iProp, PROPERTYKEY* pkey)
{
	if (iProp >= ARRAYSIZE(keys))
	{
		return E_INVALIDARG;
	}

	*pkey = keys[iProp];
	return S_OK;
}

HRESULT CPsdPropertyProvider::GetCount(DWORD* cProps)
{
	*cProps = ARRAYSIZE(keys);
	return S_OK;
}

HRESULT CPsdPropertyProvider::GetValue(REFPROPERTYKEY key, PROPVARIANT* pv)
{
	PropVariantInit(pv);

	if (key == PKEY_Image_HorizontalResolution)
	{
		return InitPropVariantFromDouble(psdX, pv);
	}
	else if (key == PKEY_Image_VerticalResolution)
	{
		return InitPropVariantFromDouble(psdY, pv);
	}
	else if (key == PKEY_Image_ResolutionUnit)
	{
		return InitPropVariantFromInt16(psdUnit, pv);
	}
	else
	{
		return S_OK;
	}
}

// 我们不会更改 PSD 中的值。
HRESULT CPsdPropertyProvider::IsPropertyWritable(REFPROPERTYKEY key)
{
	return S_FALSE;
}

HRESULT CPsdPropertyProvider::Commit()
{
	return STG_E_ACCESSDENIED;
}

HRESULT CPsdPropertyProvider::SetValue(REFPROPERTYKEY key, REFPROPVARIANT propVar)
{
	return STG_E_ACCESSDENIED;
}