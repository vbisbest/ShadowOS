using System;
using AppKit;
using CoreGraphics;
using Foundation;
using System.Collections;
using System.Collections.Generic;

namespace ShadowOSMonitor
{
    public class EventTableDataSource : NSTableViewDataSource
    {
        #region Public Variables
        public List<Event> Events = new List<Event>();
        #endregion

        #region Constructors
        public EventTableDataSource()
        {
        }
        #endregion        

        #region Override Methods
        public override nint GetRowCount(NSTableView tableView)
        {
            return Events.Count;
        }
        #endregion
    }
}
