
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

#pragma warning disable 414

namespace Gibraltar
{
    /// <summary>
    /// Provides enhanced information about the operating system by using native Win32 methods.
    /// </summary>
    public class OSVersionInfo
    {
        private bool m_NamesCalculated;
        private string m_OSFamilyName;
        private string m_OSEditionName;
        private Version m_Version;

        /// <summary>
        /// Interop class for structure OSVersionInfo
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class OSVERSIONINFO
        {
            public int OSVersionInfoSize;
            public int MajorVersion;
            public int MinorVersion;
            public int BuildNumber;
            public int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
            public string CSDVersion;

            public OSVERSIONINFO()
            {
                OSVersionInfoSize = Marshal.SizeOf(this);
            }

            private void StopTheCompilerComplaining()
            {
                MajorVersion = 0;
                MinorVersion = 0;
                BuildNumber = 0;
                PlatformId = 0;
                CSDVersion = String.Empty;
            }
        }

        /// <summary>
        /// Interop class for structure OSVersionInfoEx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class OSVERSIONINFOEX
        {
            public int OSVersionInfoSize;
            public int MajorVersion;
            public int MinorVersion;
            public int BuildNumber;
            public int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
            public string CSDVersion;
            public Int16 ServicePackMajor;
            public Int16 ServicePackMinor;
            public UInt16 SuiteMask;
            public byte ProductType;
            public byte Reserved;

            public OSVERSIONINFOEX()
            {
                OSVersionInfoSize = Marshal.SizeOf(this);
            }

            private void StopTheCompilerComplaining()
            {
                MajorVersion = 0;
                MinorVersion = 0;
                BuildNumber = 0;
                PlatformId = 0;
                CSDVersion = String.Empty;
                ServicePackMajor = 0;
                ServicePackMinor = 0;
                SuiteMask = 0;
                ProductType = 0;
                Reserved = 0;
            }
        }

        private class NativeMethods
        {
            private NativeMethods() { }

            [SecurityCritical]
            [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern uint RtlGetVersion([In, Out] OSVERSIONINFOEX versionInfo);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern bool GetVersionEx( [In, Out] OSVERSIONINFO osVersionInfo );

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern bool GetVersionEx( [In, Out] OSVERSIONINFOEX osVersionInfoEx );

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern bool GetProductInfo([In] Int32 OsMajorVersion, [In] Int32 OsMinorVersion, [In] Int32 SpMajorVersion, [In] Int32 SpMinorVersion, [Out] out Int32 ProductType);
        }

        private class VerPlatformId
        {
// ReSharper disable InconsistentNaming
            public const Int32 Win32s = 0;
            public const Int32 Win32Windows = 1;
            public const Int32 Win32NT = 2;
            public const Int32 WinCE = 3;
            public const Int32 Unix = 4; // Unix platform (eg. running under Mono). (Mono 2.2 reports 4 for MaxOS X also)
            public const Int32 MaxOSX = 6; // MaxOS X platform (eg. running under Mono).
            public const Int32 UnixMono1 = 128; // Mono 1.0 & 1.0 report 128 for Unix. (We don't officially support that version.)
// ReSharper restore InconsistentNaming

            private VerPlatformId() { }
        }

        /// <summary>
        /// Suite mask for all versions.
        /// </summary>
        private class VerSuiteMask
        {
// ReSharper disable InconsistentNaming
            public const UInt32 VER_SERVER_NT = 0x80000000;
            public const UInt32 VER_WORKSTATION_NT = 0x40000000;

            public const UInt16 VER_SUITE_SMALLBUSINESS = 0x00000001;
            public const UInt16 VER_SUITE_ENTERPRISE = 0x00000002;
            public const UInt16 VER_SUITE_BACKOFFICE = 0x00000004;
            public const UInt16 VER_SUITE_COMMUNICATIONS = 0x00000008;
            public const UInt16 VER_SUITE_TERMINAL = 0x00000010;
            public const UInt16 VER_SUITE_SMALLBUSINESS_RESTRICTED = 0x00000020;
            public const UInt16 VER_SUITE_EMBEDDEDNT = 0x00000040;
            public const UInt16 VER_SUITE_DATACENTER = 0x00000080;
            public const UInt16 VER_SUITE_SINGLEUSERTS = 0x00000100;
            public const UInt16 VER_SUITE_PERSONAL = 0x00000200;
            public const UInt16 VER_SUITE_BLADE = 0x00000400;
            public const UInt16 VER_SUITE_EMBEDDED_RESTRICTED = 0x00000800;
            public const UInt16 VER_SUITE_WH_SERVER =  0x00008000; 
// ReSharper restore InconsistentNaming

            private VerSuiteMask() { }
        }

