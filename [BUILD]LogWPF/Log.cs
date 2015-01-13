using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace _BUILD_LogWPF
{
    public class Log
    {
        /// <summary>
        /// The current logging level.
        /// </summary>
        public static volatile Level LoggingLevel = Level.Trace;

        /// <summary>
        /// Contains a value indicating whether trace level messages are currently enabled.
        /// </summary>
        public static volatile bool IsTraceEnabled = true;

        /// <summary>
        /// Contains a value indicating whether debug level messages are currently enabled.
        /// </summary>
        public static volatile bool IsDebugEnabled = true;

        /// <summary>
        /// Contains a value indicating whether info level messages are currently enabled.
        /// </summary>
        public static volatile bool IsInfoEnabled = true;

        /// <summary>
        /// Contains a value indicating whether warn level messages are currently enabled.
        /// </summary>
        public static volatile bool IsWarnEnabled = true;

        /// <summary>
        /// Contains a value indicating whether error level messages are currently enabled.
        /// </summary>
        public static volatile bool IsErrorEnabled = true;

        /// <summary>
        /// Contains a value indicating whether fatal level messages are currently enabled.
        /// </summary>
        public static volatile bool IsFatalEnabled = true;

        /// <summary>
        /// Occurs when a new message was added to the log.
        /// </summary>
        public static event Action<object> NewMessage;

        /// <summary>
        /// Occurs when the logging level was changed.
        /// </summary>
        public static event Action<object> ChangedLoggingLevel;

        /// <summary>
        /// The message container.
        /// </summary>
        public static readonly ConcurrentBag<Entry> Messages = new ConcurrentBag<Entry>();

        /// <summary>
        /// Sets the debugging level.
        /// </summary>
        /// <param name="level">The level.</param>
        public static void SetLevel(Level level)
        {
            LoggingLevel = level;

            IsTraceEnabled = level >= Level.Trace;
            IsDebugEnabled = level >= Level.Debug;
            IsInfoEnabled = level >= Level.Info;
            IsWarnEnabled = level >= Level.Warn;
            IsErrorEnabled = level >= Level.Error;
            IsFatalEnabled = level >= Level.Fatal;

            if (ChangedLoggingLevel != null)
            {
                Task.Factory.StartNew(ChangedLoggingLevel, level);
            }
        }

        /// <summary>
        /// The weight of the message.
        /// </summary>        
        [Flags]
        public enum Level : int
        {
            None = 0,
            Fatal = 1,
            Error = 1 << 1,
            Warn = 1 << 2,
            Info = 1 << 3,
            Debug = 1 << 4,
            Trace = 1 << 5
        }

        /// <summary>
        /// Writes the diagnostic message at Trace level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="file">The file where this message originates from.</param>
        /// <param name="method">The method where this message originates from.</param>
        /// <param name="line">The line where this message originates from.</param>
        public static void Trace(string message, [CallerFilePath] string file = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = 0)
        {
            if (IsTraceEnabled) Write(Level.Trace, message, file, method, line);
        }

        /// <summary>
        /// Writes the diagnostic message at Debug level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="file">The file where this message originates from.</param>
        /// <param name="method">The method where this message originates from.</param>
        /// <param name="line">The line where this message originates from.</param>
        public static void Debug(string message, [CallerFilePath] string file = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = 0)
        {
            if (IsDebugEnabled) Write(Level.Debug, message, file, method, line);
        }

        /// <summary>
        /// Writes the diagnostic message at Info level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="file">The file where this message originates from.</param>
        /// <param name="method">The method where this message originates from.</param>
        /// <param name="line">The line where this message originates from.</param>
        public static void Info(string message, [CallerFilePath] string file = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = 0)
        {
            if (IsInfoEnabled) Write(Level.Info, message, file, method, line);
        }

        /// <summary>
        /// Writes the diagnostic message at Warn level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="file">The file where this message originates from.</param>
        /// <param name="method">The method where this message originates from.</param>
        /// <param name="line">The line where this message originates from.</param>
        public static void Warn(string message, [CallerFilePath] string file = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = 0)
        {
            if (IsWarnEnabled) Write(Level.Warn, message, file, method, line);
        }

        /// <summary>
        /// Writes the diagnostic message at Error level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="file">The file where this message originates from.</param>
        /// <param name="method">The method where this message originates from.</param>
        /// <param name="line">The line where this message originates from.</param>
        public static void Error(string message, [CallerFilePath] string file = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = 0)
        {
            if (IsErrorEnabled) Write(Level.Error, message, file, method, line);
        }

        /// <summary>
        /// Writes the diagnostic message at Fatal level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="file">The file where this message originates from.</param>
        /// <param name="method">The method where this message originates from.</param>
        /// <param name="line">The line where this message originates from.</param>
        public static void Fatal(string message, [CallerFilePath] string file = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = 0)
        {
            if (IsFatalEnabled) Write(Level.Fatal, message, file, method, line);
        }

        /// <summary>
        /// Writes the specified diagnostic message to the log.
        /// </summary>
        /// <param name="level">The weight of the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="file">The file where this message originates from.</param>
        /// <param name="method">The method where this message originates from.</param>
        /// <param name="line">The line where this message originates from.</param>
        private static void Write(Level level, string message, string file, string method, int line)
        {
            var log = new Entry(DateTime.Now, level, file, method, line, message);

            Messages.Add(log);

            if (NewMessage != null)
            {
                Task.Factory.StartNew(NewMessage, log);
                //NewMessage(log);
                //ThreadPool.QueueUserWorkItem(NewMessage, log);
                //new Thread(new ParameterizedThreadStart(NewMessage)).Start(log);
            }
        }

        public class Entry
        {
            /// <summary>
            /// Gets or sets the time when the logged message occurred.
            /// </summary>
            /// <value>
            /// The time when the logged message occurred.
            /// </value>
            public readonly DateTime Time;

            /// <summary>
            /// Gets or sets the weight of the logged message.
            /// </summary>
            /// <value>
            /// The weight of the logged message.
            /// </value>
            public readonly Level Level;

            /// <summary>
            /// Gets or sets the file where the logged message occurred.
            /// </summary>
            /// <value>
            /// The file where the logged message occurred.
            /// </value>
            public readonly string File;

            /// <summary>
            /// Gets or sets the method in which the logged message occurred.
            /// </summary>
            /// <value>
            /// The method in which the logged message occurred.
            /// </value>
            public readonly string Method;

            /// <summary>
            /// Gets or sets the line where the logged message occurred.
            /// </summary>
            /// <value>
            /// The line where the logged message occurred.
            /// </value>
            public readonly int Line;

            /// <summary>
            /// Gets or sets the logged message.
            /// </summary>
            /// <value>
            /// The logged message.
            /// </value>
            public readonly string Message;

            /// <summary>
            /// Gets or sets the managed ID of the thread in the thread pool.
            /// </summary>
            /// <value>
            /// The managed ID of the thread in the thread pool.
            /// </value>
            public readonly int Thread;

            /// <summary>
            /// Initializes a new instance of the <see cref="Entry" /> class.
            /// </summary>
            /// <param name="time">The time when the logged message occurred.</param>
            /// <param name="level">The weight of the logged message.</param>
            /// <param name="file">The file where the logged message occurred.</param>
            /// <param name="method">The method in which the logged message occurred.</param>
            /// <param name="line">The line where the logged message occurred.</param>
            /// <param name="message">The logged message.</param>
            public Entry(DateTime time, Level level, string file, string method, int line, string message)
            {
                Time = time;
                Level = level;
                File = file;
                Method = method;
                Line = line;
                Message = message;
                Thread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format("{0:HH:mm:ss.fff} {1} {2} {3}/{4}():{5} - {6}", Time, Level.ToString().ToUpper(),
                    Thread, Path.GetFileName(File), Method, Line, Message);
            }
        }
    }
}
