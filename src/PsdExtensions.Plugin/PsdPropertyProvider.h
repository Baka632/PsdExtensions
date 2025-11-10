// PsdPropertyProvider.h: CPsdPropertyProvider 的声明

#pragma once
#include "resource.h"       // 主符号

#include <propsys.h>     // Property System APIs and interfaces
#include <propkey.h>     // System PROPERTYKEY definitions
#include <propvarutil.h> // PROPVARIANT and VARIANT helper APIs

#include "PsdExtensionsPlugin_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Windows CE 平台(如不提供完全 DCOM 支持的 Windows Mobile 平台)上无法正确支持单线程 COM 对象。定义 _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA 可强制 ATL 支持创建单线程 COM 对象实现并允许使用其单线程 COM 对象实现。rgs 文件中的线程模型已被设置为“Free”，原因是该模型是非 DCOM Windows CE 平台支持的唯一线程模型。"
#endif

using namespace ATL;


// CPsdPropertyProvider

class ATL_NO_VTABLE CPsdPropertyProvider :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CPsdPropertyProvider, &CLSID_PsdPropertyProvider>,
	public IDispatchImpl<IPsdPropertyProvider, &IID_IPsdPropertyProvider, &LIBID_PsdExtensionsPluginLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IInitializeWithFile,
	public IPropertyStore,
	public IPropertyStoreCapabilities
{
public:
	CPsdPropertyProvider()
	{
	}

DECLARE_REGISTRY_RESOURCEID(106)

BEGIN_COM_MAP(CPsdPropertyProvider)
	COM_INTERFACE_ENTRY(IPsdPropertyProvider)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(IInitializeWithFile)
	COM_INTERFACE_ENTRY(IPropertyStore)
	COM_INTERFACE_ENTRY(IPropertyStoreCapabilities)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:
	HRESULT STDMETHODCALLTYPE Initialize(LPCWSTR pszFilePath, DWORD grfMode);
	HRESULT STDMETHODCALLTYPE IsPropertyWritable(REFPROPERTYKEY key);
	HRESULT STDMETHODCALLTYPE Commit();
	HRESULT STDMETHODCALLTYPE GetAt(DWORD iProp, PROPERTYKEY* pkey);
	HRESULT STDMETHODCALLTYPE GetCount(DWORD* cProps);
	HRESULT STDMETHODCALLTYPE GetValue(REFPROPERTYKEY key, PROPVARIANT* pv);
	HRESULT STDMETHODCALLTYPE SetValue(REFPROPERTYKEY key, REFPROPVARIANT propVar);

private:
	double psdX = 0;
	double psdY = 0;
	short psdUnit = 0;
	static HINSTANCE psdExtensionsCSharpLibrary;
};

OBJECT_ENTRY_AUTO(__uuidof(PsdPropertyProvider), CPsdPropertyProvider)