        /// <summary>
        /// Product type for pre Windows 6
        /// </summary>
        private class VerProductType
        {
            public const byte VER_NT_WORKSTATION = 0x00000001;
            public const byte VER_NT_DOMAIN_CONTROLLER = 0x00000002;
            public const byte VER_NT_SERVER = 0x00000003;

            private VerProductType() { }
        }

        /// <summary>
        /// Product type for Windows 6 and later, replaces Suite Mask, etc.
        /// </summary>
        /// <remarks>This is from http://msdn.microsoft.com/en-us/library/windows/desktop/ms724358(v=vs.85).aspx </remarks>
        private class VerProductInfo
        {
            public const UInt32 PRODUCT_BUSINESS = 0x00000006;
            public const UInt32 PRODUCT_BUSINESS_N = 0x00000010;
            public const UInt32 PRODUCT_CLUSTER_SERVER = 0x00000012;
            public const UInt32 PRODUCT_CLUSTER_SERVER_V = 0x00000040;
            public const UInt32 PRODUCT_CORE = 0x00000065;
            public const UInt32 PRODUCT_CORE_N = 0x00000062;
            public const UInt32 PRODUCT_CORE_COUNTRYSPECIFIC = 0x00000063;
            public const UInt32 PRODUCT_CORE_SINGLELANGUAGE = 0x00000064;
            public const UInt32 PRODUCT_DATACENTER_EVALUATION_SERVER = 0x00000050;
            public const UInt32 PRODUCT_DATACENTER_SERVER = 0x00000008;
            public const UInt32 PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C;
            public const UInt32 PRODUCT_DATACENTER_SERVER_CORE_V = 0x00000027;
            public const UInt32 PRODUCT_DATACENTER_SERVER_V = 0x00000025;
            public const UInt32 PRODUCT_ENTERPRISE = 0x00000004;
            public const UInt32 PRODUCT_ENTERPRISE_E = 0x00000046;
            public const UInt32 PRODUCT_ENTERPRISE_N_EVALUATION = 0x00000054;
            public const UInt32 PRODUCT_ENTERPRISE_N = 0x0000001B;
            public const UInt32 PRODUCT_ENTERPRISE_EVALUATION = 0x00000048;
            public const UInt32 PRODUCT_ENTERPRISE_SERVER = 0x0000000A;
            public const UInt32 PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E;
            public const UInt32 PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029;
            public const UInt32 PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F;
            public const UInt32 PRODUCT_ENTERPRISE_SERVER_V = 0x00000026;

            public const UInt32 PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT = 0x0000003B;
            public const UInt32 PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL = 0x0000003C;
            public const UInt32 PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC = 0x0000003D;
            public const UInt32 PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC = 0x0000003E;

            public const UInt32 PRODUCT_HOME_BASIC = 0x00000002;
            public const UInt32 PRODUCT_HOME_BASIC_E = 0x00000043;
            public const UInt32 PRODUCT_HOME_BASIC_N = 0x00000005;
            public const UInt32 PRODUCT_HOME_PREMIUM = 0x00000003;
            public const UInt32 PRODUCT_HOME_PREMIUM_E = 0x00000044;
            public const UInt32 PRODUCT_HOME_PREMIUM_N = 0x0000001A;
            public const UInt32 PRODUCT_HOME_PREMIUM_SERVER = 0x00000022;
            public const UInt32 PRODUCT_HOME_SERVER = 0x00000013;

            public const UInt32 PRODUCT_HYPERV = 0x0000002A;
            public const UInt32 PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E;
            public const UInt32 PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020;
            public const UInt32 PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F;
            public const UInt32 PRODUCT_MULTIPOINT_STANDARD_SERVER = 0x0000004C;
            public const UInt32 PRODUCT_MULTIPOINT_PREMIUM_SERVER = 0x0000004D;

            public const UInt32 PRODUCT_PROFESSIONAL = 0x00000030;
            public const UInt32 PRODUCT_PROFESSIONAL_E = 0x00000045;
            public const UInt32 PRODUCT_PROFESSIONAL_N = 0x00000031;
            public const UInt32 PRODUCT_PROFESSIONAL_WMC = 0x00000067; 

