﻿//
// RunButton.cs
//
// Author:
//       Marius Ungureanu <marius.ungureanu@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using AppKit;
using Foundation;
using CoreGraphics;
using MonoDevelop.Components.MainToolbar;
using MonoDevelop.Ide;
using MonoDevelop.Components;
using Xwt.Mac;
using CoreImage;

namespace MonoDevelop.MacIntegration.MainToolbar
{
	[Register]
	class RunButton : NSButton
	{
		NSImage stopIcon, continueIcon, buildIcon;
		NSImage stopIconDisabled, continueIconDisabled, buildIconDisabled;


		public RunButton ()
		{
			stopIcon = ImageService.GetIcon ("stop").ToNSImage ();
			continueIcon = ImageService.GetIcon ("continue").ToNSImage ();
			buildIcon = ImageService.GetIcon ("build").ToNSImage ();
			stopIconDisabled = ImageService.GetIcon ("stop").WithStyles("disabled").ToNSImage ();
			continueIconDisabled = ImageService.GetIcon ("continue").WithStyles("disabled").ToNSImage ();
			buildIconDisabled = ImageService.GetIcon ("build").WithStyles("disabled").ToNSImage ();

			//Cell = new ColoredButtonCell { BezelColor = Styles.BaseBackgroundColor.ToNSColor () };

			icon = OperationIcon.Run;
			ImagePosition = NSCellImagePosition.ImageOnly;
			BezelStyle = NSBezelStyle.TexturedRounded;
			Enabled = false;
			Cell.ImageDimsWhenDisabled = false;
		}

		NSImage GetIcon ()
		{
			switch (icon) {
			case OperationIcon.Stop:
				return Enabled ? stopIcon : stopIconDisabled;
			case OperationIcon.Run:
				return Enabled ? continueIcon : continueIconDisabled;
			case OperationIcon.Build:
				return Enabled ? buildIcon : buildIconDisabled;
			}
			throw new InvalidOperationException ();
		}

		public override bool Enabled {
			get {
				return base.Enabled;
			}
			set {
				base.Enabled = value;
				Image = GetIcon ();
			}
		}

		OperationIcon icon;
		public OperationIcon Icon {
			get { return icon; }
			set {
				if (value == icon)
					return;
				icon = value;
				Image = GetIcon ();
			}
		}

		public override CGSize IntrinsicContentSize {
			get {
				return new CGSize (38, 25);
			}
		}
	}

	class ColoredButtonCell : NSButtonCell
	{
		public NSColor BezelColor { get; set; }

		public override void DrawBezelWithFrame (CGRect frame, NSView controlView)
		{
			if (BezelColor != null) {
				if (controlView.Frame.Size.Width <= 0 || controlView.Frame.Size.Height <= 0)
					return;
				
				var scaledSize = new CGSize (controlView.Frame.Size.Width * controlView.Window.Screen.BackingScaleFactor, controlView.Frame.Size.Height * controlView.Window.Screen.BackingScaleFactor);

				var image = new NSImage(scaledSize);
				image.LockFocusFlipped(!controlView.IsFlipped);
				base.DrawBezelWithFrame (frame, controlView);
				image.UnlockFocus();

				// create Core image for transformation
				var scaledRect = new CGRect(0, 0, scaledSize.Width, scaledSize.Height);
				var ciImage = CIImage.FromCGImage(image.AsCGImage (ref scaledRect, NSGraphicsContext.CurrentContext, null));

				var filter = new CIColorMonochrome();
				filter.SetDefaults();
				filter.Image = ciImage;
				filter.Color = new CIColor(BezelColor);
				filter.Intensity = 1.0f;
				ciImage = (CIImage)filter.ValueForKey(new NSString("outputImage"));

				var ciCtx = CIContext.FromContext(NSGraphicsContext.CurrentContext.GraphicsPort, null);

				ciCtx.DrawImage (ciImage, new CGRect(0, 0, controlView.Frame.Size.Width, controlView.Frame.Size.Height), scaledRect);
			} else
				base.DrawBezelWithFrame (frame, controlView);
		}
	}
}

