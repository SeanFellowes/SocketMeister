//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SocketMeister
//{
//    internal class ThreadHostExecuteProcessEventArgs : EventArgs { public object OptionalData { get; set; } }
//    public class ThreadHostErrorCountChangedEventArgs : EventArgs { public int ErrorCount { get; set; } }
//    public class ThreadHostIsProcessingChangedEventArgs : EventArgs { public ThreadHostProcessingStatusTypes ProcessingStatus { get; set; } }
//    public class ThreadHostIsRunningChangedEventArgs : EventArgs { public bool IsRunning { get; set; } }
//    public class ThreadHostProcessingDescriptionChangedEventArgs : EventArgs { public string ProcessingDescription { get; set; } }
//    public class ThreadHostCollectionChangedEventArgs : EventArgs { public List<IThreadHost> Items { get; set; } }

//    /// <summary>
//    /// Current processing status of a ThreadHost
//    /// </summary>
//    public enum ThreadHostProcessingStatusTypes
//    {
//        /// <summary>
//        /// The host thread is currently not processing anything
//        /// </summary>
//        NotProcessing = 0,
//        /// <summary>
//        /// The ThreadHost is executing it's custom process.
//        /// </summary>
//        Processing = 1,
//        /// <summary>
//        /// THe thread has been instructed to stop and is executing code in the Finalize section (Often there is nothing to run)
//        /// </summary>
//        Finalizing = 2
//    }

//    /// <summary>
//    /// Threadhost is an interface class.
//    /// </summary>
//    public interface IThreadHost
//    {
//        #region PROPERTIES

//        /// <summary>
//        /// Number of errors which have occured on this thread. The counter is reset whenever the thread is started.
//        /// </summary>
//        int ErrorCount { get; }
//        /// <summary>
//        /// If activated, code associated with the thread will execute, otherwise, thread will sit idle. This property should be true in production, but useful in testing to isolate code.
//        /// </summary>
//        bool IsEnabled { get; set; }
//        /// <summary>
//        /// Indicator as to whether a thread is doing any work (Processing or Finalizing)
//        /// </summary>
//        ThreadHostProcessingStatusTypes ProcessingStatus { get; }
//        /// <summary>
//        /// Indicator as to whether a thread is running or not. ThreadHost threads are designed to always run while the service is running
//        /// </summary>
//        bool IsRunning { get; }
//        /// <summary>
//        /// Thread was aborted trying to stop if from running
//        /// </summary>
//        bool IsThreadAborted { get; }
//        /// <summary>
//        /// Purpose of the thread
//        /// </summary>
//        string Description { get; }
//        /// <summary>
//        /// Useful for debuging or a console application, to show some details about what task which is currently executing in the thread. This will always be blank if the thread is not processing
//        /// </summary>
//        string ProcessingDescription { get; }
//        /// <summary>
//        /// The thread has been requested to stop, or is stopped.
//        /// </summary>
//        bool Stop { get; }

//        #endregion


//        #region EVENTS

//        /// <summary>
//        /// Raised when an error occurs on the thread
//        /// </summary>
//        event EventHandler<ThreadHostErrorCountChangedEventArgs> ErrorCountChanged;
//        /// <summary>
//        /// Raised when the thread starts or stops running. This should only ever occur then the service starts or stops
//        /// </summary>
//        event EventHandler<ThreadHostIsRunningChangedEventArgs> IsRunningChanged;
//        /// <summary>
//        /// Raised whenever the description of the current processing action changes.
//        /// </summary>
//        event EventHandler<ThreadHostProcessingDescriptionChangedEventArgs> ProcessingDescriptionChanged;
//        /// <summary>
//        /// Raised when the thread start or stops processing code
//        /// </summary>
//        event EventHandler<ThreadHostIsProcessingChangedEventArgs> ProcessingStatusChanged;

//        #endregion

//    }
//}
