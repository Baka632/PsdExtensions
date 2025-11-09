global using static PsdExtensions.CSharp.CommonValues;

using System.Runtime.InteropServices.Marshalling;

namespace PsdExtensions.CSharp;

internal static class CommonValues
{
    internal static readonly StrategyBasedComWrappers DefaultComWrappers = new();

    internal const int E_NOINTERFACE = unchecked((int)0x80004002);
    internal const int CLASS_E_NOAGGREGATION = unchecked((int)0x80040110);
    internal const int CLASS_E_CLASSNOTAVAILABLE = unchecked((int)0x80040111);
    internal const int ERROR_ALREADY_INITIALIZED = unchecked((int)0x800704df);
    internal const int ERROR_NOT_SUPPORTED = unchecked((int)0x80070032);
    internal const int STG_E_ACCESSDENIED = unchecked((int)0x80030005);
    internal const int E_UNEXPECTED = unchecked((int)0x8000FFFF);
    internal const int E_FAIL = unchecked((int)0x80004005);
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int SELFREG_E_CLASS = unchecked((int)0x80040201);

    internal const int S_OK = 0;
    internal const int S_FALSE = 1;
}
