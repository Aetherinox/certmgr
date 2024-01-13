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
using System.Xml.Linq;

namespace CertMgr
{

    public class CertMgr
    {

        /*
            Define > Classes
        */

        private AppInfo AppInfo = new AppInfo( );

        /*
            Define > Exit Codes
        */

        [Flags]
        enum ExitCode : int
        {
            None                = -1,
            Abort               = 0,
            Success             = 1,
            ErrorMissingArg     = 2,
            ErrorMissingDep     = 4,
            ErrorGeneric        = 8,
        }

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
                Console.Write( "Show information about a specified pfx keyfile" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   -l, --list               ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "List all certificates" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   -i, --install            ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Install PFX keyfile and register with container" );

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
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -n <PFX_FILE>                            ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Show information about the pfx_infile" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -i <PFX_FILE> <PFX_PASSWD>               ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Install PFX keyfile" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -i <PFX_FILE> <PFX_PASSWD> <VS_KEY_ID>   ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Install PFX keyfile for VS project container" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -l                                       ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "List all certificates" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -d <VS_KEY_ID>                           ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Delete key for specified container id" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -x                                       ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Delete all keys for all containers" );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write( $"   {Assembly.GetEntryAssembly( ).GetName( ).Name } -s <PFX_THUMBPRINT> <FILE_TO_SIGN>       ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write( "Sign binary / dll with key using signtool" );

                Console.WriteLine( );
                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( " ------------------------------------------------------------------------------" );
                Console.ForegroundColor = ConsoleColor.DarkGray ;
                Console.WriteLine( );
                Console.ResetColor() ;

            return (int)ExitCode.None;
        }

        /*
            Main
        */

        static int Main( string[] args )
        {

            string assemblyName = Assembly.GetEntryAssembly( ).GetName( ).Name;

            /*
                Help / About
            */

            if ( args.Length == 0 || args[ 0 ] == "-h" || args[ 0 ] == "--help" )
            {
                About( );
                return (int)ExitCode.None;
            }

            /*
                Info
                    lists the status of a specified pfx cert file
            */

            if ( args[ 0 ] == "--info" || args[ 0 ] == "-n" )
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
                    Console.Write( $"        {assemblyName} --info <PFX_FILE>");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    Console.WriteLine( );

                    return (int)ExitCode.ErrorMissingArg;
                }

                string arg_pfx_path             = args[ 1 ];
                string arg_pfx_container_id     = ResolveKeySourceTask.ResolveAssemblyKey( arg_pfx_path );

                Console.WriteLine( arg_pfx_container_id );
                Console.WriteLine( $"Installed: {ResolveKeySourceTask.IsContainerInstalled( arg_pfx_container_id )}" );

                return (int)ExitCode.Success;
            }

            /*
                List
                    Utilize certutil to list all keys
            */

            if ( args[ 0 ] == "--list" || args[ 0 ] == "-l" )
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
                    Console.WriteLine( );

