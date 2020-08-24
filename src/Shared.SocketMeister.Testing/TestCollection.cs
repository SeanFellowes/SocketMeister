using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SocketMeister.Testing.Tests;
using System.Threading;

namespace SocketMeister.Testing
{
    internal class TestCollection<T> : List<T>
    {
        public TestCollection()
        {
            //  DYNAMICALLY ADD TESTS. TESTS ARE ANY CLASS NAMED BETWEEN SocketMeister.Test001 and SocketMeister.Test999
            //  THIS ELIMINATES HAVING TO ADD CODE HERE WHEN A NEW TEST IS CREATED.
            //  IGNORE TEMPLATE TEST 000
            for (int ctr = 1; ctr <= 999; ctr++)
            {
                string className = typeof(Test000).Namespace + ".Test" + ctr.ToString("000", CultureInfo.InvariantCulture) + "Harness";
                Type t = Type.GetType(className);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
            }
        }


        private T AddTest(Type t, int ctr)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            object[] parms = new object[1];
            parms[0] = ctr;

            T test = (T)Activator.CreateInstance(t, parms);
            Add(test);
            return test;
        }

    }
}
