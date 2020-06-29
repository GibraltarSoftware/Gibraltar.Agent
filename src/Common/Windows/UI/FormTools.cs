#region File Header
// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 - 2015 by Gibraltar Software, Inc.  
//  *    All rights reserved.
//  *******************************************************************/
#endregion

//#define DISABLE_FONT_CONTROL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Gibraltar.Windows.UI
{
    /// <summary>
    /// Standard helper methods for WinForms development.
    /// </summary>
    public static class FormTools
    {
        private static readonly Font s_FixedWidthFont;

        static FormTools()
        {
            //figure out our fixed width font
            using (var installedFonts = new InstalledFontCollection())
            {
                //since we're going to do several lookups, lets index this list....
                var indexFontCollection = new Dictionary<string, FontFamily>(installedFonts.Families.Length, StringComparer.OrdinalIgnoreCase);

                foreach (var family in installedFonts.Families)
                {
                    try
                    {
                        if (indexFontCollection.ContainsKey(family.Name) == false)
                            indexFontCollection.Add(family.Name, family);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning("Unable to add font family to candidate font list due to {0}\r\nWe will skip the font family {1}.\r\n{2}", ex, family.Name, ex.Message);
                    }
                }

                FontFamily fixedWidthFontFamily;
                if ((indexFontCollection.TryGetValue("Consolas", out fixedWidthFontFamily) == false)
                    || (fixedWidthFontFamily.IsStyleAvailable(FontStyle.Regular) == false))
                {
                    //ok, our second best...
                    if ((indexFontCollection.TryGetValue("Lucida Console", out fixedWidthFontFamily) == false)
                        || (fixedWidthFontFamily.IsStyleAvailable(FontStyle.Regular) == false))
                    {
                        //if not, something we can always get.
                        fixedWidthFontFamily = indexFontCollection["Courier New"];
                    }
                }

                s_FixedWidthFont = new Font(fixedWidthFontFamily, 8, GraphicsUnit.Point);
            }
        }

        /// <summary>
        /// Ensures the default font for every control is set to the current OS system font.
        /// </summary>
        /// <param name="form"></param>
        public static void ApplyOSFont(Form form)
        {
#if DISABLE_FONT_CONTROL
            return;
#endif

            form.Font = MergeFont(form.Font);

            // Set the default dialog font on each child control
            foreach (Control currentControl in form.Controls)
            {
                ApplyOSFont(currentControl);
            }
        }

        /// <summary>
        /// Ensures the default font for every control is set to the current OS system font.
        /// </summary>
        /// <param name="control"></param>
        public static void ApplyOSFont(Control control)
        {
#if DISABLE_FONT_CONTROL
            return;
#endif

            var newFont = MergeFont(control.Font);

            //lets optimize:  only do it if it'd make a difference.  Even setting the font to the same value has work.
            if (newFont != control.Font)
                control.Font = newFont;

            // Set the default dialog font on each child control
            foreach (Control currentControl in control.Controls)
            {
                ApplyOSFont(currentControl);
            }
        }

        /// <summary>
        /// Merges the provided font with the system text font family.
        /// </summary>
        /// <param name="originalFont"></param>
        /// <returns></returns>
        public static Font MergeFont(Font originalFont)
        {
            if (originalFont == null)
            {
                //just create a new system font
                return OSFont;
            }

#if DISABLE_FONT_CONTROL
            return originalFont;
#else
            //we want to replace the font family but nothing else; we leave size scaling to the OS.
            if (OSFont.Name.Equals(originalFont.Name))
                return originalFont;

            return new Font(OSFont.Name, originalFont.Size, originalFont.Style, OSFont.Unit);                
#endif
        }

        /// <summary>
        /// The correct system text font (OS specific)
        /// </summary>
        public static Font OSFont
        {
            get
            {
                return SystemFonts.MessageBoxFont;
            }
        }

        /// <summary>
        /// Our best fixed width font for the current OS.
        /// </summary>
        public static Font FixedWidthFont
        {
            get
            {
                return s_FixedWidthFont;
            }
        }

        /// <summary>
        /// Updates any HelpProvider control to use the full path of the executing assembly for the path.
        /// </summary>
        /// <param name="provider"></param>
        public static void FixHelpProviderPath(HelpProvider provider)
        {
            if (provider == null)
                return;

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            if (string.IsNullOrEmpty(assemblyPath))
            {
                assemblyPath = Assembly.GetEntryAssembly().Location;
            }

            if (string.IsNullOrEmpty(assemblyPath))
            {
                return; //we have no help path to guess with
            }

            string helpFilePath = Path.GetDirectoryName(assemblyPath);

            if (File.Exists(provider.HelpNamespace) == false)
            {
                provider.HelpNamespace = Path.Combine(helpFilePath, provider.HelpNamespace);
            }
        }

        /* NOTE: Moved to Gibraltar.FileSystemTools (Common\FileSystem.tools.cs) so it doesn't have to load FormTools to check this.
        /// <summary>
        /// Returns a safe format provider for the current user interface culture.
        /// </summary>
        /// <remarks>If the current UI culture is not usable it will fall back to the thread main culture which always is.</remarks>
        public static IFormatProvider UICultureFormat
        {
            get
            {
                Thread currentThread = Thread.CurrentThread;

                //start with the current UI Culture...
                CultureInfo originalCulture = currentThread.CurrentUICulture;

                IFormatProvider provider = (originalCulture.IsNeutralCulture) ? currentThread.CurrentCulture : originalCulture;

                return provider;
            }
        }
        */

        /// <summary>
        /// Get a scaled bitmap for the provided icon, or null if no icon is provided.
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="imageSize"></param>
        /// <returns>A scaled image from the icon or null if no icon was provided.</returns>
        public static Image ScaleIcon(Icon icon, Size imageSize)
        {
            if (icon == null)
            {
                //nothing we can give.
                return null;
            }

            //use the provided icon.  We need to convert it to the dimension we want.
            Icon adjustedViewIcon = new Icon(icon, imageSize);
            Bitmap viewImage = adjustedViewIcon.ToBitmap();

            //and we might not get the size we want because it may not be present for the icon...
            if (viewImage.Size != imageSize)
            {
                Bitmap resizedImage = new Bitmap(imageSize.Width, imageSize.Height, PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(resizedImage))
                {
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(viewImage, 0, 0, resizedImage.Width, resizedImage.Height);
                }
                viewImage = resizedImage;
            }
            return viewImage;
        }
    }
}