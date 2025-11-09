// dllmain.h: 模块类的声明。

class CPsdExtensionsPluginModule : public ATL::CAtlDllModuleT< CPsdExtensionsPluginModule >
{
public :
	DECLARE_LIBID(LIBID_PsdExtensionsPluginLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_PSDEXTENSIONSPLUGIN, "{c23f365b-30df-4dd5-819d-3afc6d1e66ed}")
	HINSTANCE CurrentInstance = NULL;
};

extern class CPsdExtensionsPluginModule _AtlModule;
