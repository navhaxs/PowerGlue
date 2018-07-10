using StartupHelper;

namespace PowerGlue.Models
{
    class StartupModel
    {
        StartupManager StartupController = new StartupManager("PowerGlue", RegistrationScope.Local, false);

        public void Register()
        {
            StartupController.Register();
        }

        public void Unregister()
        {
            StartupController.Unregister();
        }

        public bool IsRegistered()
        {
            return StartupController.IsRegistered;
        }
    }
}