            public const UInt32 PRODUCT_SB_SOLUTION_SERVER_EM = 0x00000036;
            public const UInt32 PRODUCT_SERVER_FOR_SB_SOLUTIONS = 0x00000033;
            public const UInt32 PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM = 0x00000037;
            public const UInt32 PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018;
            public const UInt32 PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023;
            public const UInt32 PRODUCT_SERVER_FOUNDATION = 0x00000021;
            public const UInt32 PRODUCT_SB_SOLUTION_SERVER = 0x00000032;
            public const UInt32 PRODUCT_SMALLBUSINESS_SERVER = 0x00000009;
            public const UInt32 PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 0x00000019;
            public const UInt32 PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE = 0x0000003F;
            public const UInt32 PRODUCT_SOLUTION_EMBEDDEDSERVER = 0x00000038; //Windows Multipoint Server
            public const UInt32 PRODUCT_STANDARD_EVALUATION_SERVER = 0x0000004F; //Server Standard (Evaluation edition)
            public const UInt32 PRODUCT_STANDARD_SERVER = 0x00000007; //Server Standard
            public const UInt32 PRODUCT_STANDARD_SERVER_CORE = 0x0000000D;
            public const UInt32 PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028;
            public const UInt32 PRODUCT_STANDARD_SERVER_SOLUTIONS = 0x00000034; //Server Solutions Premium 
            public const UInt32 PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE = 0x00000035; //Server Solutions Premium (Core installation)
            public const UInt32 PRODUCT_STANDARD_SERVER_V = 0x00000024;

            public const UInt32 PRODUCT_STARTER = 0x0000000B;
            public const UInt32 PRODUCT_STARTER_E = 0x00000042;
            public const UInt32 PRODUCT_STARTER_N = 0x0000002F;
            public const UInt32 PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017;
            public const UInt32 PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE = 0x0000002E;
            public const UInt32 PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014;
            public const UInt32 PRODUCT_STORAGE_EXPRESS_SERVER_CORE = 0x0000002B;
            public const UInt32 PRODUCT_STORAGE_STANDARD_EVALUATION_SERVER = 0x00000060;
            public const UInt32 PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015;
            public const UInt32 PRODUCT_STORAGE_STANDARD_SERVER_CORE = 0x0000002C;
            public const UInt32 PRODUCT_STORAGE_WORKGROUP_EVALUATION_SERVER = 0x0000005F;
            public const UInt32 PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016;
            public const UInt32 PRODUCT_STORAGE_WORKGROUP_SERVER_CORE = 0x0000002D;

            public const UInt32 PRODUCT_UNDEFINED = 0x00000000;
            public const UInt32 PRODUCT_UNLICENSED = 0xABCDABCD;
            public const UInt32 PRODUCT_ULTIMATE = 0x00000001;
            public const UInt32 PRODUCT_ULTIMATE_E = 0x00000047;
            public const UInt32 PRODUCT_ULTIMATE_N = 0x0000001C;
            public const UInt32 PRODUCT_WEB_SERVER = 0x00000011;
            public const UInt32 PRODUCT_WEB_SERVER_CORE = 0x0000001D;

        }

        /// <summary>
        /// Create a new version information object, populated by reading the configuration of the local computer.
        /// </summary>
        /// <remarks>If the caller doesn't have sufficient permissions to read the local configuration an exception will be thrown.</remarks>
        public OSVersionInfo()
        {
            //load up our version information from the OS.
            OSVERSIONINFO osVersionInfo = new OSVERSIONINFO();

            OperatingSystem os = Environment.OSVersion;
            PlatformID platformId = os.Platform;
            if (platformId >= PlatformID.Unix)
            {
                // Must be Mono running on Unix/Linux/MacOSX.  P/Invokes won't work.

                osVersionInfo.PlatformId = (int)platformId;
                osVersionInfo.MajorVersion = os.Version.Major;
                osVersionInfo.MinorVersion = os.Version.Minor;
                osVersionInfo.BuildNumber = os.Version.Build;
                osVersionInfo.CSDVersion = os.ServicePack;
            }
            else // Some flavor of Windows.  There's a lot more to this.
            {
                //While we never should run on a < windows 2000 system, lets do this right anyway.  For earlier OS
                //we have to use the short version of OS Version Info.
                if (!UseOSVersionInfoEx(osVersionInfo))
                    LoadOsVersionInfo(osVersionInfo);
                else
                    LoadOsVersionInfoEx();

                //and now, is this vista or later?  If so we have to do the NEW product type
                if (MajorVersion >= 6)
                {
                    LoadOsVersionInfoPostWindows8();
                    LoadProductInfo();
                }
            }

            CalculateNames();
        }

        /// <summary>
        /// Create a version information object for the provided version configuration.
        /// </summary>
        /// <param name="platformId"></param>
        /// <param name="osVersion"></param>
        /// <param name="servicePack"></param>
        /// <param name="suiteMask"></param>
        /// <param name="productType"></param>
        /// <remarks>No calls to the operating system are made if this constructor is used.</remarks>
        public OSVersionInfo(int platformId, Version osVersion, string servicePack, int suiteMask, int productType)
        {
            PlatformId = platformId;
            MajorVersion = osVersion.Major;
            MinorVersion = osVersion.Minor;
            BuildNumber = osVersion.Build;

            //note we do not store the OS version, we'll recalculate it if asked to get rid of any cruft and ensure round-trip safety.

            ServicePack = servicePack;
            SuiteMask = suiteMask;
            ProductType = productType;

            CalculateNames();
        }

