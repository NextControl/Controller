using System;

namespace ManiaplanetXMLRPC.Attributes
{
    public enum EventOrder
    {
        None,
        AfterDependencies
    }

    public class OrderAttribute : Attribute
    {
        #region Public Fields

        public EventOrder order = EventOrder.None;

        #endregion Public Fields

        #region Public Constructors

        public OrderAttribute(EventOrder order)
        {
            this.order = order;
        }

        #endregion Public Constructors
    }
}