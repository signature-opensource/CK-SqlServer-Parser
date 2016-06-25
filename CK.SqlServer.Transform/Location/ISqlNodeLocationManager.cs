using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Exposes everything needed to manage location of nodes below a <see cref="Root"/>.
    /// </summary>
    public interface ISqlNodeLocationManager
    {
        /// <summary>
        /// Gets the root location.
        /// </summary>
        SqlNodeLocation Root { get; }

        /// <summary>
        /// The location before this root. Its <see cref="Node"/> is <see cref="SqlKeyword.BegOfInput"/>
        /// and its <see cref="Position"/> is -1.
        /// </summary>
        SqlNodeLocation BegMarker { get; }

        /// <summary>
        /// The location after this root: its <see cref="Node"/> is <see cref="SqlKeyword.EndOfInput"/> and 
        /// its <see cref="Position"/> is equal to the root node's width.
        /// </summary>
        SqlNodeLocation EndMarker { get; }

        /// <summary>
        /// Gets a raw location object for the position.
        /// If a full location can efficiently be retrieved, the returned location may be a full one. 
        /// </summary>
        /// <param name="position">The position of the node.</param>
        /// <returns>A location.</returns>
        SqlNodeLocation GetRawLocation( int position );

        /// <summary>
        /// Gets the full location for the position.
        /// </summary>
        /// <param name="position">The position of the node.</param>
        /// <returns>A full location.</returns>
        SqlNodeLocation GetFullLocation( int position );

        /// <summary>
        /// Returns the qualified location of a node for the given position.
        /// The node must exist at this position otherwise an <see cref="ArgumentException"/> is thrown. 
        /// </summary>
        /// <param name="position">The position of the node.</param>
        /// <param name="node">The node that must exist at the given position.</param>
        /// <returns>The qualified location.</returns>
        SqlNodeLocation GetQualifiedLocation( int position, ISqlNode node );
    }

}