        #region Public Properties and Methods

        /// <summary>
        /// The platform Id, nearly always 1 indicating Windows NT.  0 indicates Win32s and 2 indicates Windows CE.
        /// </summary>
        public int PlatformId { get; private set; }

        /// <summary>
        /// The major operating system version
        /// </summary>
        public int MajorVersion { get; private set; }

        /// <summary>
        /// The minor operating system version
        /// </summary>
        public int MinorVersion { get; private set; }
        
        /// <summary>
        /// The operating system build number
        /// </summary>
        public int BuildNumber { get; private set; }

        /// <summary>
        /// The major version of the service pack applied
        /// </summary>
        public int SpMajorVersion { get; private set; }

        /// <summary>
        /// The minor version of the service pack applied.
        /// </summary>
        public int SpMinorVersion { get; private set; }

        /// <summary>
        /// The operating system suite mask, indicating the specific OS installation for windows prior to 6.0
        /// </summary>
        public int SuiteMask { get; private set; }

        /// <summary>
        /// The display string for service pack, empty if no service pack is applied.
        /// </summary>
        public string ServicePack { get; private set; }

        /// <summary>
        /// The product type.  Prior to windows 6.0 this just indicates server, workstation or domain controller.  After 6.0 this replaced Suite Mask.
        /// </summary>
        public int ProductType { get; private set; }

        /// <summary>
        /// The well known operating system family name, like Windows Vista or Windows Server 2003.
        /// </summary>
        public string FamilyName
        {
            get
            {
                if (m_NamesCalculated == false)
                {
                    CalculateNames();
                }

                return m_OSFamilyName;
            }
        }

        /// <summary>
        /// The edition of the operating system without the family name, such as Workstation or Standard Server.
        /// </summary>
        public string EditionName
        {
            get
            {
                if (m_NamesCalculated == false)
                {
                    CalculateNames();
                }

                return m_OSEditionName;
            }
        }

        /// <summary>
        /// The well known OS name and edition name
        /// </summary>
        public string FullName
        {
            get
            {
                if (m_NamesCalculated == false)
                {
                    CalculateNames();
                }

                return m_OSFamilyName + (string.IsNullOrEmpty(m_OSEditionName) ? string.Empty : " " + m_OSEditionName);               
            }
        }

        /// <summary>
        /// The well known OS name, edition name, and service pack like Windows XP Professional Service Pack 3
        /// </summary>
        public string FullNameWithServicePack
        {
            get
            {
                return FullName + (string.IsNullOrEmpty(ServicePack) ? string.Empty : " " + ServicePack);
            }
        }

        /// <summary>
        /// The version information object for the major.minor.build.
        /// </summary>
        public Version Version
        {
            get
            {
                if (m_Version == null)
                {
                    m_Version = new Version(MajorVersion, MinorVersion, BuildNumber);
                }

                return m_Version;
            }
        }

        #endregion

        #region Private Properties and Methods

        // check for NT4 SP6 or later
        private static bool UseOSVersionInfoEx(OSVERSIONINFO info)
        {
            bool nativeCallResult = NativeMethods.GetVersionEx(info);

            if (!nativeCallResult)
            {
                int error = Marshal.GetLastWin32Error();

                throw new InvalidOperationException("Failed to get OSVersionInfo. Error = 0x" + error.ToString("8X", CultureInfo.CurrentCulture));
            }

            if (info.PlatformId >= VerPlatformId.Unix) return true; // TODO: Check that this is correct/meaningful for Unix.

            if (info.MajorVersion < 4) return false;
            if (info.MajorVersion > 4) return true;

            if (info.MinorVersion < 0) return false;
            if (info.MinorVersion > 0) return true;

            if (info.CSDVersion == "Service Pack 6") return true;

            return false;
        }

        private void LoadOsVersionInfo(OSVERSIONINFO info)
        {
            PlatformId = info.PlatformId;

            MajorVersion = info.MajorVersion;
            MinorVersion = info.MinorVersion;
            BuildNumber = info.BuildNumber;
            ServicePack = info.CSDVersion;
        }

        private void LoadOsVersionInfoEx()
        {
            var info = new OSVERSIONINFOEX();

            bool nativeCallResult = NativeMethods.GetVersionEx(info);

            if (!nativeCallResult)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException("Failed to get OSVersionInfoEx. Error = 0x" + error.ToString("8X", CultureInfo.CurrentCulture));
            }

