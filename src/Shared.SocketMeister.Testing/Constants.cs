namespace SocketMeister.Testing
{
    internal class Constants
    {
        public static int ControlBusPort = 4505;
        public static int HarnessFixedServer1Port = 4506;

        //  Silverlight ports are between 4502-4534
        public static int SilverlightPolicyPort = 943;

        public static string ControlBusServerIPAddress = "127.0.0.1";

        public static int MaxWaitMsForControlBusClientToHarnessControllerConnect = 10000;
    }
}
