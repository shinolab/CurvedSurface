// This file is autogenerated
using System;
using System.Runtime.InteropServices;

#if UNITY_2020_2_OR_NEWER
#nullable enable
#endif

namespace AUTD3Sharp
{
    namespace NativeMethods
    {
        internal static class LinkTwinCAT
        {
            private const string DLL = "autd3capi_link_twincat";

            [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] public static extern LinkPtr AUTDLinkTwinCAT(byte[] err);

            [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] public static extern LinkPtr AUTDLinkTwinCATTimeout(LinkPtr twincat, ulong timeoutNs);

            [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] public static extern LinkPtr AUTDLinkRemoteTwinCAT(string serverAmsNetId, byte[] err);

            [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] public static extern LinkPtr AUTDLinkRemoteTwinCATServerIP(LinkPtr twincat, string addr);

            [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] public static extern LinkPtr AUTDLinkRemoteTwinCATClientAmsNetId(LinkPtr twincat, string id);

            [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)] public static extern LinkPtr AUTDLinkRemoteTwinCATTimeout(LinkPtr twincat, ulong timeoutNs);
        }
    }

}

#if UNITY_2020_2_OR_NEWER
#nullable disable
#endif

