using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// This context is available on <see cref="SqlNodeLocationVisitor.VisitContext"/> property.
    /// </summary>
    public interface IVisitContext : IVisitContextBase
    {
        /// <summary>
        /// Gets the visited node.
        /// </summary>
        ISqlNode VisitedNode { get; }

        /// <summary>
        /// Gets or sets an optional object to the visited node.
        /// This can be used to transfer information between <see cref="SqlNodeLocationVisitor.BeforeVisitItem"/>
        /// and <see cref="SqlNodeLocationVisitor.AfterVisitItem"/> for a node.
        /// </summary>
        object Tag { get; set; }

        /// <summary>
        /// Gets the range filter status for the <see cref="VisitedNode"/>.
        /// </summary>
        VisitedNodeRangeFilterStatus RangeFilterStatus { get; }

        /// <summary>
        /// Gets the current depth.
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Gets the current position.
        /// </summary>
        int Position { get; }

        /// <summary>
        /// Obtains the location of the currently visited node.
        /// When no nodes are being visited, the root is returned.
        /// </summary>
        /// <param name="qualifiedLocation">True to force the obtention of a qualified location.</param>
        /// <returns>A (potentially qualified) location.</returns>
        SqlNodeLocation GetCurrentLocation( bool ensureQualifiedLocation = false );

    }
}
