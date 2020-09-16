#if TESTHARNESS
#endif

namespace SocketMeister.Testing.Tests
{
    internal class Test001Client : TestOnClientBase, ITestOnClient
    {
        public Test001Client() : base (Test001Base.Id, Test001Base.Description)
        {
            base.Parent = this;   
        }


        internal static void Test001Step001(ClientController Controller)
        {
            return;
        }




    }
}
