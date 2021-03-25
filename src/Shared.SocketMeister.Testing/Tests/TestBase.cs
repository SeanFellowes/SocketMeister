namespace SocketMeister.Testing.Tests
{
    internal partial class TestBase<T> : ITest<T>
    {
        private readonly int _id;
        private readonly string _description;
        public T _parent = default;
        private readonly object _lock = new object();

        public TestBase(int Id, string Description)
        {
            _id = Id;
            _description = Description;
        }


        public string Description => _description;

        public int Id => _id;

        public object Lock => _lock;

        /// <summary>
        /// Short name for the test (e.g. Test001)
        /// </summary>
        public string Name => "Test" + _id.ToString("000");

        internal T Parent
        {
            get { lock (_lock) { return _parent; } }
            set { lock (_lock) { _parent = value; } }
        }



    }
}
