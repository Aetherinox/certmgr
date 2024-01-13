﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace MSBuildCode
{
    /// <summary>
    /// Adjusted MSBuild source from https://github.com/microsoft/msbuild/blob/e70a3159d64f9ed6ec3b60253ef863fa883a99b1/src/Shared/StrongNameHelpers.cs
    /// </summary>
    internal static class StrongNameHelpers
    {
        [ThreadStatic]
        private static int t_ts_LastStrongNameHR;

        [SecurityCritical]
        [ThreadStatic]
        private static MSBuildCode.IClrStrongName s_StrongName;

        [ThreadStatic]
        private static MethodInfo s_GetRuntimeInterfaceAsObjectMethod;

        private static MSBuildCode.IClrStrongName StrongName
        {
            [SecurityCritical]
            get
            {
                if (s_StrongName == null)
                {
                    if (s_GetRuntimeInterfaceAsObjectMethod == null)
                    {
                        s_GetRuntimeInterfaceAsObjectMethod = typeof(RuntimeEnvironment).GetMethod("GetRuntimeInterfaceAsObject");
                    }
                    if (s_GetRuntimeInterfaceAsObjectMethod != null)
                    {
                        s_StrongName = (MSBuildCode.IClrStrongName)s_GetRuntimeInterfaceAsObjectMethod.Invoke(null, new object[2]
                        {
                        new Guid("B79B0ACD-F5CD-409b-B5A5-A16244610B92"),
                        new Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D")
                        });
                    }
                }
                return s_StrongName;
            }
        }

        private static MSBuildCode.IClrStrongNameUsingIntPtr StrongNameUsingIntPtr
        {
            [SecurityCritical]
            get
            {
                return (MSBuildCode.IClrStrongNameUsingIntPtr)StrongName;
            }
        }

        [SecurityCritical]
        public static int StrongNameErrorInfo()
        {
            return t_ts_LastStrongNameHR;
        }

        [SecurityCritical]
        public static void StrongNameFreeBuffer(IntPtr pbMemory)
        {
            StrongNameUsingIntPtr.StrongNameFreeBuffer(pbMemory);
        }

        [SecurityCritical]
        public static bool StrongNameGetPublicKey(string pwzKeyContainer, IntPtr pbKeyBlob, int cbKeyBlob, out IntPtr ppbPublicKeyBlob, out int pcbPublicKeyBlob)
        {
            int num = StrongNameUsingIntPtr.StrongNameGetPublicKey(pwzKeyContainer, pbKeyBlob, cbKeyBlob, out ppbPublicKeyBlob, out pcbPublicKeyBlob);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                ppbPublicKeyBlob = IntPtr.Zero;
                pcbPublicKeyBlob = 0;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameKeyDelete(string pwzKeyContainer)
        {
            int num = StrongName.StrongNameKeyDelete(pwzKeyContainer);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameKeyGen(string pwzKeyContainer, int dwFlags, out IntPtr ppbKeyBlob, out int pcbKeyBlob)
        {
            int num = StrongName.StrongNameKeyGen(pwzKeyContainer, dwFlags, out ppbKeyBlob, out pcbKeyBlob);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                ppbKeyBlob = IntPtr.Zero;
                pcbKeyBlob = 0;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameKeyInstall(string pwzKeyContainer, IntPtr pbKeyBlob, int cbKeyBlob)
        {
            int num = StrongNameUsingIntPtr.StrongNameKeyInstall(pwzKeyContainer, pbKeyBlob, cbKeyBlob);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameSignatureGeneration(string pwzFilePath, string pwzKeyContainer, IntPtr pbKeyBlob, int cbKeyBlob)
        {
            IntPtr ppbSignatureBlob = IntPtr.Zero;
            int pcbSignatureBlob = 0;
            return StrongNameSignatureGeneration(pwzFilePath, pwzKeyContainer, pbKeyBlob, cbKeyBlob, ref ppbSignatureBlob, out pcbSignatureBlob);
        }

        [SecurityCritical]
        public static bool StrongNameSignatureGeneration(string pwzFilePath, string pwzKeyContainer, IntPtr pbKeyBlob, int cbKeyBlob, ref IntPtr ppbSignatureBlob, out int pcbSignatureBlob)
        {
            int num = StrongNameUsingIntPtr.StrongNameSignatureGeneration(pwzFilePath, pwzKeyContainer, pbKeyBlob, cbKeyBlob, ppbSignatureBlob, out pcbSignatureBlob);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                pcbSignatureBlob = 0;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameSignatureSize(IntPtr pbPublicKeyBlob, int cbPublicKeyBlob, out int pcbSize)
        {
            int num = StrongNameUsingIntPtr.StrongNameSignatureSize(pbPublicKeyBlob, cbPublicKeyBlob, out pcbSize);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                pcbSize = 0;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameSignatureVerification(string pwzFilePath, int dwInFlags, out int pdwOutFlags)
        {
            int num = StrongName.StrongNameSignatureVerification(pwzFilePath, dwInFlags, out pdwOutFlags);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                pdwOutFlags = 0;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameSignatureVerificationEx(string pwzFilePath, bool fForceVerification, out bool pfWasVerified)
        {
            int num = StrongName.StrongNameSignatureVerificationEx(pwzFilePath, fForceVerification, out pfWasVerified);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                pfWasVerified = false;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameTokenFromPublicKey(IntPtr pbPublicKeyBlob, int cbPublicKeyBlob, out IntPtr ppbStrongNameToken, out int pcbStrongNameToken)
        {
            int num = StrongNameUsingIntPtr.StrongNameTokenFromPublicKey(pbPublicKeyBlob, cbPublicKeyBlob, out ppbStrongNameToken, out pcbStrongNameToken);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                ppbStrongNameToken = IntPtr.Zero;
                pcbStrongNameToken = 0;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameSignatureSize(byte[] bPublicKeyBlob, int cbPublicKeyBlob, out int pcbSize)
        {
            int num = StrongName.StrongNameSignatureSize(bPublicKeyBlob, cbPublicKeyBlob, out pcbSize);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                pcbSize = 0;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameTokenFromPublicKey(byte[] bPublicKeyBlob, int cbPublicKeyBlob, out IntPtr ppbStrongNameToken, out int pcbStrongNameToken)
        {
            int num = StrongName.StrongNameTokenFromPublicKey(bPublicKeyBlob, cbPublicKeyBlob, out ppbStrongNameToken, out pcbStrongNameToken);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                ppbStrongNameToken = IntPtr.Zero;
                pcbStrongNameToken = 0;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameGetPublicKey(string pwzKeyContainer, byte[] bKeyBlob, int cbKeyBlob, out IntPtr ppbPublicKeyBlob, out int pcbPublicKeyBlob)
        {
            int num = StrongName.StrongNameGetPublicKey(pwzKeyContainer, bKeyBlob, cbKeyBlob, out ppbPublicKeyBlob, out pcbPublicKeyBlob);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                ppbPublicKeyBlob = IntPtr.Zero;
                pcbPublicKeyBlob = 0;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameKeyInstall(string pwzKeyContainer, byte[] bKeyBlob, int cbKeyBlob)
        {
            int num = StrongName.StrongNameKeyInstall(pwzKeyContainer, bKeyBlob, cbKeyBlob);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                return false;
            }
            return true;
        }

        [SecurityCritical]
        public static bool StrongNameSignatureGeneration(string pwzFilePath, string pwzKeyContainer, byte[] bKeyBlob, int cbKeyBlob)
        {
            IntPtr ppbSignatureBlob = IntPtr.Zero;
            int pcbSignatureBlob = 0;
            return StrongNameSignatureGeneration(pwzFilePath, pwzKeyContainer, bKeyBlob, cbKeyBlob, ref ppbSignatureBlob, out pcbSignatureBlob);
        }

        [SecurityCritical]
        public static bool StrongNameSignatureGeneration(string pwzFilePath, string pwzKeyContainer, byte[] bKeyBlob, int cbKeyBlob, ref IntPtr ppbSignatureBlob, out int pcbSignatureBlob)
        {
            int num = StrongName.StrongNameSignatureGeneration(pwzFilePath, pwzKeyContainer, bKeyBlob, cbKeyBlob, ppbSignatureBlob, out pcbSignatureBlob);
            if (num < 0)
            {
                t_ts_LastStrongNameHR = num;
                pcbSignatureBlob = 0;
                return false;
            }
            return true;
        }
    }
}