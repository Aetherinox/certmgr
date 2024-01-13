using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MSBuildCode;
using System.Management.Automation;
using certmgr;
using Lng = certmgr.Properties.Resource;
using Cfg = certmgr.Properties.Settings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace CertMgr
{

    public class CertMgr
    {

        /*
            Define > Classes
        */

        private AppInfo AppInfo = new AppInfo( );

        /*
            Execute powershell query
            checks to see if a target file has been signed with x509 cert

            @param      : str query
            @return     : str
        */

        public static string PowershellQ( string query )
        {
            using ( PowerShell ps = PowerShell.Create( ) )
            {

                ps.AddScript( query );

                Collection<PSObject> PSOutput = ps.Invoke( );
                StringBuilder sb = new StringBuilder( );

                foreach ( PSObject PSItem in PSOutput )
                {
                    if ( PSItem != null )
                    {
                        Console.WriteLine( PSItem );
                        sb.AppendLine( PSItem.ToString( ) );
                    }
                }

                if ( ps.Streams.Error.Count > 0 )
                {
                    // Error collection
                }

                return sb.ToString( );
            }
        }

        /*
            Hacky method for keeping console commands from printing
                re-route output
        */

        static public void HandleOutput(string input) { }

        /*
            Exists on Path
        */

        public static bool ExistsOnPath(string exeName)
        {
            try
            {
                using ( Process p = new Process( ) )
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = "where";
                    p.StartInfo.Arguments = exeName;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.OutputDataReceived += (s, e) => HandleOutput( e.Data );
                    p.Start( );
                    p.BeginOutputReadLine( );
                    p.WaitForExit( );

                    return p.ExitCode == 0;
                }
            }
            catch( Win32Exception )
            {
                throw new Exception("'where' command is not on path");
            }
        }

        /*
            About
        */

        static public int About()
        {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( );
                Console.WriteLine( " " + AppInfo.Title + " " );
                Console.WriteLine( );
                Console.WriteLine( " @author        : " + AppInfo.Company + " " );
                Console.WriteLine( " @version       : " + AppInfo.PublishVersion + " " );
                Console.WriteLine( " @copyright     : " + AppInfo.Copyright + " " );
                Console.WriteLine( " @website       : https://github.com/aetherinox/certmgr" );
                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.Gray ;
                Console.WriteLine( );
                Console.WriteLine(@" certmgr.exe is a replacement for:
   sn.exe                   Strong Name Tool
   signtool.exe             Sign Tool
   certutil.exe             Certificate Services & Management
    
 This utility helps sign assemblies with strong names. It provides options 
 for key management, signature generation, and signature verification. It
 should be used in combination with Visual Studio and utilized when you
 want to sign a project, or change the PFX keyfile used to sign a project.
                ");

                Console.WriteLine( " Commands:" );
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   -n, --info               ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Show information about the pfx_infile" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   -l, --list               ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "List all certificates" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   -i, --install            ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Install PFX keyfile" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   -s, --sign               ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Sign binary / dll with key using signtool" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   -d, --delete             ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Delete key for specified container id" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   -x, --deleteall          ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Delete all keys for all containers" );

                Console.WriteLine( );
                Console.WriteLine( );

                Console.WriteLine( " Examples:" );
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -n <PFX_FILE>                          ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Show information about the pfx_infile" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -i <PFX_FILE> <PFX_PASSWD>             ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Install PFX keyfile" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -i <PFX_FILE> <PFX_PASSWD> <VS_KEY>    ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Install PFX keyfile for VS project container" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -l                                     ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "List all certificates" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -d <VS_KEY_ID>                         ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Delete key for specified container id" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -x                                     ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Delete all keys for all containers" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -s <PFX_THUMBPRINT> <FILE_TO_SIGN>     ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Sign binary / dll with key using signtool" );

                Console.WriteLine( );
                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.DarkGray ;
                Console.WriteLine( );
                Console.ResetColor() ;

            return -1;
        }

        /*
            Main
        */

        static int Main( string[] args )
        {

            /*
                Help / About
            */

            if ( args.Length == 0 || args[ 0 ] == "-h" || args[ 0 ] == "--help" )
            {
                About( );
                return -1;
            }

            /*
                Info
                    lists the status of a specified pfx cert file
            */

            if ( args[ 0 ].StartsWith( "--info" ) || args[ 0 ].StartsWith( "-n" ) )
            {

                /*
                    Basic info about pfx certificate and container
                */

                if ( args.Length == 1 )
                {
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write( " Error: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( "Missing " );
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write( "pfx certificate file" );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write( $"        {Assembly.GetEntryAssembly( ).GetName( ).Name }.exe --info <PFX_FILE>");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );

                    return -1;
                }

                string arg_pfx_path             = args[ 1 ];
                string arg_pfx_container_id     = ResolveKeySourceTask.ResolveAssemblyKey( arg_pfx_path );

                Console.WriteLine( arg_pfx_container_id );
                Console.WriteLine( $"Installed: {ResolveKeySourceTask.IsContainerInstalled( arg_pfx_container_id )}" );

                return 0;
            }

            /*
                List
                    Utilize certutil to list all keys
            */

            if ( args[ 0 ].StartsWith( "--list" ) || args[ 0 ].StartsWith( "-l" ) )
            {

                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( $"   { AppInfo.Title } : List");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.White;

                /*
                    Check for Certutil
                        This should never happen, but just in case.
                */

                bool bCertUtil = ExistsOnPath( "certutil" );

                if ( !bCertUtil )
                {
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write( " Error: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( "Could not find the program" );
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write( " Certutil.exe" );
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( " Certutil must be in your Windows PATH environment variables list." );
                    Console.WriteLine( );
                    Console.Write( " Check " );
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write( @"C:\Windows\System32\certutil.exe" );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );

                    return -1;
                }

                string cmd = "certutil -csp \"Microsoft Strong Cryptographic Provider\" -key";
                PowershellQ( cmd );

                return -1;

            }

            /*
                Certutil > Delete
            */

            if ( args[ 0 ].StartsWith( "--delete" ) || args[ 0 ].StartsWith( "-d" ) )
            {
                if ( args.Length == 1 )
                {
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write( " Error: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( "Must specify " );
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write( "VS_ID_CONTAINER" );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write( $"        {Assembly.GetEntryAssembly( ).GetName( ).Name }.exe --delete <VS_ID_CONTAINER>");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );

                    return -1;
                }

                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( $"   { AppInfo.Title } : Delete");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.Gray ;
                Console.ResetColor( );

                /*
                    Check for Certutil
                        This should never happen, but just in case.
                */

                bool bCertUtil = ExistsOnPath( "certutil" );

                if ( !bCertUtil )
                {
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write( " Error: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( "Could not find the program" );
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write( " Certutil.exe" );
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( " Certutil must be in your Windows PATH environment variables list." );
                    Console.WriteLine( );
                    Console.Write( " Check " );
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write( @"C:\Windows\System32\certutil.exe" );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );

                    return -1;
                }

                string arg_container_id     = args[ 1 ];
                string ps_executecmd        = "certutil -delkey -csp \"Microsoft Strong Cryptographic Provider\" \""+ arg_container_id + "\"";

                PowershellQ( ps_executecmd );

                return -1;
            }

            /*
                Certutil > Delete All
            */

            if ( args[ 0 ].StartsWith( "--deleteall" ) || args[ 0 ].StartsWith( "-x" ) )
            {
                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( $"   { AppInfo.Title } : Delete All");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.Gray ;
                Console.ResetColor( );

                string ps_executecmd ="certutil -csp \"Microsoft Strong Cryptographic Provider\" -key | Select-String -Pattern \"VS_KEY\" | %{ $_.ToString().Trim()} | %{ certutil -delkey -csp \"Microsoft Strong Cryptographic Provider\" $_}";
                PowershellQ( ps_executecmd );

                return -1;
            }

            /*
                Certutil > Sign
            */

            if ( args[ 0 ].StartsWith( "--sign" ) || args[ 0 ].StartsWith( "-s" ) )
            {
                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( $"   { AppInfo.Title } : Sign" );
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.Gray ;
                Console.ResetColor( );

                /*
                    Check for Signtool
                */

                bool bSignTool = ExistsOnPath( "signtool" );

                if ( !bSignTool )
                {
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write( " Error: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( "Could not find the program" );
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write( " signtool.exe" );
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( " signtool.exe must be in your Windows PATH environment variables list." );
                    Console.WriteLine( );

                    return -1;
                }

                /*
                    Args > Missing Thumbprint
                */

                if ( args.Length == 1 )
                {
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write( " Error: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( "Missing certificate thumbprint " );
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write( "PFX_THUMBPRINT" );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write( $"        {Assembly.GetEntryAssembly( ).GetName( ).Name }.exe --sign <PFX_THUMBPRINT> <FILE_TO_SIGN>");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );

                    return -2;
                }

                string arg_pfx_thumbprint   = args[ 1 ];

                /*
                    Args > File to Sign
                */

                if ( args.Length == 2 )
                {
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write( " Error: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( "Missing binary to sign " );
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write( "FILE_TO_SIGN" );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write( $"        {Assembly.GetEntryAssembly( ).GetName( ).Name }.exe --sign <PFX_THUMBPRINT> <FILE_TO_SIGN>");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );

                    return -2;
                }

                string arg_file_tosign      = args[ 2 ];
                string ps_executecmd        = "signtool sign /sha1 \"" + arg_pfx_thumbprint + "\" /fd SHA256 /t http://timestamp.comodoca.com/authenticode \"" + arg_file_tosign + "\"";
 
                PowershellQ( ps_executecmd );

                return -1;
            }

            /*
                Install
            */

            if ( args[ 0 ].StartsWith( "--install" ) || args[ 0 ].StartsWith( "-i" ) )
            {
                string arg_pfx_path         = args[ 1 ];
                string arg_pfx_container_id = args.Length == 4 ? args[ 3 ] : ResolveKeySourceTask.ResolveAssemblyKey( arg_pfx_path );

                if ( args.Length == 2 )
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write( " Error: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( "Missing additional arguments" );
                    Console.WriteLine( );
                    Console.WriteLine( "        <pfx_passwd>           PFX certificate password" );
                    Console.WriteLine( "        <vs_id_container>      Visual Studio container ID" );


                    return 0;
                }

                if ( ResolveKeySourceTask.IsContainerInstalled( arg_pfx_container_id ))
                {
                    //Installs from infile in the specified key container. The key container resides in the strong name CSP.
                    Console.WriteLine( );
                    Console.Error.WriteLine( "Key pair already installed in strong name CSP key container: " + arg_pfx_container_id + "." );
                    Console.WriteLine( );
                    Console.Error.WriteLine( "To delete the key container run following command from the Developer Command Prompt:");
                    Console.Error.WriteLine( $"     {Assembly.GetEntryAssembly( ).GetName( ).Name }.exe -delete " + arg_pfx_container_id );
                    Console.Error.WriteLine( );
                    Console.Error.WriteLine( "To list all installed key containers run the command:" );
                    Console.Error.WriteLine( $"     {Assembly.GetEntryAssembly( ).GetName( ).Name }.exe -list" );

                    return -2;
                }

                string arg_pfx_passwd = args[ 2 ];

                /*
                    open pfx and export private key
                */

                var pfxCert         = new X509Certificate2( arg_pfx_path, arg_pfx_passwd, X509KeyStorageFlags.Exportable );
                var pfxPrivateKey   = pfxCert.PrivateKey as RSACryptoServiceProvider;
                var pfxCspBlob      = pfxPrivateKey.ExportCspBlob( true );

                /*
                    Create Cryptographic Service Provider and register key container
                */

                const string DotNetStrongSigningCSP = "Microsoft Strong Cryptographic Provider";
                var cspParameters = new CspParameters( 1, DotNetStrongSigningCSP, arg_pfx_container_id )
                {
                    KeyNumber   = (int)KeyNumber.Signature, // signing container
                    Flags       = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseNonExportableKey
                };

                try
                {
                    using ( var rsaCSP = new RSACryptoServiceProvider( cspParameters ) )
                    {
                        rsaCSP.PersistKeyInCsp = true;
                        rsaCSP.ImportCspBlob( pfxCspBlob );
                    };
                }
                catch ( CryptographicException e )
                {
                    if ( !e.Message.Contains( "Object already exists." ) )
                    {
                        throw;
                    }
                    Console.Error.WriteLine( $"An error occurred while attempting to create the strong name CSP key container '{arg_pfx_container_id}'.");
                    Console.Error.WriteLine( "It's likely that this container was already created by another user.");
                    Console.Error.WriteLine( "Either get that user to modify the container to be readable by everyone, or to delete the container with the following command from the Developer Command Prompt:");
                    Console.Error.WriteLine( $"sn.exe -d {arg_pfx_container_id}");

                    return -2;
                }

                Console.Error.WriteLine($"The key pair has been installed into the strong name CSP key container '{arg_pfx_container_id}'.");
                Console.WriteLine( arg_pfx_container_id );

                return 0;

            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write( " Error: " );

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write( "Unknown command" );
            Console.WriteLine( );

            return -1;

        }

    }


}


