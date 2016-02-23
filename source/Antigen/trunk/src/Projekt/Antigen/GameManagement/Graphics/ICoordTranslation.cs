using Microsoft.Xna.Framework;

namespace Antigen.Graphics
{
    interface ICoordTranslation
    {
        Point ToAbsolute(Point coords);

// TODO Remove this rule as soon as the member is used.
// ReSharper disable UnusedMember.Global
        Point ToRelative(Point coords);
// ReSharper restore UnusedMember.Global
    }
}
