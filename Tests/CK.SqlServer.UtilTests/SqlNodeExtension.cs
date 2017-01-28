using CK.SqlServer.Parser;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace CK.SqlServer.UtilTests
{
    public static class SqlNodeExtension
    {

        public static XElement ToXml( this ISqlNode @this, string name = "Sql" )
        {
            if( @this is SqlToken ) return null;

            var props = @this.GetType().GetProperties()
                                .Where( p => p.Name != "UnPar" )
                                .Where( p => typeof( ISqlNode ).IsAssignableFrom( p.PropertyType )
                                                && p.GetIndexParameters().Length == 0 )
                                .Select( p => new { Name = p.Name, Value = (ISqlNode)p.GetValue( @this ) } )
                                .Where( o => o.Value != null );

            return new XElement( name, 
                        new XAttribute( "Type", @this.GetType().Name.Replace( '`', '_' ) ),
                        new XElement( "T", @this.ToString() ),
                        props.Select( o => ToXml( o.Value, o.Name ) ) );
        }

        /// <summary>
        /// Writes an <see cref="IEnumerable"/> of <see cref="SqlNode"/> without its trivias. 
        /// Calls <see cref="SqlNode.WriteWithoutTrivias(StringBuilder)"/> on each token.
        /// </summary>
        /// <param name="this">An IEnumerable of SqlNode.</param>
        /// <param name="separator">Separator between tokens.</param>
        /// <param name="b">StringBuilder to write into.</param>
        public static StringBuilder WriteWithoutTrivias(
            this IEnumerable<SqlNode> @this,
            string separator, StringBuilder b )
        {
            bool one = false;
            foreach( SqlNode t in @this )
            {
                if( one ) b.Append( separator );
                one = true;
                b.Append( t.ToString() );
            }
            return b;
        }

        /// <summary>
        /// Returns a string for an <see cref="IEnumerable"/> of <see cref="SqlNode"/> without its trivias. 
        /// Calls <see cref="SqlNode.WriteWithoutTrivias(StringBuilder)"/> on each node.
        /// </summary>
        /// <param name="this">An IEnumerable of SqlNode.</param>
        /// <param name="separator">Separator between nodes.</param>
        /// <returns>Tokens without trivias.</returns>
        public static string ToStringWithoutTrivias( this IEnumerable<SqlNode> @this, string separator )
        {
            StringBuilder b = new StringBuilder();
            @this.WriteWithoutTrivias( separator, b );
            return b.ToString();
        }

    }
}
