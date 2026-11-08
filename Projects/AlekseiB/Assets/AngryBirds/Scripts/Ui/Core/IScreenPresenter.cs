using System;

namespace AngryBirds.Ui
{
    /// <summary>
    /// Logic of one screen. Created by DI (constructor injection of the services it needs),
    /// attached to the screen view by <see cref="IUiService"/>.
    /// Bind runs every time the screen is shown, Unbind every time it is hidden, Dispose once on shutdown.
    /// </summary>
    public interface IScreenPresenter : IDisposable
    {
        /// <summary>Connects the presenter to the created view. Called once, before the first Bind.</summary>
        void Attach(ScreenBase view);

        /// <summary>Subscribes to services and pushes the current state to the view.</summary>
        void Bind();

        /// <summary>Unsubscribes from everything that <see cref="Bind"/> subscribed to.</summary>
        void Unbind();
    }

    /// <summary>
    /// Presenter base with a typed view. Derive from it and register with
    /// <see cref="UiRegistrationExtensions.RegisterScreenPresenter{TScreen, TPresenter}"/>.
    /// </summary>
    public abstract class ScreenPresenter<TScreen> : IScreenPresenter
        where TScreen : ScreenBase
    {
        protected TScreen View { get; private set; }

        void IScreenPresenter.Attach(ScreenBase view)
        {
            View = (TScreen)view;
        }

        public abstract void Bind();

        public abstract void Unbind();

        public virtual void Dispose()
        {
        }
    }
}
