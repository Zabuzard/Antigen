using Antigen.Input;

namespace Antigen.Logic.UnitModes
{
    interface IModeControl : IUpdateable, IRightClickListener
    {
        void Activate();
        void Terminate();
    }
}
