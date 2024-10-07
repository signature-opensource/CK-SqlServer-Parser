using CK.SqlServer.Parser;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.SqlServer.Transform.Tests;

[TestFixture]
public class LocationFinderSyntaxtTests
{
    [Test]
    public void all_and_each_with_out_of()
    {
        var a = new SqlAnalyser();

        // Naked.
        a.Reset( "each {token}" );
        a.IsISqlTLocationFinder( true ).Should().NotBeNull();

        a.Reset( "all {token}" );
        a.IsISqlTLocationFinder( true ).Should().NotBeNull();

        // With 'N'.
        a.Reset( "all 4 {token}" );
        a.IsISqlTLocationFinder( true ).Should().NotBeNull();

        a.Reset( "each 90 {token}" );
        a.IsISqlTLocationFinder( true ).Should().NotBeNull();

    }
}
