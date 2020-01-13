// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ShadowOSMonitor
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSButtonCell ButtonCapture { get; set; }

		[Outlet]
		AppKit.NSButtonCell ButtonClearEvents { get; set; }

		[Outlet]
		AppKit.NSTableColumn ColumnDetails { get; set; }

		[Outlet]
		AppKit.NSTableColumn ColumnEventType { get; set; }

		[Outlet]
		AppKit.NSTableView TableEvents { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ButtonCapture != null) {
				ButtonCapture.Dispose ();
				ButtonCapture = null;
			}

			if (ButtonClearEvents != null) {
				ButtonClearEvents.Dispose ();
				ButtonClearEvents = null;
			}

			if (ColumnDetails != null) {
				ColumnDetails.Dispose ();
				ColumnDetails = null;
			}

			if (ColumnEventType != null) {
				ColumnEventType.Dispose ();
				ColumnEventType = null;
			}

			if (TableEvents != null) {
				TableEvents.Dispose ();
				TableEvents = null;
			}
		}
	}
}