                    return (int)ExitCode.ErrorMissingDep;
                }

                string cmd = "certutil -csp \"Microsoft Strong Cryptographic Provider\" -key";
                PowershellQ( cmd );

                return (int)ExitCode.Success;

            }

            /*
                Certutil > Delete
            */

            if ( args[ 0 ] == "--delete" || args[ 0 ] == "-d" )
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
                    Console.Write( $"        {assemblyName} --delete <VS_ID_CONTAINER>");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    Console.WriteLine( );

                    return (int)ExitCode.ErrorMissingArg;
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
                    Console.WriteLine( );

                    return (int)ExitCode.ErrorMissingDep;
                }

                string arg_container_id     = args[ 1 ];
                string ps_executecmd        = "certutil -delkey -csp \"Microsoft Strong Cryptographic Provider\" \""+ arg_container_id + "\"";

                PowershellQ( ps_executecmd );

                return (int)ExitCode.Success;
            }

            /*
                Certutil > Delete All
            */

            if ( args[ 0 ] == "--deleteall" || args[ 0 ] == "-x" )
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

                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine( " Are you sure you want to delete all containers and associated keypairs?" );
                Console.WriteLine( " You cannot undo this action!!!!" );
                Console.WriteLine( );

                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write( " Are you sure? [ Y | N ]:  " );
                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.Flush( );

                ConsoleKeyInfo yesNo = Console.ReadKey( true );

                /*
                    Delete > Confirmation
                */

                if ( yesNo.Key == ConsoleKey.N )
                {
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine( " Aborted certificate deletion. No changes made." );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    
                    return (int)ExitCode.Abort;
                }

                string ps_executecmd ="certutil -csp \"Microsoft Strong Cryptographic Provider\" -key | Select-String -Pattern \"VS_KEY\" | %{ $_.ToString().Trim()} | %{ certutil -delkey -csp \"Microsoft Strong Cryptographic Provider\" $_}";
                PowershellQ( ps_executecmd );

                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine( " Successfully deleted all keypairs and associated containers." );
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine( );

                return (int)ExitCode.Success;
            }

            /*
                Certutil > Sign
            */

            if ( args[ 0 ] == "--sign" || args[ 0 ] == "-s" )
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
                    Console.WriteLine( );

                    return (int)ExitCode.ErrorMissingDep;
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
                    Console.Write( $"        {assemblyName} --sign <PFX_THUMBPRINT> <FILE_TO_SIGN>");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    Console.WriteLine( );

                    return (int)ExitCode.ErrorMissingArg;
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
                    Console.Write( $"        {assemblyName} --sign <PFX_THUMBPRINT> <FILE_TO_SIGN>");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    Console.WriteLine( );

                    return (int)ExitCode.ErrorMissingArg;
                }

                string arg_file_tosign      = args[ 2 ];
                string ps_executecmd        = "signtool sign /sha1 \"" + arg_pfx_thumbprint + "\" /fd SHA256 /t http://timestamp.comodoca.com/authenticode \"" + arg_file_tosign + "\"";
 
                PowershellQ( ps_executecmd );

                return (int)ExitCode.Success;
            }

            /*
                Install
            */

            if ( args[ 0 ] == "--install" || args[ 0 ] == "-i" )
            {

                /*
                    Arg > Missing pfx file
                */

                if ( args.Length == 1 )
                {
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write( " Error: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( "Must specify " );
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write( "PFX_FILE" );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write( $"        {assemblyName} --install <PFX_FILE>");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );
                    Console.WriteLine( );

                    return (int)ExitCode.ErrorMissingArg;
                }

                /*
                    assign > pfx path
                */

                string arg_pfx_path         = args[ 1 ];
                string arg_pfx_container_id = args.Length == 4 ? args[ 3 ] : ResolveKeySourceTask.ResolveAssemblyKey( arg_pfx_path );

                /*
                    Displays additional information
                */

                if ( !String.IsNullOrEmpty( arg_pfx_container_id ) )
                {
                    Console.WriteLine( );
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine( " Located Key and Container: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( );

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write( " Container:       " );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( arg_pfx_container_id );

                    Console.WriteLine( );

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write( " Installed:       " );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( ResolveKeySourceTask.IsContainerInstalled( arg_pfx_container_id ) );

                    Console.WriteLine( );
                    Console.WriteLine( );
                }

                /*
                    Arg > pfx password
                */

                if ( args.Length == 2 )
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write( " Error: " );

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( "Missing additional arguments" );
                    Console.WriteLine( );
                    Console.WriteLine( "        <pfx_passwd>           PFX certificate password" );
                    Console.WriteLine( "        <vs_id_container>      Visual Studio container ID" );
                    Console.WriteLine( );
                    Console.WriteLine( );

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine( " Install PFX file" );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( $"        {assemblyName} --install <PFX_FILE> <PFX_PASSWD>");

                    Console.WriteLine( );
                    Console.WriteLine( );

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine( " Install PFX file under container" );
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write( $"        {assemblyName} --install <PFX_FILE> <PFX_PASSWD> <VS_ID_CONTAINER>");

                    Console.WriteLine( );
                    Console.WriteLine( );

                    return (int)ExitCode.ErrorMissingArg;
                }

                /*
                    check if specified pfx key already exists in the strong name container
                */

                if ( ResolveKeySourceTask.IsContainerInstalled( arg_pfx_container_id ))
                {
                    Console.WriteLine( );
                    Console.Error.WriteLine( "Keypair already installed in strong name CSP container: " + arg_pfx_container_id + "." );
                    Console.WriteLine( );
                    Console.Error.WriteLine( "To delete the key container run following command from the Developer Command Prompt:");
                    Console.Error.WriteLine( $"     {assemblyName} --delete " + arg_pfx_container_id );
                    Console.Error.WriteLine( );
                    Console.Error.WriteLine( "To list all installed key containers run the command:" );
                    Console.Error.WriteLine( $"     {assemblyName} --list" );

                    return (int)ExitCode.Abort;
                }

                /*
                    assign > pfx password
                */

                string arg_pfx_passwd = args[ 2 ];


                /*
                    open pfx and export private key
                */

                var pfx_cert        = new X509Certificate2( arg_pfx_path, arg_pfx_passwd, X509KeyStorageFlags.Exportable );
                var pfx_key_priv    = pfx_cert.PrivateKey as RSACryptoServiceProvider;
                var pfx_csp         = pfx_key_priv.ExportCspBlob( true );

                /*
                    Create Cryptographic Service Provider and register key container
                */

                const string DotNetStrongSigningCSP = "Microsoft Strong Cryptographic Provider";
                var cspParams = new CspParameters( 1, DotNetStrongSigningCSP, arg_pfx_container_id )
                {
                    KeyNumber   = (int)KeyNumber.Signature,
                    Flags       = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseNonExportableKey
                };

                try
                {
                    using ( var RsaCSP = new RSACryptoServiceProvider( cspParams ) )
                    {
                        RsaCSP.PersistKeyInCsp = true;
                        RsaCSP.ImportCspBlob( pfx_csp );
                    };
                }
                catch ( CryptographicException e )
                {
                    if ( !e.Message.Contains( "Container already exists." ) )
                    {
                        throw;
                    }

                    Console.Error.WriteLine( $"An error occurred while attempting to create the strong name CSP key container '{arg_pfx_container_id}'.");
                    Console.Error.WriteLine( "The ontainer may have been created by another user.");
                    Console.Error.WriteLine( "Modify the container to be readable by everyone, or delete the container with the following command from the Developer Command Prompt:");
                    Console.Error.WriteLine( $"     {assemblyName} --delete {arg_pfx_container_id}" );
                    Console.Error.WriteLine( $"     sn.exe -d {arg_pfx_container_id}" );

                    return (int)ExitCode.ErrorGeneric;
                }

                /*
                    Successful creation
                */

                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write( " Success: " );

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write( "Keypair was successfully installed into strong name CSP key container:" );
                Console.WriteLine( );
                Console.WriteLine( );
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write( " Container:       " );
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write( arg_pfx_container_id );

                Console.WriteLine( );

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write( " Keyfile:         " );
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write( arg_pfx_path );

                Console.WriteLine( );
                Console.WriteLine( );

                return (int)ExitCode.Success;
            }

            /*
                User specified no args
            */

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write( " Error: " );

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write( "Unknown command" );
            Console.WriteLine( );

            return (int)ExitCode.None;

        }

    }


}


