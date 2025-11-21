#pragma once

#include "pch.h"
#include "dllmain.h"
#include <PathCch.h>
#include <strsafe.h>
#include <Windows.h>
#include <atlcomcli.h>

using namespace ATL;

constexpr auto EXTENDED_PATH = 32767;

static HRESULT GetCurrentModulePath(LPWSTR result, size_t length)
{
	WCHAR* currentDllPath = new WCHAR[EXTENDED_PATH];
	if (!GetModuleFileNameW(_AtlModule.CurrentInstance, currentDllPath, EXTENDED_PATH))
	{
		delete[] currentDllPath;
		return AtlHresultFromWin32(GetLastError());
	}

	HRESULT hr = S_OK;

	hr = PathCchRemoveFileSpec(currentDllPath, EXTENDED_PATH);
	if (FAILED(hr))
	{
		delete[] currentDllPath;
		return hr;
	}

	hr = StringCchCopyW(result, length, currentDllPath);
	delete[] currentDllPath;
	return hr;
}

