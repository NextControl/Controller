using ManiaplanetXMLRPC.Attributes;

namespace ManiaplanetXMLRPC.Interfaces
{
    public delegate void ResultEvent(CallbackContext context);

    public interface IRegisterEvent<T>
    {
        #region Public Properties

        #endregion Public Properties

        #region Public Methods

        void RegisterListener<Interface, OBJ>(OBJ op)
            where Interface : ICallback
            where OBJ : class;

        void RegisterListener<Know>(ResultEvent onAction) where Know : ICallback;

        #endregion Public Methods
    }

    public static class ObjectExtensionOperator
    {
    }

    public class Extendable : object
    {
        public static explicit operator string(Extendable e) => e.ToString();
        public static explicit operator int(Extendable e) => (int)e;
        public static explicit operator bool(Extendable e) => (bool)e;
    }
}