using SocketMeister.Testing.ControlBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Test Harness Client
    /// </summary>
    internal class ClientController : IDisposable
    {
        private readonly Dictionary<string, Type> assemblyTypeDictionary = new Dictionary<string, Type>();
        private readonly ControlBusClient _controlBusClient;
        private readonly object _lock = new object();
        private SocketClient _socketClient = null;

        /// <summary>
        /// Triggered when connection could not be established with the HarnessController. This ClientController should now abort (close)
        /// </summary> 
        public event EventHandler<EventArgs> ControlBusConnectionFailed;

        /// <summary>
        /// Event raised when a status of a socket connection has changed
        /// </summary>
        public event EventHandler<EventArgs> ControlBusConnectionStatusChanged;

        /// Event raised when an exception occurs
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionRaised;


        public ClientController(int ControlBusClientId)
        {
            //  SETUP ASSEMBLY TYPE DICTIONARY
            Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type t in assemblyTypes)
            {
                if (assemblyTypeDictionary.ContainsKey(t.Name) == false)
                    assemblyTypeDictionary.Add(t.Name, t);
            }

            //  SETUP CONTROL BUS
            _controlBusClient = new ControlBusClient(ControlBusClientType.ClientController, ControlBusClientId, Constants.ControlBusServerIPAddress, Constants.ControlBusPort);
            _controlBusClient.ConnectionFailed += ControlBusClient_ConnectionFailed;
            _controlBusClient.ConnectionStatusChanged += ControlBusClient_ConnectionStatusChanged;
            _controlBusClient.MessageReceived += ControlBusClient_MessageReceived;
            _controlBusClient.ExceptionRaised += ControlBusClient_ExceptionRaised;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
        }


        public int ClientId { get { return _controlBusClient.ControlBusClientId; } }

        /// <summary>
        /// Lock to provide threadsafe operations
        /// </summary>
        public object Lock { get { return _lock; } }

        public SocketClient SocketClient { get { lock (_lock) { return _socketClient; } } }



        private void ControlBusClient_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            short messageType = Convert.ToInt16(e.Parameters[0]);

            if (messageType == ControlMessage.SocketClientStart)
            {
                List<SocketEndPoint> endPoints = null;
                bool enableCompression;

                using (BinaryReader reader = new BinaryReader(new MemoryStream((byte[])e.Parameters[1])))
                {
                    int capacity = reader.ReadInt32();
                    endPoints = new List<SocketEndPoint>(capacity);
                    for (int ptr = 1; ptr <= capacity; ptr++)
                    {
                        endPoints.Add(new SocketEndPoint(reader.ReadString(), reader.ReadUInt16()));
                    }
                    enableCompression = reader.ReadBoolean();
                }
                SocketClientStart(endPoints, enableCompression);
            }

            else if (messageType == ControlMessage.SocketClientStop)
            {
                SocketClientStop();
            }

            else if (messageType == ControlMessage.ExecuteCommand)
            {
                if (e.Parameters.Length == 3)
                {
                    e.Response = ExecuteMethod((string)e.Parameters[1], (string)e.Parameters[2]);
                }
                else if (e.Parameters.Length == 4)
                {
                    e.Response = ExecuteMethod((string)e.Parameters[1], (string)e.Parameters[2], Serializer.DeserializeParameters((byte[])e.Parameters[3]));
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(e) + "." + nameof(e.Parameters), "Expected 3 or 4 parameters for messageType == ControlMessage.ExecuteMethod");
            }


            else
            {
                throw new ArgumentOutOfRangeException(nameof(e) + "." + nameof(e.Parameters) + "[0]", "No process defined for " + nameof(e) + "." + nameof(e.Parameters) + "[0] = " + messageType + ".");
            }
        }

        private void ControlBusClient_ConnectionFailed(object sender, EventArgs e)
        {
            //  CONNECTION TO THE HarnessController COULD NOT BE ESTABLISHED.
            ControlBusConnectionFailed?.Invoke(this, e);
        }

        private void ControlBusClient_ConnectionStatusChanged(object sender, EventArgs e)
        {
            ControlBusConnectionStatusChanged?.Invoke(sender, e);
        }

        private void ControlBusClient_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            ExceptionRaised?.Invoke(this, e);
        }

        public void Start()
        {
            _controlBusClient.Start();
        }

        /// <summary>
        /// Attempts to cleanly disconnect the control bus client
        /// </summary>
        public void Stop()
        {
            _controlBusClient.ConnectionFailed -= ControlBusClient_ConnectionFailed;
            _controlBusClient.ConnectionStatusChanged -= ControlBusClient_ConnectionStatusChanged;
            _controlBusClient.MessageReceived -= ControlBusClient_MessageReceived; ;
            _controlBusClient.ExceptionRaised -= ControlBusClient_ExceptionRaised;
            _controlBusClient.Stop();
        }


        internal byte[] ExecuteMethod(string ClassName, string StaticMethodName, object[] Parameters = null)
        {
            //  GET THE CLASS TYPE
            assemblyTypeDictionary.TryGetValue(ClassName, out Type thisObjectType);
            if (thisObjectType == null) throw new ArgumentOutOfRangeException(nameof(ClassName), "Class '" + ClassName + "' does not exist in the assembly '" + Assembly.GetExecutingAssembly().FullName + "'");

            //  GET THE METHOD. IT MUST BE STATIC
            MethodInfo getMethod = thisObjectType.GetMethod(StaticMethodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (getMethod == null)
            {
                throw new ArgumentOutOfRangeException(nameof(StaticMethodName), "Method '" + StaticMethodName + "' does not exist or is not static, in the class '" + ClassName + "'.");
            }

            //  PARAMETERS MUST INCLUDE THIS ClientController INSTANCE. BUILD NEW PARAMETERS
            object[] callParams;
            if (Parameters == null)
            {
                callParams = new object[1];
                callParams[0] = this;
            }
            else
            {
                callParams = new object[Parameters.Length + 1];
                callParams[0] = this;
                for (int ptr = 0; ptr < Parameters.Length; ptr++)
                {
                    callParams[ptr + 1] = Parameters[ptr];
                }
            }

            //  CALL THE STATIC CLASS
            object rVal = getMethod.Invoke(null, callParams);
            if (rVal == null) return null;
            else if (rVal.GetType() != typeof(byte[]))
                throw new InvalidOperationException("Static class '" + ClassName + "' must return void, null or a byte array (byte[]).");
            else
                return (byte[])rVal;
        }



        //private void ProcessMessage()
        //{
        //    Type thisObjectType = null;
        //    object thisClass = null;
        //    object results = null;
        //    object[] parameters;

        //    //  VALIDATE
        //    if (request.RequestClassName.Trim() == "")
        //    {
        //        throw new Exception("A class name was not provided to the SocketServices method CallServerMethod. Unable to process request");
        //    }
        //    if (request.RequestMethodName.Trim() == "")
        //    {
        //        throw new Exception("A method name was not provided to the SocketServices method CallServerMethod (ClassName=" + request.RequestClassName + ") . Unable to process request");
        //    }

        //    //  LOOK FOR THE CLASS IN THE CURRENT ASSEMBLY
        //    Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
        //    foreach (Type ty in assemblyTypes)
        //    {
        //        if (ty.Name == request.RequestClassName)
        //        {
        //            thisObjectType = ty;
        //            break;
        //        }
        //    }
        //    if (thisObjectType == null)
        //    {
        //        throw new Exception("CallServerMethod could not find a class of type " + request.RequestClassName + " in this assembly (" + Assembly.GetExecutingAssembly().FullName.ToString() + ")");
        //    }

        //    //  VALIDATE THE METHOD EXISTS
        //    //  METHOD MUST BEGIN WITH "CLIENT_"
        //    //  METHOD MUST BE NOT PUBLIC
        //    //  METHOD MUST BE STATIC
        //    bool throwError = false;
        //    if (request.RequestMethodName.Length < 8) throwError = true;
        //    else if (request.RequestMethodName.ToUpper().Substring(0, 7) != "CLIENT_") throwError = true;
        //    if (throwError == true) throw new Exception("Unable to execute method '" + request.RequestMethodName + "' on class '" + request.RequestClassName + "'. Methods callable must be named 'Client_' as the first 7 characters of the method name. This is a security feature");
        //    var getMethod = thisObjectType.GetMethod(request.RequestMethodName, BindingFlags.Static | BindingFlags.NonPublic);
        //    if (getMethod == null)
        //    {
        //        throw new Exception("Could not find the method " + request.RequestMethodName + " in the class " + request.RequestClassName + ". It does not exist, or is not 'internal'.");
        //    }

        //    //  SETUP THE ARRAY OF OBJECTS
        //    int pCnt = 0;
        //    if (request.RequestParam1 != null) pCnt++;
        //    if (request.RequestParam2 != null) pCnt++;
        //    if (request.RequestParam3 != null) pCnt++;
        //    if (request.RequestParam4 != null) pCnt++;
        //    if (request.RequestParam5 != null) pCnt++;
        //    if (request.RequestParam6 != null) pCnt++;
        //    if (request.RequestParam7 != null) pCnt++;
        //    if (request.RequestParam8 != null) pCnt++;
        //    if (request.RequestParam9 != null) pCnt++;
        //    if (request.RequestParam10 != null) pCnt++;
        //    parameters = new object[pCnt];
        //    pCnt = 0;
        //    if (request.RequestParam1 != null) { parameters[pCnt] = request.RequestParam1; pCnt++; }
        //    if (request.RequestParam2 != null) { parameters[pCnt] = request.RequestParam2; pCnt++; }
        //    if (request.RequestParam3 != null) { parameters[pCnt] = request.RequestParam3; pCnt++; }
        //    if (request.RequestParam4 != null) { parameters[pCnt] = request.RequestParam4; pCnt++; }
        //    if (request.RequestParam5 != null) { parameters[pCnt] = request.RequestParam5; pCnt++; }
        //    if (request.RequestParam6 != null) { parameters[pCnt] = request.RequestParam6; pCnt++; }
        //    if (request.RequestParam7 != null) { parameters[pCnt] = request.RequestParam7; pCnt++; }
        //    if (request.RequestParam8 != null) { parameters[pCnt] = request.RequestParam8; pCnt++; }
        //    if (request.RequestParam9 != null) { parameters[pCnt] = request.RequestParam9; pCnt++; }
        //    if (request.RequestParam10 != null) { parameters[pCnt] = request.RequestParam10; pCnt++; }


        //    //  IF THE CLASS IS STATIC, EXECUTE THE METHOD STATICALLY, OTHERWISE CREATE AN INSTANCE OF THE CLASS AND RUN THE METHOD.
        //    if (thisObjectType.IsSealed == true && thisObjectType.IsAbstract == true)
        //    {
        //        //  THE CLASS IS STATIC. CALL IT
        //        if (pCnt > 0) { results = getMethod.Invoke(null, parameters); }
        //        else { results = getMethod.Invoke(null, null); }
        //    }
        //    else
        //    {
        //        //  NON STATIC CLASS - CALL IT
        //        thisClass = Activator.CreateInstance(thisObjectType);
        //        if (pCnt > 0) { results = getMethod.Invoke(thisClass, parameters); }
        //        else { results = getMethod.Invoke(thisClass, null); }
        //    }

        //    //  IF WE HAVE RESULTS, SERIALIZE THEM
        //    if (results != null)
        //    {
        //        try
        //        {
        //            response.ResponseData = serial.Serialize(results);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception("An error occured when trying to serialize the results from the call to " + request.RequestClassName + "." + request.RequestMethodName + "() :" + ex.Message);
        //        }
        //    }
        //    response.ResponseStatus.Status = SocketResponseStatusTypes.Success;
        //}










        internal void SocketClientStart(List<SocketEndPoint> EndPoints, bool EnableCompression)
        {
            lock (_lock)
            {
                SocketClientStop();
                _socketClient = new SocketClient(EndPoints, EnableCompression);
            }
        }


        internal void SocketClientStop()
        {
            lock (_lock)
            {
                if (_socketClient == null) return;
                _socketClient.Stop();
                _socketClient.Dispose();
                _socketClient = null;
            }

        }

    }
}