            PlatformId = info.PlatformId;
            MajorVersion = info.MajorVersion;
            MinorVersion = info.MinorVersion;
            BuildNumber = info.BuildNumber;
            ServicePack = info.CSDVersion;
            SpMajorVersion = info.ServicePackMajor;
            SpMinorVersion = info.ServicePackMinor;
            SuiteMask = info.SuiteMask;
            ProductType = info.ProductType;
        }

        private void LoadOsVersionInfoPostWindows8()
        {
            var info = new OSVERSIONINFOEX();

            var nativeCallResult = NativeMethods.RtlGetVersion(info);

            if (nativeCallResult != 0)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException("Failed to get RtlGetVersion. Error = 0x" + error.ToString("8X", CultureInfo.CurrentCulture));
            }

            PlatformId = (int) info.PlatformId;
            MajorVersion = (int) info.MajorVersion;
            MinorVersion = (int) info.MinorVersion;
            BuildNumber = (int) info.BuildNumber;
            ServicePack = info.CSDVersion;
            SpMajorVersion = info.ServicePackMajor;
            SpMinorVersion = info.ServicePackMinor;
            SuiteMask = info.SuiteMask;
            ProductType = info.ProductType;
        }

        private void LoadProductInfo()
        {
            int productType;
            bool nativeCallResult = NativeMethods.GetProductInfo(MajorVersion, MinorVersion, SpMajorVersion, SpMinorVersion, out productType);

            if (!nativeCallResult)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException("Failed to get OSVersionInfoEx. Error = 0x" + error.ToString("8X", CultureInfo.CurrentCulture));
            }

