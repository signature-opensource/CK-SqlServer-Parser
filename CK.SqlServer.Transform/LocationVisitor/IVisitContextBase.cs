using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Base context exposes the <see cref="LocationManager"/> and <see cref="AddError"/> method
    /// and is avalaible when no nodes are being visited.
    /// </summary>
    public interface IVisitContextBase
    {
        /// <summary>
        /// Gets the location manager to use.
        /// </summary>
        ISqlNodeLocationManager LocationManager { get; }

        /// <summary>
        /// Gets the monitor to use to raise error or to say something to the external world.
        /// </summary>
        IActivityMonitor Monitor { get; }

        /// <summary>
        /// Gets the current range filter. Can be null.
        /// </summary>
        ISqlNodeLocationRange RangeFilter { get; }
    }


}
