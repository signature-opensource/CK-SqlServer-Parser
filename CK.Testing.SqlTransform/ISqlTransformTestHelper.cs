using System;

namespace CK.Testing
{
    /// <summary>
    /// Mixin of <see cref="SqlTransform.ISqlTransformTestHelperCore"/> and <see cref="IMonitorTestHelper"/>.
    /// </summary>
    public interface ISqlTransformTestHelper : IMixinTestHelper, IMonitorTestHelper, SqlTransform.ISqlTransformTestHelperCore
    {
    }
}