            ProductType = productType;
        }

        private void CalculateNames()
        {
            m_NamesCalculated = true; //so we don't keep retrying this.

            string familyName = null;
            switch (PlatformId)
            {
                case VerPlatformId.Win32s:
                    familyName = "Win32s";
                    break;
                case VerPlatformId.Win32Windows:
                    switch (MinorVersion)
                    {
                        case 0:
                            familyName = "Windows 95";
                            break;
                        case 10:
                            familyName = "Windows 98";
                            break;
                        case 90:
                            familyName = "Windows ME";
                            break;
                    }
                    break;
                case VerPlatformId.Win32NT:

                    switch (MajorVersion)
                    {
                        case 3:
                            familyName = "Windows NT 3.51";
                            break;
                        case 4:
                            familyName = "Windows NT 4.0";
                            break;
                        case 5:
                            switch (MinorVersion)
                            {
                                case 0:
                                    familyName = "Windows 2000";
                                    break;
                                case 1:
                                    familyName = "Windows XP";
                                    break;
                                case 2:
                                    familyName = "Windows 2003";
                                    break;
                            }
                            break;
                        case 6:
                            switch (MinorVersion)
                            {
                                case 0:
                                    //now we really have a problem - people think of the server & client as being totally different families,
                                    //but we can't distinguish that on this side.
                                    familyName = "Windows Vista";
                                    break;
                                case 1:
                                    familyName = "Windows 7";
                                    break;
                                case 2:
                                    familyName = "Windows 8";
                                    break;
                                case 3:
                                    familyName = "Windows 8.1";
                                    break;
                            }
                            break;
                        case 10:
                            familyName = "Windows 10";
                            break;
                    }
                    break;
                case VerPlatformId.WinCE:
                    familyName = "Windows CE";
                    break;

                // Added for hypothetical Mono support...

                case VerPlatformId.UnixMono1:
                case VerPlatformId.Unix:
                    familyName = "Unix";
                    break;
                case VerPlatformId.MaxOSX:
                    familyName = "MacOS X";
                    break;
            }

            //if we couldn't calculate a family name, just punt and use the version.
            if (string.IsNullOrEmpty(familyName))
            {
                familyName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", MajorVersion, MinorVersion, BuildNumber);
            }

            //Now figure out the correct edition name.  
            string editionName = string.Empty;

            if (PlatformId >= VerPlatformId.Unix)
            {
                // It's a Linux/Unix/MaxOSX running us under Mono.
                // TODO: Anything further we can do to distinguish the Unix "edition" or equivalent?  Should it be left null or ""?
            }
            else
            {
                // It's one of the Windows flavors.  Figure out which one.
                if (MajorVersion < 6)
                {
                    //Before Vista this is primarily about Suite, but we'll look at version if we have to.
                    if ((ProductType == VerProductType.VER_NT_SERVER)
                        || (ProductType == VerProductType.VER_NT_DOMAIN_CONTROLLER))
                    {
                        //it's a server product of some type.
                        if ((TestFlag(SuiteMask, VerSuiteMask.VER_SUITE_SMALLBUSINESS))
                            || (TestFlag(SuiteMask, VerSuiteMask.VER_SUITE_SMALLBUSINESS_RESTRICTED)))
                        {
                            editionName = "Small Business Server";
                        }
                        else if (TestFlag(SuiteMask, VerSuiteMask.VER_SUITE_DATACENTER))
                        {
                            editionName = "Datacenter Server";
                        }
                        else if (TestFlag(SuiteMask, VerSuiteMask.VER_SUITE_ENTERPRISE))
                        {
                            //a little goofy - for W2k they used a different caption
                            switch (MinorVersion)
                            {
                                case 0:
                                    editionName = "Advanced Server";
                                    break;
                                default:
                                    editionName = "Enterprise Server";
                                    break;
                            }
                        }
                        else if (TestFlag(SuiteMask, VerSuiteMask.VER_SUITE_BLADE))
                        {
                            editionName = "Web Server";
                        }
                        else if ((TestFlag(SuiteMask, VerSuiteMask.VER_SUITE_EMBEDDEDNT))
                                 || (TestFlag(SuiteMask, VerSuiteMask.VER_SUITE_EMBEDDED_RESTRICTED)))
                        {
                            editionName = "Embedded";
                        }
                        else if (TestFlag(SuiteMask, VerSuiteMask.VER_SUITE_WH_SERVER))
                        {
                            editionName = "Home Server";
                        }
                        else
                        {
                            //no suite mask - it's just a standard server.
                            editionName = "Standard Server";
                        }
                    }
                    else if (ProductType == VerProductType.VER_NT_WORKSTATION)
                    {
                        //now we need to break down the crazy different rendition of desktop editions.
                        if (TestFlag(SuiteMask, VerSuiteMask.VER_SUITE_PERSONAL))
                        {
                            editionName = "Home";
                        }
                        else
                        {
                            editionName = "Professional";
                        }
                    }
                }
                else
                {
                    //now that we're in Vista or later we use product type pretty much exclusively.
                    bool isServer = false;
                    switch ((uint)ProductType)
                    {
                        case VerProductInfo.PRODUCT_BUSINESS:
                            editionName = "Business";
                            break;
                        case VerProductInfo.PRODUCT_BUSINESS_N:
                            editionName = "Business N";
                            break;
                        case VerProductInfo.PRODUCT_CLUSTER_SERVER:
                        case VerProductInfo.PRODUCT_CLUSTER_SERVER_V:                           
                            isServer = true;
                            editionName = "Cluster Server";
                            break;
                        case VerProductInfo.PRODUCT_DATACENTER_EVALUATION_SERVER:
                        case VerProductInfo.PRODUCT_DATACENTER_SERVER:
                        case VerProductInfo.PRODUCT_DATACENTER_SERVER_V:
                            isServer = true;
                            editionName = "Datacenter Server";
                            break;
                        case VerProductInfo.PRODUCT_DATACENTER_SERVER_CORE:
                        case VerProductInfo.PRODUCT_DATACENTER_SERVER_CORE_V:
                            isServer = true;
                            editionName = "Datacenter Server Core";
                            break;
                        case VerProductInfo.PRODUCT_ENTERPRISE_EVALUATION:
                        case VerProductInfo.PRODUCT_ENTERPRISE:
                            isServer = false; //you're thinking Enterprise = server, right?  No, this is Enterprise desktop.
                            editionName = "Enterprise";
                            break;
                        case VerProductInfo.PRODUCT_ENTERPRISE_E:
                            isServer = false; //you're thinking Enterprise = server, right?  No, this is Enterprise desktop.
                            editionName = "Enterprise E";
                            break;
                        case VerProductInfo.PRODUCT_ENTERPRISE_N_EVALUATION:
                        case VerProductInfo.PRODUCT_ENTERPRISE_N:
                            isServer = false; //you're thinking Enterprise = server, right?  No, this is Enterprise desktop.
                            editionName = "Enterprise N";
                            break;
                        case VerProductInfo.PRODUCT_ENTERPRISE_SERVER:
                        case VerProductInfo.PRODUCT_ENTERPRISE_SERVER_V:
                        case VerProductInfo.PRODUCT_ENTERPRISE_SERVER_IA64:
                            isServer = true;
                            editionName = "Enterprise Server";
                            break;
                        case VerProductInfo.PRODUCT_ENTERPRISE_SERVER_CORE:
                        case VerProductInfo.PRODUCT_ENTERPRISE_SERVER_CORE_V:
                            isServer = true;
                            editionName = "Enterprise Server Core";
                            break;
                        case VerProductInfo.PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT:
                        case VerProductInfo.PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC:
                            isServer = true;
                            editionName = "Essential Server Solution Management";
                            break;
                        case VerProductInfo.PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL:
                        case VerProductInfo.PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC:
                            isServer = true;
                            editionName = "Essential Server Solution Additional";
                            break;
                        case VerProductInfo.PRODUCT_HOME_BASIC:
                            editionName = "Home Basic";
                            break;
                        case VerProductInfo.PRODUCT_HOME_BASIC_E:
                            editionName = "Home Basic E";
                            break;
                        case VerProductInfo.PRODUCT_HOME_BASIC_N:
                            editionName = "Home Basic N";
                            break;
                        case VerProductInfo.PRODUCT_HOME_PREMIUM:
                            editionName = "Home Premium";
                            break;
                        case VerProductInfo.PRODUCT_HOME_PREMIUM_E:
                            editionName = "Home Premium E";
                            break;
                        case VerProductInfo.PRODUCT_HOME_PREMIUM_N:
                            editionName = "Home Premium N";
                            break;
                        case VerProductInfo.PRODUCT_HOME_PREMIUM_SERVER:
                            isServer = true;
                            editionName = "Home Server 2011";
                            break;
                        case VerProductInfo.PRODUCT_HOME_SERVER:
                            isServer = true;
                            editionName = "Home Server";
                            break;
                        case VerProductInfo.PRODUCT_HYPERV:
                            isServer = true;
                            editionName = "Hyper-V Server";
                            break;
                        case VerProductInfo.PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                            isServer = true;
                            editionName = "Essential Business Server Management Server";
                            break;
                        case VerProductInfo.PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                            isServer = true;
                            editionName = "Essential Business Server Messaging Server";
                            break;
                        case VerProductInfo.PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                            isServer = true;
                            editionName = "Essential Business Server Security Server";
                            break;
                        case VerProductInfo.PRODUCT_MULTIPOINT_STANDARD_SERVER:
                            isServer = true;
                            editionName = "MultiPoint Server Standard";
                            break;
                        case VerProductInfo.PRODUCT_MULTIPOINT_PREMIUM_SERVER:
                            isServer = true;
                            editionName = "MultiPoint Server Premium ";
                            break;
                        case VerProductInfo.PRODUCT_CORE:
                            editionName = string.Empty;
                            break;
                        case VerProductInfo.PRODUCT_CORE_N:
                            editionName = "N";
                            break;
                        case VerProductInfo.PRODUCT_CORE_COUNTRYSPECIFIC:
                            editionName = "China";
                            break;
                        case VerProductInfo.PRODUCT_CORE_SINGLELANGUAGE:
                            editionName = "Single Language";
                            break;
                        case VerProductInfo.PRODUCT_PROFESSIONAL:
                            editionName = "Professional";
                            break;
                        case VerProductInfo.PRODUCT_PROFESSIONAL_E:
                            editionName = "Professional E";
                            break;
                        case VerProductInfo.PRODUCT_PROFESSIONAL_N:
                            editionName = "Professional N";
                            break;
                        case VerProductInfo.PRODUCT_PROFESSIONAL_WMC:
                            editionName = "Professional with Media Center";
                            break;
                        case VerProductInfo.PRODUCT_SB_SOLUTION_SERVER_EM:
                        case VerProductInfo.PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM:
                            editionName = "Server For SB Solutions EM";
                            break;
                        case VerProductInfo.PRODUCT_SERVER_FOR_SB_SOLUTIONS:
                            editionName = "Server For SB Solutions";
                            break;
                        case VerProductInfo.PRODUCT_SERVER_FOR_SMALLBUSINESS:
                        case VerProductInfo.PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
                            isServer = true;
                            editionName = "Windows Essential Server Solutions";
                            break;
                        case VerProductInfo.PRODUCT_SERVER_FOUNDATION:
                            isServer = true;
                            editionName = "Server Foundation";
                            break;
                        case VerProductInfo.PRODUCT_SB_SOLUTION_SERVER:
                            isServer = true;
                            editionName = "Small Business Server 2011 Essentials";
                            break;
                        case VerProductInfo.PRODUCT_SMALLBUSINESS_SERVER:
                            isServer = true;
                            editionName = "Small Business Server";
                            break;
                        case VerProductInfo.PRODUCT_SMALLBUSINESS_SERVER_PREMIUM:
                            isServer = true;
                            editionName = "Small Business Server Premium";
                            break;
                        case VerProductInfo.PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE:
                            isServer = true;
                            editionName = "Small Business Server Premium Core";
                            break;
                        case VerProductInfo.PRODUCT_SOLUTION_EMBEDDEDSERVER:
                            isServer = true;
                            editionName = "Multipoint Server";
                            break;
                        case VerProductInfo.PRODUCT_STANDARD_EVALUATION_SERVER:
                        case VerProductInfo.PRODUCT_STANDARD_SERVER:
                        case VerProductInfo.PRODUCT_STANDARD_SERVER_V:
                            isServer = true;
                            editionName = "Standard Server";
                            break;
                        case VerProductInfo.PRODUCT_STANDARD_SERVER_CORE:
                        case VerProductInfo.PRODUCT_STANDARD_SERVER_CORE_V:
                            isServer = true;
                            editionName = "Standard Server Core";
                            break;
                        case VerProductInfo.PRODUCT_STANDARD_SERVER_SOLUTIONS:
                            isServer = true;
                            editionName = "Server Solutions Premium";
                            break;
                        case VerProductInfo.PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE:
                            isServer = true;
                            editionName = "Server Solutions Premium Core";
                            break;
                        case VerProductInfo.PRODUCT_STARTER:
                            editionName = "Starter";
                            break;
                        case VerProductInfo.PRODUCT_STARTER_E:
                            editionName = "Starter E";
                            break;
                        case VerProductInfo.PRODUCT_STARTER_N:
                            editionName = "Starter N";
                            break;
                        case VerProductInfo.PRODUCT_STORAGE_ENTERPRISE_SERVER:
                            isServer = true;
                            editionName = "Storage Server Enterprise";
                            break;
                        case VerProductInfo.PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE:
                            isServer = true;
                            editionName = "Storage Server Enterprise Core";
                            break;
                        case VerProductInfo.PRODUCT_STORAGE_EXPRESS_SERVER:
                            isServer = true;
                            editionName = "Storage Server Express";
                            break;
                        case VerProductInfo.PRODUCT_STORAGE_EXPRESS_SERVER_CORE:
                            isServer = true;
                            editionName = "Storage Server Express Core";
                            break;
                        case VerProductInfo.PRODUCT_STORAGE_STANDARD_EVALUATION_SERVER:
                        case VerProductInfo.PRODUCT_STORAGE_STANDARD_SERVER:
                            isServer = true;
                            editionName = "Storage Server Standard";
                            break;
                        case VerProductInfo.PRODUCT_STORAGE_STANDARD_SERVER_CORE:
                            isServer = true;
                            editionName = "Storage Server Standard Core";
                            break;
                        case VerProductInfo.PRODUCT_STORAGE_WORKGROUP_EVALUATION_SERVER:
                        case VerProductInfo.PRODUCT_STORAGE_WORKGROUP_SERVER:
                            isServer = true;
                            editionName = "Storage Server Workgroup";
                            break;
                        case VerProductInfo.PRODUCT_STORAGE_WORKGROUP_SERVER_CORE:
                            isServer = true;
                            editionName = "Storage Server Workgroup Core";
                            break;
                        case VerProductInfo.PRODUCT_UNDEFINED:
                            editionName = "Unknown";
                            break;
                        case VerProductInfo.PRODUCT_UNLICENSED:
                            editionName = "Unlicensed";
                            break;
                        case VerProductInfo.PRODUCT_ULTIMATE:
                            editionName = "Ultimate Edition";
                            break;
                        case VerProductInfo.PRODUCT_ULTIMATE_E:
                            editionName = "Ultimate E Edition";
                            break;
                        case VerProductInfo.PRODUCT_ULTIMATE_N:
                            editionName = "Ultimate N Edition";
                            break;
                        case VerProductInfo.PRODUCT_WEB_SERVER:
                            isServer = true;
                            editionName = "Web Server";
                            break;
                        case VerProductInfo.PRODUCT_WEB_SERVER_CORE:
                            isServer = true;
                            editionName = "Web Server Core";
                            break;
                    }

                    //for version 6.0 we have to use the is server option to know the difference between vista and 2008
                    if (isServer)
                    {
                        if (MajorVersion == 6)
                        {
                            switch (MinorVersion)
                            {
                                case 0:
                                    familyName = "Windows 2008";
                                    break;
                                case 1:
                                    familyName = "Windows 2008 R2";
                                    break;
                                case 2:
                                    familyName = "Windows 2012";
                                    break;
                                case 3:
                                    familyName = "Windows 2012 R2";
                                    break;
                                default:
                                    familyName = "Windows Server";
                                    break;
                            }
                        }
                        else if (MajorVersion == 10)
                        {
                            if (BuildNumber <= 17134)
                                familyName = "Windows 2016";
                            else
                                familyName = "Windows 2019";
                        }
                    }
                }
            }

            m_OSFamilyName = familyName;
            m_OSEditionName = editionName;
        }

        private static bool TestFlag(int mask, uint testFlag)
        {
            return ((mask & testFlag) != 0);
        }


        #endregion

    }
}
