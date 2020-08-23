using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SocketMeister.Testing.Tests;
using System.Threading;

namespace SocketMeister.Testing
{
    internal class TestCollection : List<ITestOnHarness>
    {
        private readonly object classLock = new object();

        public event EventHandler<TestStatusChangedEventArgs> TestStatusChanged;

        public TestCollection()
        {
            //  DYNAMICALLY ADD TESTS. TESTS ARE ANY CLASS NAMED BETWEEN SocketMeister.Test001 and SocketMeister.Test999
            //  THIS ELIMINATES HAVING TO ADD CODE HERE WHEN A NEW TEST IS CREATED.
            //  IGNORE TEMPLATE TEST 000
            for (int ctr = 1; ctr <= 999; ctr++)
            {
                string className = typeof(Test000).Namespace + ".Test" + ctr.ToString("000", CultureInfo.InvariantCulture);
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


        private void AddTest(Type t, int ctr)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            object[] parms = new object[1];
            parms[0] = ctr;

            ITestOnHarness test = (ITestOnHarness)Activator.CreateInstance(t, parms);
            test.StatusChanged += Test_StatusChanged;
            Add(test);
        }

        private void Test_StatusChanged(object sender, TestStatusChangedEventArgs e)
        {
            TestStatusChanged?.Invoke(this, e);
        }


    }
}
