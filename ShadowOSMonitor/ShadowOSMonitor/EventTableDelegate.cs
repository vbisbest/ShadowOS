using System;
using AppKit;
using CoreGraphics;
using Foundation;
using System.Collections;
using System.Collections.Generic;

namespace ShadowOSMonitor
{
    public class EventTableDelegate : NSTableViewDelegate
    {
       

        #region Constants 
        private const string CellIdentifier = "EventCell";
        #endregion

        #region Private Variables
        private EventTableDataSource DataSource;
        #endregion

        #region Constructors
        public EventTableDelegate(EventTableDataSource datasource)
        {
            this.DataSource = datasource;
        }
        #endregion

        #region Override Methods

       

        public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
        { 
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data
            NSTextField view = (NSTextField)tableView.MakeView(CellIdentifier, this);
            if (view == null)
            {
                view = new NSTextField();
                view.Identifier = CellIdentifier;
                view.BackgroundColor = NSColor.Clear;
                view.Bordered = false;
                view.Selectable = false;
                view.Editable = false;
            }

            // Setup view based on the column selected
            switch (tableColumn.Title)
            {
                case "Event Type":
                    view.StringValue = DataSource.Events[(int)row].EventType;
                    break;
                case "Action":
                    if (string.IsNullOrEmpty(DataSource.Events[(int)row].Action))
                    {
                        view.StringValue = string.Empty;
                    }
                    else
                    {
                        view.StringValue = DataSource.Events[(int)row].Action;
                    }
                    break;
                case "Details":
                    if (string.IsNullOrEmpty(DataSource.Events[(int)row].Details))
                    {
                        view.StringValue = string.Empty;
                    }
                    else
                    {
                        view.StringValue = DataSource.Events[(int)row].Details;
                    }
                    break;
            }

            return view;
        }        
        #endregion
    }
}