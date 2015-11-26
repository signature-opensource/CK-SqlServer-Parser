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
    [AddPath("CodeCakeBuilder/Tools")]
    public class Build : CodeCakeHost
    {
        public Build()
        {
            string configuration = null;
            var nugetOutputDir = Cake.Directory( "CodeCakeBuilder/Release" );
            SimpleRepositoryInfo gitInfo = null;

            Task( "Check-Repository" )
                .Does( () =>
                {
                    gitInfo = Cake.GetSimpleRepositoryInfo();
                    if( !gitInfo.IsValid ) throw new Exception( "SimpleGitVersionInfo: This solution is not ready for publishing." );
                    else
                    {
                        if( gitInfo.IsValidRelease )
                        {
                            configuration = "Release";
                        }
                        else
                        {
                            configuration = "Debug";
                        }
                        Cake.Log.Information( "Packages in version '{0}' can be published.", gitInfo.NuGetVersion );
                    }
                } );

            Task( "Clean" )
                .IsDependentOn( "Check-Repository" )
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
                    Cake.NuGetRestore( "CK-SqlServer-Parser.sln" );
                } );

            Task( "Build" )
                .IsDependentOn( "Check-Repository" )
                .IsDependentOn( "Restore-NuGet-Packages" )
                .Does( () =>
                {
                    Cake.MSBuild( "CK.SqlServer.Parser/CK.SqlServer.Parser.csproj", new MSBuildSettings()
                        .UseToolVersion( MSBuildToolVersion.NET45 )
                        .SetVerbosity( Verbosity.Normal )
                        .SetConfiguration( configuration )
                        .SetNodeReuse( false ) );
                } );

            Task( "Create-NuGet-Package" )
                .IsDependentOn( "Build" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    Cake.CreateDirectory( nugetOutputDir );
                    Cake.NuGetPack( "CodeCakeBuilder/CK.SqlServer.Parser.nuspec", new NuGetPackSettings()
                    {
                        Version = gitInfo.NuGetVersion,
                        BasePath = Cake.Environment.WorkingDirectory,
                        OutputDirectory = nugetOutputDir,
                    } );
                } );

            Task( "Publish-NuGet-Package" )
                .IsDependentOn( "Create-NuGet-Package" )
                .Does( () =>
                {
                    var settings = new NuGetPushSettings()
                    {
                        ApiKey = Cake.InteractiveEnvironmentVariable( "NUGET_API_KEY" ),
                        Verbosity = NuGetVerbosity.Normal,
                        Source = "https://www.nuget.org/api/v2/package"
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
