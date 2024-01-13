using certmgr;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MSBuildCode;
using System.Management.Automation;
using System.ComponentModel;
using System.Diagnostics;
using Lng = certmgr.Properties.Resource;
using Cfg = certmgr.Properties.Settings;

namespace CertMgr
{

    public class CertMgr
    {

        /*
            Define > Classes
        */

        private AppInfo AppInfo                 = new AppInfo( );
        readonly static Action<string> wl       = Console.WriteLine;
        readonly static Action<string> ws       = Console.Write;
        static string assemblyName              = Assembly.GetEntryAssembly( ).GetName( ).Name;

        /*
            Define > Console fg
        */

        static public void fg( ConsoleColor clr )
        {
            Console.ForegroundColor = clr;
        }

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
                        wl( PSItem.ToString( ) );
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
                    p.StartInfo.UseShellExecute         = false;
                    p.StartInfo.FileName                = "where";
                    p.StartInfo.Arguments               = exeName;
                    p.StartInfo.RedirectStandardOutput  = true;
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
            Help Dialog
                default if no arguments are specified by user
        */

        static public int Help( )
        {
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.Red );
                wl( "" );
                wl( " " + AppInfo.Title + " " );
                wl( "" );
                wl( " @author        : " + AppInfo.Company + " " );
                wl( " @version       : " + AppInfo.PublishVersion + " " );
                wl( " @copyright     : " + AppInfo.Copyright + " " );
                wl( " @website       : https://github.com/aetherinox/certmgr" );
                wl( "" );
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.Gray );
                wl( "" );
                wl(@" certmgr.exe is a replacement for:
   sn.exe                   Strong Name Tool
   signtool.exe             Sign Tool
   certutil.exe             Certificate Services & Management
    
 This utility helps sign assemblies with strong names. It provides options 
 for key management, signature generation, and signature verification. It
 should be used in combination with Visual Studio and utilized when you
 want to sign a project, or change the PFX keyfile used to sign a project.
                ");

                wl( " Commands:" );
                fg( ConsoleColor.Red );
                ws( $"   -n, --info               ");
                fg( ConsoleColor.Gray );
                ws( "Show information about a specified pfx keyfile" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   -l, --list               ");
                fg( ConsoleColor.Gray );
                ws( "List all certificates" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   -i, --install            ");
                fg( ConsoleColor.Gray );
                ws( "Install PFX keyfile and register with container" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   -s, --sign               ");
                fg( ConsoleColor.Gray );
                ws( "Sign binary / dll with key using signtool" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   -d, --delete             ");
                fg( ConsoleColor.Gray );
                ws( "Delete key for specified container id" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   -x, --deleteall          ");
                fg( ConsoleColor.Gray );
                ws( "Delete all keys for all containers" );

                wl( "" );
                wl( "" );

                wl( " Examples:" );
                fg( ConsoleColor.Red );
                ws( $"   {assemblyName} -n <PFX_FILE>                            ");
                fg( ConsoleColor.Gray );
                ws( "Show information about the pfx_infile" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   {assemblyName} -i <PFX_FILE> <PFX_PASSWD>               ");
                fg( ConsoleColor.Gray );
                ws( "Install PFX keyfile" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   {assemblyName} -i <PFX_FILE> <PFX_PASSWD> <VS_KEY_ID>   ");
                fg( ConsoleColor.Gray );
                ws( "Install PFX keyfile for VS project container" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   {assemblyName} -l                                       ");
                fg( ConsoleColor.Gray );
                ws( "List all certificates" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   {assemblyName} -d <VS_KEY_ID>                           ");
                fg( ConsoleColor.Gray );
                ws( "Delete key for specified container id" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   {assemblyName} -x                                       ");
                fg( ConsoleColor.Gray );
                ws( "Delete all keys for all containers" );

                wl( "" );

                fg( ConsoleColor.Red );
                ws( $"   {assemblyName} -s <PFX_THUMBPRINT> <FILE_TO_SIGN>       ");
                fg( ConsoleColor.Gray );
                ws( "Sign binary / dll with key using signtool" );

                wl( "" );
                wl( "" );
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.DarkGray );
                wl( "" );
                Console.ResetColor( ) ;

            return (int)ExitCode.None;
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
                Help( );
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
                    wl( "" );
                    fg( ConsoleColor.Red );
                    ws( " Error: " );

                    fg( ConsoleColor.White );
                    ws( "Missing " );
                    fg( ConsoleColor.Green );
                    ws( "pfx certificate file" );
                    fg( ConsoleColor.White );
                    wl( "" );
                    fg( ConsoleColor.DarkGray );
                    ws( $"        {assemblyName} --info <PFX_FILE>");
                    fg( ConsoleColor.White );
                    wl( "" );
                    wl( "" );

                    return (int)ExitCode.ErrorMissingArg;
                }

                string arg_pfx_path             = args[ 1 ];
                string arg_pfx_container_id     = ResolveKeySourceTask.ResolveAssemblyKey( arg_pfx_path );

                wl( arg_pfx_container_id );
                wl( $"Installed: {ResolveKeySourceTask.IsContainerInstalled( arg_pfx_container_id )}" );

                return (int)ExitCode.Success;
            }

            /*
                List
                    Utilize certutil to list all keys
            */

            if ( args[ 0 ] == "--list" || args[ 0 ] == "-l" )
            {

                wl( "" );
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.Red );
                wl( $"   { AppInfo.Title } : List");
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.White );

                /*
                    Check for Certutil
                        This should never happen, but just in case.
                */

                bool bCertUtil = ExistsOnPath( "certutil" );

                if ( !bCertUtil )
                {
                    wl( "" );
                    fg( ConsoleColor.Red );
                    ws( " Error: " );

                    fg( ConsoleColor.White );
                    ws( "Could not find the program" );
                    fg( ConsoleColor.Green );
                    ws( " Certutil.exe" );
                    wl( "" );
                    fg( ConsoleColor.White );
                    wl( " Certutil must be in your Windows PATH environment variables list." );
                    wl( "" );
                    ws( " Check " );
                    fg( ConsoleColor.Green );
                    ws( @"C:\Windows\System32\certutil.exe" );
                    fg( ConsoleColor.White );
                    wl( "" );
                    wl( "" );

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
                    wl( "" );
                    fg( ConsoleColor.Red );
                    ws( " Error: " );

                    fg( ConsoleColor.White );
                    ws( "Must specify " );
                    fg( ConsoleColor.Green );
                    ws( "VS_ID_CONTAINER" );
                    fg( ConsoleColor.White );
                    wl( "" );
                    fg( ConsoleColor.DarkGray );
                    ws( $"        {assemblyName} --delete <VS_ID_CONTAINER>");
                    fg( ConsoleColor.White );
                    wl( "" );
                    wl( "" );

                    return (int)ExitCode.ErrorMissingArg;
                }

                wl( "" );
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.Red );
                wl( $"   { AppInfo.Title } : Delete");
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.White );

                /*
                    Check for Certutil
                        This should never happen, but just in case.
                */

                bool bCertUtil = ExistsOnPath( "certutil" );

                if ( !bCertUtil )
                {
                    wl( "" );
                    fg( ConsoleColor.Red );
                    ws( " Error: " );

                    fg( ConsoleColor.White );
                    ws( "Could not find the program" );
                    fg( ConsoleColor.Green );
                    ws( " Certutil.exe" );
                    wl( "" );
                    fg( ConsoleColor.White );
                    wl( " Certutil must be in your Windows PATH environment variables list." );
                    wl( "" );
                    ws( " Check " );
                    fg( ConsoleColor.Green );
                    ws( @"C:\Windows\System32\certutil.exe" );
                    fg( ConsoleColor.White );
                    wl( "" );
                    wl( "" );

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
                wl( "" );
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.Red );
                wl( $"   { AppInfo.Title } : Delete All");
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.White );

                wl( "" );
                fg( ConsoleColor.White );
                wl( " Are you sure you want to delete all containers and associated keypairs?" );
                wl( " You cannot undo this action!!!!" );
                wl( "" );

                wl( "" );
                fg( ConsoleColor.Green );
                ws( " Are you sure? [ Y | N ]:  " );
                fg( ConsoleColor.White );
                Console.Out.Flush( );

                ConsoleKeyInfo yesNo = Console.ReadKey( true );

                /*
                    Delete > Confirmation
                */

                if ( yesNo.Key == ConsoleKey.N )
                {
                    wl( "" );
                    fg( ConsoleColor.Red );
                    wl( " Aborted certificate deletion. No changes made." );
                    fg( ConsoleColor.White );
                    wl( "" );
                    
                    return (int)ExitCode.Abort;
                }

                string ps_executecmd ="certutil -csp \"Microsoft Strong Cryptographic Provider\" -key | Select-String -Pattern \"VS_KEY\" | %{ $_.ToString().Trim()} | %{ certutil -delkey -csp \"Microsoft Strong Cryptographic Provider\" $_}";
                PowershellQ( ps_executecmd );

                wl( "" );
                fg( ConsoleColor.Yellow );
                wl( " Successfully deleted all keypairs and associated containers." );
                fg( ConsoleColor.White );
                wl( "" );

                return (int)ExitCode.Success;
            }

            /*
                Certutil > Sign
            */

            if ( args[ 0 ] == "--sign" || args[ 0 ] == "-s" )
            {
                wl( "" );
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.Red );
                wl( $"   { AppInfo.Title } : Sign" );
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.White );

                /*
                    Check for Signtool
                */

                bool bSignTool = ExistsOnPath( "signtool" );

                if ( !bSignTool )
                {
                    wl( "" );
                    fg( ConsoleColor.Red );
                    ws( " Error: " );

                    fg( ConsoleColor.White );
                    ws( "Could not find the program" );
                    fg( ConsoleColor.Green );
                    ws( " signtool.exe" );
                    wl( "" );
                    fg( ConsoleColor.White );
                    wl( " signtool.exe must be in your Windows PATH environment variables list." );
                    wl( "" );
                    wl( "" );

                    return (int)ExitCode.ErrorMissingDep;
                }

                /*
                    Args > Missing Thumbprint
                */

                if ( args.Length == 1 )
                {
                    wl( "" );
                    fg( ConsoleColor.Red );
                    ws( " Error: " );

                    fg( ConsoleColor.White );
                    ws( "Missing certificate thumbprint " );
                    fg( ConsoleColor.Green );
                    ws( "PFX_THUMBPRINT" );
                    fg( ConsoleColor.White );
                    wl( "" );
                    fg( ConsoleColor.DarkGray );
                    ws( $"        {assemblyName} --sign <PFX_THUMBPRINT> <FILE_TO_SIGN>");
                    fg( ConsoleColor.White );
                    wl( "" );
                    wl( "" );

                    return (int)ExitCode.ErrorMissingArg;
                }

                string arg_pfx_thumbprint   = args[ 1 ];

                /*
                    Args > File to Sign
                */

                if ( args.Length == 2 )
                {
                    wl( "" );
                    fg( ConsoleColor.Red );
                    ws( " Error: " );

                    fg( ConsoleColor.White );
                    ws( "Missing binary to sign " );
                    fg( ConsoleColor.Green );
                    ws( "FILE_TO_SIGN" );
                    fg( ConsoleColor.White );
                    wl( "" );
                    fg( ConsoleColor.DarkGray );
                    ws( $"        {assemblyName} --sign <PFX_THUMBPRINT> <FILE_TO_SIGN>");
                    fg( ConsoleColor.White );
                    wl( "" );
                    wl( "" );

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
                    wl( "" );
                    fg( ConsoleColor.Red );
                    ws( " Error: " );

                    fg( ConsoleColor.White );
                    ws( "Must specify " );
                    fg( ConsoleColor.Green );
                    ws( "PFX_FILE" );
                    fg( ConsoleColor.White );
                    wl( "" );
                    fg( ConsoleColor.DarkGray );
                    ws( $"        {assemblyName} --install <PFX_FILE>");
                    fg( ConsoleColor.White );
                    wl( "" );
                    wl( "" );

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
                    wl( "" );
                    fg( ConsoleColor.Blue );
                    wl( " Located Key and Container: " );

                    fg( ConsoleColor.White );
                    wl( "" );

                    fg( ConsoleColor.DarkGray );
                    ws( " Container:       " );
                    fg( ConsoleColor.White );
                    ws( arg_pfx_container_id );

                    wl( "" );

                    fg( ConsoleColor.DarkGray );
                    ws( " Installed:       " );
                    fg( ConsoleColor.White );
                    ws( ResolveKeySourceTask.IsContainerInstalled( arg_pfx_container_id ).ToString( ) );

                    wl( "" );
                    wl( "" );
                }

                /*
                    Arg > pfx password
                */

                if ( args.Length == 2 )
                {

                    fg( ConsoleColor.Red );
                    ws( " Error: " );

                    fg( ConsoleColor.White );
                    ws( "Missing additional arguments" );
                    wl( "" );
                    wl( "        <pfx_passwd>           PFX certificate password" );
                    wl( "        <vs_id_container>      Visual Studio container ID" );
                    wl( "" );
                    wl( "" );

                    fg( ConsoleColor.DarkGray );
                    wl( " Install PFX file" );
                    fg( ConsoleColor.White );
                    ws( $"        {assemblyName} --install <PFX_FILE> <PFX_PASSWD>");

                    wl( "" );
                    wl( "" );

                    fg( ConsoleColor.DarkGray );
                    wl( " Install PFX file under container" );
                    fg( ConsoleColor.White );
                    ws( $"        {assemblyName} --install <PFX_FILE> <PFX_PASSWD> <VS_ID_CONTAINER>");

                    wl( "" );
                    wl( "" );

                    return (int)ExitCode.ErrorMissingArg;
                }

                /*
                    check if specified pfx key already exists in the strong name container
                */

                if ( ResolveKeySourceTask.IsContainerInstalled( arg_pfx_container_id ))
                {
                    wl( "" );
                    wl( "Keypair already installed in strong name CSP container: " + arg_pfx_container_id + "." );
                    wl( "" );
                    wl( "To delete the key container run following command from the Developer Command Prompt:");
                    wl( $"     {assemblyName} --delete " + arg_pfx_container_id );
                    wl( "" );
                    wl( "To list all installed key containers run the command:" );
                    wl( $"     {assemblyName} --list" );

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

                const string CspCrypto = "Microsoft Strong Cryptographic Provider";
                var CspParams = new CspParameters( 1, CspCrypto, arg_pfx_container_id )
                {
                    KeyNumber   = (int)KeyNumber.Signature,
                    Flags       = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseNonExportableKey
                };

                try
                {
                    using ( var CspRSA = new RSACryptoServiceProvider( CspParams ) )
                    {
                        CspRSA.PersistKeyInCsp = true;
                        CspRSA.ImportCspBlob( pfx_csp );
                    };
                }
                catch ( CryptographicException e )
                {
                    if ( !e.Message.Contains( "Container already exists." ) )
                    {
                        throw;
                    }

                    wl( $"An error occurred while attempting to create the strong name CSP key container '{arg_pfx_container_id}'.");
                    wl( "The ontainer may have been created by another user.");
                    wl( "Modify the container to be readable by everyone, or delete the container with the following command from the Developer Command Prompt:");
                    wl( $"     {assemblyName} --delete {arg_pfx_container_id}" );
                    wl( $"     sn.exe -d {arg_pfx_container_id}" );

                    return (int)ExitCode.ErrorGeneric;
                }

                /*
                    Successful creation
                */

                wl( "" );
                fg( ConsoleColor.Green );
                ws( " Success: " );

                fg( ConsoleColor.White );
                ws( "Keypair was successfully installed into strong name CSP key container:" );
                wl( "" );
                wl( "" );
                fg( ConsoleColor.DarkGray );
                ws( " Container:       " );
                fg( ConsoleColor.White );
                ws( arg_pfx_container_id );

                wl( "" );

                fg( ConsoleColor.DarkGray );
                ws( " Keyfile:         " );
                fg( ConsoleColor.White );
                ws( arg_pfx_path );

                wl( "" );
                wl( "" );

                return (int)ExitCode.Success;
            }

            /*
                User specified no args
            */

            fg( ConsoleColor.Red );
            ws( " Error: " );

            fg( ConsoleColor.White );
            ws( "Unknown command" );
            wl( "" );

            return (int)ExitCode.None;

        }

    }


}


