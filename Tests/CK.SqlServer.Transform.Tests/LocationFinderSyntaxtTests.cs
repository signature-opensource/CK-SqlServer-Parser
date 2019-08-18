using CK.SqlServer.Parser;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.SqlServer.Transform.Tests
{
    [TestFixture]
    public class LocationFinderSyntaxtTests
    {
        [Test]
        public void all_and_each_with_out_of()
        {
            var a = new SqlAnalyser();

            // Naked.
            a.Reset( "each {token}" );
            a.IsSqlTLocationFinder( true ).Should().NotBeNull();

            a.Reset( "all {token}" );
            a.IsSqlTLocationFinder( true ).Should().NotBeNull();

            // With 'N'.
            a.Reset( "all 4 {token}" );
            a.IsSqlTLocationFinder( true ).Should().NotBeNull();

            a.Reset( "each 90 {token}" );
            a.IsSqlTLocationFinder( true ).Should().NotBeNull();

            // With 'out of N'.
            a.Reset( "all out of 4 {token}" );
            a.IsSqlTLocationFinder( true ).Should().NotBeNull();

            a.Reset( "each out of 90 {token}" );
            a.IsSqlTLocationFinder( true ).Should().NotBeNull();

            // With 'N out of N': the two N must be the same!
            a.Reset( "all 1 out of 4" );
            a.IsSqlTLocationFinder( true ).Should().BeNull();
            a.GetCurrentResult().ErrorMessage.Should().Contain( "followed by 'N out of N'" );

            a.Reset( "all 4 out of 4 {token}" );
            a.IsSqlTLocationFinder( true ).Should().NotBeNull();

            a.Reset( "each 90 out of 4" );
            a.IsSqlTLocationFinder( true ).Should().BeNull();
            a.GetCurrentResult().ErrorMessage.Should().Contain( "followed by 'N out of N'" );

            a.Reset( "each 90 out of 90 {token}" );
            a.IsSqlTLocationFinder( true ).Should().NotBeNull();

        }
    }
}
