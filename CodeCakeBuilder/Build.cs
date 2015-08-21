using System;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.NuGet;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.SignTool;
using Cake.Core;
using Cake.Core.Diagnostics;
using Code.Cake;
using SimpleGitVersion;
using Cake.Common.Tools.NuGet.Pack;
using System.Collections.Generic;
using Cake.Common.Tools.NuGet.Push;
using Cake.Common.Tools.NuGet.Restore;

namespace CodeCake
{
    public class Build : CodeCakeHost
    {
        public Build()
        {
            var configuration = Cake.Argument( "configuration", "Release" );
            var securePath = Cake.Argument( "securePath", "../../_Secure" );
            var secureDir = Cake.Directory( securePath );

            var nugetExe = Cake.File( Environment.ExpandEnvironmentVariables( "%LOCALAPPDATA%/NuGet/NuGet.exe" ) );
            if( !Cake.FileExists( nugetExe ) ) throw new Exception( "Unable to find nuget.exe: " + nugetExe );

            var nugetOutputDir = Cake.Directory( "CodeCakeBuilder/Release" );
            SimpleRepositoryInfo gitInfo = null;
            SignToolSignSettings signSettingsForRelease = null;

            Task( "Clean" )
                .Does( () =>
                {
                    Cake.CleanDirectory( Cake.Directory( "CK.SqlServer.Parser/bin" ) + Cake.Directory( configuration ) );
                    Cake.CleanDirectory( Cake.Directory( "CK.SqlServer.Parser/obj" ) + Cake.Directory( configuration ) );
                    Cake.CleanDirectory( nugetOutputDir );
                } );

            Task( "Restore-NuGet-Packages" )
                .IsDependentOn( "Clean" )
                .Does( () =>
                {
                    Cake.NuGetRestore( "CK-SqlServer-Parser.sln", new NuGetRestoreSettings() { ToolPath = nugetExe } );
                } );

            Task( "Build" )
                .IsDependentOn( "Check-Publish" )
                .IsDependentOn( "Restore-NuGet-Packages" )
                .Does( () =>
                {
                    Cake.MSBuild( "CK.SqlServer.Parser/CK.SqlServer.Parser.csproj", new MSBuildSettings()
                        .UseToolVersion( MSBuildToolVersion.NET45 )
                        .SetVerbosity( Verbosity.Normal )
                        .SetConfiguration( configuration )
                        .SetNodeReuse( false ) );
                } );

            Task( "Check-Publish" )
                .Does( () =>
                {
                    gitInfo = Cake.GetSimpleRepositoryInfo();
                    if( !gitInfo.IsValid ) throw new Exception( "SimpleGitVersionInfo: This solution is not ready for publishing." );
                    else if( !Cake.DirectoryExists( secureDir ) ) throw new Exception( String.Format( "SecurePath '{0}' not found.", secureDir ) );
                    else
                    {
                        // If the release is a not a CI build, we must sign the artifacts before packaging.
                        if( gitInfo.IsValidRelease )
                        {
                            if( configuration != "Release" ) throw new Exception( "A release version must be published in 'Release' configuration!" );
                            signSettingsForRelease = new SignToolSignSettings()
                            {
                                TimeStampUri = new Uri( "http://timestamp.verisign.com/scripts/timstamp.dll" ),
                                CertPath = secureDir + Cake.File( "Invenietis-Authenticode.pfx" ),
                                Password = System.IO.File.ReadAllText( secureDir + Cake.File( "Invenietis-Authenticode.p.txt" ) )
                            };
                        }
                        Cake.Log.Information( "Packages in version '{0}' can be published.", gitInfo.NuGetVersion );
                    }
                } );

            Task( "Sign-Authenticode" )
                .IsDependentOn( "Build" )
                .WithCriteria( () => signSettingsForRelease != null )
                .Does( () =>
                {
                    Cake.Sign( "CK.SqlServer.Parser/bin/Release/CK.SqlServer.Parser.dll", signSettingsForRelease );
                } );

            Task( "Create-NuGet-Package" )
                .IsDependentOn( "Build" )
                .IsDependentOn( "Check-Publish" )
                .IsDependentOn( "Sign-Authenticode" )
                .Does( () =>
                {
                    Cake.CreateDirectory( nugetOutputDir );
                    Cake.NuGetPack( "CodeCakeBuilder/CK.SqlServer.Parser.nuspec", new NuGetPackSettings()
                    {
                        Version = gitInfo.NuGetVersion,
                        BasePath = Cake.Environment.WorkingDirectory,
                        OutputDirectory = nugetOutputDir,
                        ToolPath = nugetExe
                    } );
                } );

            Task( "Publish-NuGet-Package" )
                .IsDependentOn( "Create-NuGet-Package" )
                .Does( () =>
                {
                    var settings = new NuGetPushSettings()
                    {
                        ApiKey = System.IO.File.ReadAllText( secureDir + Cake.File( "NuGet-Push-ApiKey.txt" ) ),
                        Verbosity = NuGetVerbosity.Detailed,
                        Source = "http://proget.app.invenietis.net/nuget/Default",
                        ToolPath = nugetExe
                    };
                    foreach( var f in Cake.GetFiles( nugetOutputDir.Path.FullPath + "/*.nupkg" ) )
                    {
                        Cake.NuGetPush( f, settings );
                    }
                } );

            Task( "Default" ).IsDependentOn( "Publish-NuGet-Package" );
        }
    }
}
