using Topshelf;

namespace CCCP.Telegram
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(hostConfig =>
            {
                hostConfig.StartAutomaticallyDelayed();

                hostConfig.UseLog4Net("log4net.config");

                hostConfig.EnableServiceRecovery(serviceRecovery =>
                {
                    serviceRecovery.RestartService(5);
                });

                hostConfig.Service<CoffeeCounterService>(serviceConfig =>
                {
                    serviceConfig.ConstructUsing(() => new CoffeeCounterService());
                    serviceConfig.WhenStarted(s => s.Start());
                    serviceConfig.WhenStopped(s => s.Stop());
                });
            });
        }
    }
}