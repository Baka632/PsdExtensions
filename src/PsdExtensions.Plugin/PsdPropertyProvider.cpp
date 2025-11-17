// PsdPropertyProvider.cpp: CPsdPropertyProvider 的实现

#include "pch.h"
#include "PsdPropertyProvider.h"
#include "dllmain.h"
#include <PathCch.h>

constexpr auto EXTENDED_PATH = 32767;

typedef HRESULT(*getPsdProperties)(const void*, DWORD, double*, double*, short*, short*);

HINSTANCE CPsdPropertyProvider::psdExtensionsCSharpLibrary = NULL;

const PROPERTYKEY keys[] = {
	PKEY_Image_HorizontalResolution,
	PKEY_Image_VerticalResolution,
	PKEY_Image_ResolutionUnit,
	PKEY_Document_PageCount
};
const PCWSTR CSharpLibraryName = L"PsdExtensions.CSharp.dll";
const PCSTR GetPsdPropertiesFuncName = "GetPsdProperties";

// CPsdPropertyProvider
HRESULT STDMETHODCALLTYPE CPsdPropertyProvider::Initialize(LPCWSTR pszFilePath, DWORD grfMode)
{
	HRESULT hr = E_UNEXPECTED;

	if (pszFilePath == NULL)
	{
		return hr;
	}

	WCHAR* currentDllPath = new WCHAR[EXTENDED_PATH];
	if (!GetModuleFileNameW(_AtlModule.CurrentInstance, currentDllPath, EXTENDED_PATH))
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
		HINSTANCE handle = LoadLibraryW(currentDllPath);
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

	double x = 0;
	double y = 0;
	short unit = 0;
	short layerCount = 0;

	hr = GetPsdProperties(pszFilePath, grfMode, &x, &y, &unit, &layerCount);

	if (SUCCEEDED(hr))
	{
		psdX = x;
		psdY = y;
		psdUnit = unit;
        psdLayerCount = layerCount;
	}

	return hr;
}

HRESULT STDMETHODCALLTYPE CPsdPropertyProvider::GetAt(DWORD iProp, PROPERTYKEY* pkey)
{
	if (iProp >= ARRAYSIZE(keys))
	{
		return E_INVALIDARG;
	}

	*pkey = keys[iProp];
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CPsdPropertyProvider::GetCount(DWORD* cProps)
{
	*cProps = ARRAYSIZE(keys);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CPsdPropertyProvider::GetValue(REFPROPERTYKEY key, PROPVARIANT* pv)
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
	else if (key == PKEY_Document_PageCount)
	{
		return InitPropVariantFromInt32((LONG)psdLayerCount, pv);
	}
	else
	{
		return S_OK;
	}
}

// 我们不会更改 PSD 中的值。
HRESULT STDMETHODCALLTYPE CPsdPropertyProvider::IsPropertyWritable(REFPROPERTYKEY key)
{
	return S_FALSE;
}

HRESULT STDMETHODCALLTYPE CPsdPropertyProvider::Commit()
{
	return STG_E_ACCESSDENIED;
}

HRESULT STDMETHODCALLTYPE CPsdPropertyProvider::SetValue(REFPROPERTYKEY key, REFPROPVARIANT propVar)
{
	return STG_E_ACCESSDENIED;
}