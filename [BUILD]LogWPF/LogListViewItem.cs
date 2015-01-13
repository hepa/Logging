using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _BUILD_LogWPF
{
    public class LogListViewItem
    {
        /// <summary>
        /// Gets or sets the original log entry.
        /// </summary>
        /// <value>The original log entry.</value>
        public Log.Entry Item { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        /// <value>The time.</value>
        public string Time { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public string Location { get; set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get
            {
                return Item.Message;
            }
        }

        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <value>The level.</value>
        public string Level
        {
            get
            {
                return Item.Level.ToString();
            }
        }

        /// <summary>
        /// Gets the thread.
        /// </summary>
        /// <value>The thread.</value>
        public string Thread
        {
            get
            {
                return Item.Thread.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogListViewItem" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public LogListViewItem(Log.Entry item)
        {
            Item = item;
            Time = item.Time.ToString("HH:mm:ss.fff");
            Location = Path.GetFileName(item.File) + "/" + item.Method + "():" + item.Line;

            switch (item.Level)
            {
                default:
                case Log.Level.None: Icon = "Images/unchecked.png"; break;
                case Log.Level.Trace: Icon = "Images/bug.png"; break;
                case Log.Level.Debug: Icon = "Images/information-white.png"; break;
                case Log.Level.Info: Icon = "Images/information.png"; break;
                case Log.Level.Warn: Icon = "Images/exclamation.png"; break;
                case Log.Level.Error: Icon = "Images/exclamation-red.png"; break;
                case Log.Level.Fatal: Icon = "Images/fire.png"; break;
            }
        }
    }
}
