
namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// Used to create a custom sequence control engine for a wizard.
    /// </summary>
    public interface IUIWizardSequencer
    {
        /// <summary>
        /// Called by the wizard engine to initialize the sequencer.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="request"></param>
        void Initialize(WizardEngine engine, object request);

        /// <summary>
        /// Move to the next page of the wizard from the current page, if possible
        /// </summary>
        /// <returns>The new page the wizard moved to</returns>
        /// <remarks>If moving next isn't valid at this point an exception will be thrown.</remarks>
        IUIWizardPage MoveNext();

        /// <summary>
        /// Move to the previous page of the wizard from the current page, if possible
        /// </summary>
        /// <returns>The new page the wizard moved to</returns>
        /// <remarks>If moving previous isn't valid at this point an exception will be thrown.</remarks>
        IUIWizardInputPage MovePrevious();
 
    }
}
