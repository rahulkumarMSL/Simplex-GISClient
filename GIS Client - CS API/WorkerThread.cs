using System.Threading;


namespace Gis.Utils.Threading
{
    /*
     * This class provides a wrapper for a thread which can be stopped.
     */
    public class WorkerThread
    {
        private Thread m_Thread;        // The thread object.

        public delegate bool isStoppedDelegate(int timeout = 0);                                                        // The delegate for the function which returns whether the thread has been stopped or not.
        public delegate bool threadExecutionDelegate(isStoppedDelegate isStopped, ThreadParameters threadParameters);   // The delegate for the function which performs the thread.

        private ThreadParameters m_ThreadParameters = null;     // The parameters passed into the thread.

        private threadExecutionDelegate m_ThreadExecution;                              // The pointer to the function which performs the thread execution.
        private ManualResetEvent m_StopEvent = new ManualResetEvent(false);             // The event which indicates when the thread has stopped.
        private ManualResetEvent m_ThreadStartedEvent = new ManualResetEvent(false);    // The event which indicates when the thread has started.


        /*
         * The constructor of the WorkerThread class.
         * \param threadExecution - The delegate for the function which will be called when this thread executes.
         */
        public WorkerThread(threadExecutionDelegate threadExecution)
        {
            m_ThreadExecution = threadExecution;
        }
        /*
         * The destructor of the WorkerThread class.
         * This will automatically call for the thread to be stopped.
         */
        ~WorkerThread()
        {
            Stop();
        }


        /*
         * Start the thread.
         * This function will block until the thread has started.
         * \param timeout - The maximum number of milliseconds the function will wait for the thread to start. Set to 0 to not wait and to -1 to wait indefinitely.
         * \return Whether the thread started correctly or not.
         */
        public bool Start(int timeout = 0)
        {
            if (IsRunning())
            {
                return true;
            }

            m_StopEvent.Reset();

            m_ThreadStartedEvent.Reset();

            m_Thread = new Thread(new ThreadStart(PerformThreadExecution));
            m_Thread.IsBackground = true;
            m_Thread.Start();

            if (!WaitForThreadToStart(timeout))
            {
                return false;
            }

            return true;
        }
        /*
         * Start the thread.
         * This function will block until the thread has started.
         * \param threadParameters - The parameters to be passed to this thread.
         * \param timeout - The maximum number of milliseconds the function will wait for the thread to start. Set to 0 to not wait and to -1 to wait indefinitely.
         * \return Whether the thread started correctly or not.
         */
        public bool Start(ThreadParameters threadParameters, int timeout = 0)
        {
            m_ThreadParameters = threadParameters;

            if (!Start(timeout))
            {
                return false;
            }

            return true;
        }
        /*
         * Stop the thread.
         * This function will block until the thread has fully stopped.
         * \param timeout - The maximum number of milliseconds the function will wait for the thread to stop. Set to 0 to not wait and to -1 to wait indefinitely.
         * \return Whether the thread stopped correctly or not.
         */
        public bool Stop(int timeout = -1)
        {
            if (!IsRunning())
            {
                return true;
            }

            m_StopEvent.Set();

            if (!WaitForThreadToFinish(timeout))
            {
                return false;
            }

            return true;
        }
        /*
         * Runs the thread.
         * This function will block until the thread has fully stopped.
         * \param timeout - The maximum number of milliseconds the function will wait for the thread to stop. Set to 0 to not wait and to -1 to wait indefinitely.
         * \return Whether the thread ran correctly or not.
         */
        public bool Run(int timeout = -1)
        {
            if (!Start(timeout))
            {
                return false;
            }

            if (!WaitForThreadToFinish(timeout))
            {
                return false;
            }

            return true;
        }
        /*
         * Runs the thread.
         * This function will block until the thread has fully stopped.
         * \param threadParameters - The parameters to be passed to this thread.
         * \param timeout - The maximum number of milliseconds the function will wait for the thread to stop. Set to 0 to not wait and to -1 to wait indefinitely.
         * \return Whether the thread ran correctly or not.
         */
        public bool Run(ThreadParameters threadParameters, int timeout = -1)
        {
            if (!Start(threadParameters, timeout))
            {
                return false;
            }

            if (!WaitForThreadToFinish(timeout))
            {
                return false;
            }

            return true;
        }
        

        /*
         * This implements the execution of the thread.
         */
        private void PerformThreadExecution()
        {
            m_ThreadStartedEvent.Set();

            m_ThreadExecution(IsStopped, m_ThreadParameters);

            m_ThreadParameters = null;
        }


        /*
         * This returns whether the thread is running or not.
         * \param timeout - The maximum number of milliseconds the function will wait for the thread to start. Set to 0 to not wait and to -1 to wait indefinitely.
         * \return Whether the thread is running or not.
         */
        public bool IsRunning(int timeout = 0)
        {
            if (m_Thread == null)
            {
                return false;
            }

            if (m_Thread.Join(timeout))
            {
                return false;
            }

            return true;
        }
        /*
         * This returns whether the thread is running or not.
         * * \param timeout - The maximum number of milliseconds the function will wait for the thread to stop. Set to 0 to not wait and to -1 to wait indefinitely.
         * \return Whether the thread is running or not.
         */
        public bool IsStopped(int timeout = 0)
        {
            if (!m_StopEvent.WaitOne(timeout))
            {
                return false;
            }

            return true;
        }


        /*
         * This function will block until the thread has fully started.
         * \param timeout - The maximum number of milliseconds the function will wait for the thread to start. Set to -1 to wait indefinitely.
         * \return Whether the thread started correctly or not.
         */
        public bool WaitForThreadToStart(int timeout = -1)
        {
            if (timeout != 0)
            {
                if (!m_ThreadStartedEvent.WaitOne(timeout))
                {
                    return false;
                }
            }

            return true;
        }
        /*
         * This function will block until the thread has fully stopped.
         * \param timeout - The maximum number of milliseconds the function will wait for the thread to stop. Set to -1 to wait indefinitely.
         * \return Whether the thread stopped correctly or not.
         */
        public bool WaitForThreadToFinish(int timeout = -1)
        {
            if (!IsRunning())
            {
                return true;
            }

            if (timeout != 0)
            {
                if (!m_Thread.Join(timeout))
                {
                    return false;
                }
            }

            return true;
        }
    }


    /*
     * This class provides a base class for any class which holds some parameters to pass to a thread.
     */
    public abstract class ThreadParameters
    {
        public ThreadParameters() { }
        ~ThreadParameters() { }
    }
}