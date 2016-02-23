namespace Antigen.Input
{
    /// <summary>
    /// Values for the order in which
    /// event listeners receive events.
    /// Values corresponding to lower
    /// integers receive events first.
    /// </summary>
    enum EventOrder
    {
        HudUpper,
        HudLower,
        Selector,
        GameObjects
    }

    /// <summary>
    /// Super-interface for event listeners. Concrete
    /// event listeners should implement one of the specialised
    /// interfaces (e.g. <see cref="IKeyListener"/>).
    /// </summary>
    interface IEventListener
    {
        /// <summary>
        /// Indicates the priority for dispatching events to this
        /// listener. Listeners with a lower value will receive
        /// events before listeners with a higher value. The order
        /// in which events are dispatched to listeners with the
        /// same value is undetermined.
        /// </summary>
        EventOrder EventOrder { get; }
    }
}
