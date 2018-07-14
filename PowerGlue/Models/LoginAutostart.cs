using StartupHelper;

namespace PowerGlue.Models
{
    class LoginAutostart
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
