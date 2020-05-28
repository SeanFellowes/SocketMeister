using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SocketMeister
{
    internal class TestHarness : List<ITest>
    {
        public TestHarness()
        {
            //  DYNAMICALLY ADD TESTS. TESTS ARE ANY CLASS NAMED BETWEEN SocketMeister.Test000 and SocketMeister.Test999
            //  THIS ELIMINATES HAVING TO ADD CODE HERE WHEN A NEW TEST IS CREATED.
            for (int ctr = 0; ctr <= 999; ctr++ )
            {
                string className = typeof(TestHarness).Namespace + ".Test" + ctr.ToString("000", CultureInfo.InvariantCulture);
                Type t = Type.GetType(className);
                object[] parms = new object[1];
                parms[0] = ctr;
                if (t != null) Add((ITest)Activator.CreateInstance(t, parms));
            }
        }
    }
}
