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
#region File Header

// /********************************************************************
//  * COPYRIGHT:
//  *    This software program is furnished to the user under license
//  *    by Gibraltar Software, Inc, and use thereof is subject to applicable 
//  *    U.S. and international law. This software program may not be 
//  *    reproduced, transmitted, or disclosed to third parties, in 
//  *    whole or in part, in any form or by any manner, electronic or
//  *    mechanical, without the express written consent of Gibraltar Software, Inc,
//  *    except to the extent provided for by applicable license.
//  *
//  *    Copyright © 2008 by Gibraltar Software, Inc.  All rights reserved.
//  *******************************************************************/

using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using Loupe.Extensibility.Data;

#endregion

namespace Gibraltar.Monitor.Windows.Internal
{
    /// <summary>
    /// This class listens for a specified hotkey and invokes a specified delegate when the hotkey is hit.
    /// </summary>
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    internal class MessageFilter : IMessageFilter
    {
        /// <summary>
        /// The default Hot Key for activating the message filter (not combined with modifiers).
        /// </summary>
        public const Keys DefaultHotKey = Keys.F5;

        /// <summary>
        /// The set of valid modifier keys for configuring a hot key.
        /// </summary>
        public const Keys ValidModifiers = (Keys.Shift | Keys.Control | Keys.Alt); // Only considers these generic three.

        private const int WM_KEYDOWN = 0x100;

        /// <summary>
        /// The set of separators used in parsing hotkey configuration strings.
        /// </summary>
        private static readonly char[] HotKeySpliters = new[] { ' ', '-', '+', ',', ';', '|' }; // Only make this once.

        /*
        private readonly int m_HotKeyCode;
        private readonly Keys m_Modifiers;
        private readonly MethodInvoker m_InvokerDelegate;
        */
        private readonly IDictionary<Keys, MethodInvoker> m_HotKeyMap;
        private volatile bool m_Disabled; // Turn off message filtering during HotKeyMap changes.

        private readonly Dictionary<int, bool> m_Activated = new Dictionary<int, bool>(); // Protected by LOCKING itself.

        /*
        /// <summary>
        /// Accesses the KeyCode (as an int) for the hotkey configured in this instance.
        /// </summary>
        public int KeyCode { get { return m_HotKeyCode; } }

        /// <summary>
        /// Accesses the Modifiers (as a Keys enum value) for the hotkey configured in this instance.
        /// </summary>
        public Keys Modifiers { get { return m_Modifiers; } }
        */

        /// <summary>
        /// Access the collection mapping hotkeys to delegates, which may be modified dynamically.  (See Disable property.)
        /// </summary>
        public IDictionary<Keys, MethodInvoker> HotKeyMap { get { return m_HotKeyMap; } }

        /// <summary>
        /// Temporarily disable message filtering during changes to HotKeyMap for thread-safety.
        /// </summary>
        public bool Disable { get { return m_Disabled; } set { m_Disabled = value; } }

        /// <summary>
        /// Indicates whether this instance has ever been activated.
        /// </summary>
        /// <remarks>Being Active does not guarantee that the MessageFilter is successfully receiving keyboard events, only
        /// that it has been added as a message filter on some thread (only WinForms UI threads will function properly).</remarks>
        public bool Active { get; private set; }

        /// <summary>
        /// Indicates whether this instance has been activated on the current thread.
        /// </summary>
        public bool ActiveThisThread
        {
            get
            {
                bool active;
                int threadIndex = ThreadInfo.GetCurrentThreadIndex();
                lock (m_Activated) // Apparently Dictionaries are not internally threadsafe.
                {
                    if (m_Activated.TryGetValue(threadIndex, out active) == false)
                        active = false;
                }

                return active;
            }
            private set
            {
                int threadIndex = ThreadInfo.GetCurrentThreadIndex();
                lock (m_Activated) // Apparently Dictionaries are not internally threadsafe.
                {
                    m_Activated[threadIndex] = value;
                }
            }
        }

        /*
        /// <summary>
        /// Indicates whether this instance was initialized with valid settings.
        /// </summary>
        /// <remarks>This can be used to check the success of construction.  An invalid instance can not be
        /// meaningfully activated, and is thus useless.  Calling Activate() in an invalid instance does nothing.</remarks>
        public bool IsValid
        {
            get
            {
                return (m_HotKeyCode != (int) Keys.None && m_InvokerDelegate != null);
            }
        }
        */

        /// <summary>
        /// Create a MessageFilter looking for a specified hotkey to invoke a specified delegate.
        /// </summary>
        /// <remarks>Once activated, the MessageFilter will look for key-down messages matching the
        /// specified Keys.KeyCode portion of the specified hotKey and confirm that the modifiers
        /// currently held also match those specified in the Keys.Modifiers portion of the specified hotKey.
        /// See System.Windows.Forms.Keys enum for more on key codes and modifiers.</remarks>
        /// <param name="hotKey">The hotkey as a KeyCode combined with desired Modifiers (see Keys enum).</param>
        /// <param name="invokerDelegate">A delegate instance of a method taking no arguments and void return.</param>
        public MessageFilter(Keys hotKey, MethodInvoker invokerDelegate)
            : this(new Dictionary<Keys, MethodInvoker>(1))
        {
            if ((hotKey & Keys.KeyCode) != Keys.None)
                m_HotKeyMap[hotKey] = invokerDelegate; // Set the mapping for the one hotkey they wanted.

            /*
            Active = false; // Initialize the auto-property.  This mechanism may change later.
            m_Modifiers = Keys.None; // Default for no valid hotkey or no valid delegate.

            m_InvokerDelegate = invokerDelegate;
            if (m_InvokerDelegate != null)
            {
                m_HotKeyCode = (int) (hotKey & Keys.KeyCode);
                if (m_HotKeyCode != (int) Keys.None)
                    m_Modifiers = (hotKey & ValidModifiers);
            }
            else
            {
                m_HotKeyCode = (int) Keys.None;
            }
            */
        }

        /// <summary>
        /// Create a MessageFilter looking for a dynamically-changeable set of hotkey to invoke mapped delegates.
        /// </summary>
        /// <remarks>Once activated, the MessageFilter will look for key-down messages and match any keystroke (KeyCode
        /// combined with Modifiers, such as produced by ParseHotKeyString) found in the hotKeyMap, and invoke its mapped
        /// delegate.  The caller may retain the hotKeyMap to make changes, which relies on the dictionary's internal
        /// threadsafe-atomic locking for single changes at a time (to avoid holding a longer lock while blocking all
        /// keystrokes!).
        /// See System.Windows.Forms.Keys enum for more on key codes and modifiers.</remarks>
        /// <param name="hotKeyMap">A mapping of Keys to MethodInvoker delegates.</param>
        public MessageFilter(IDictionary<Keys,MethodInvoker> hotKeyMap)
        {
            Active = false; // Initialize the auto-property.  This mechanism may change later.
            m_HotKeyMap = hotKeyMap;
        }

        /// <summary>
        /// Activate this MessageFilter to begin listening for hotkeys on the current thread.
        /// </summary>
        /// <remarks>This MUST be called while on the desired UI thread, or the message filter will not receive
        /// any messages.  It should be possible to Activate the same MessageFilter instance on multiple UI threads,
        /// but this has not been confirmed.</remarks>
        public void Activate()
        {
            if (ActiveThisThread)
                return; // Guard against redundant activation.

            Active = true; // Do this first, in case we get an immediate hit on the filter.
            ActiveThisThread = true;
            Application.AddMessageFilter(this);
        }

        /// <summary>
        /// Activate this MessageFilter to begin listening for hotkeys on the UI thread of a specified Form or Control.
        /// </summary>
        /// <remarks>This will automatically BeginInvoke() as needed to the UI thread of the given Control in order
        /// to attach as a MessageFilter on that thread's message pump.</remarks>
        /// <param name="control">A windows Control (or Form) on the UI thread to listen on.</param>
        public void Activate(Control control)
        {
            if (control.InvokeRequired)
                control.BeginInvoke(new MethodInvoker(Activate)); // Activate it on the right thread.
            else
                Activate();
        }

        /// <summary>
        /// Deactivate this MessageFilter to stop listening for hotkeys on the current thread.
        /// </summary>
        /// <remarks>This MUST be called while on the same UI thread on which it was activated.  It will remain active
        /// on any other threads that have been activated.</remarks>
        public void Deactivate()
        {
            Application.RemoveMessageFilter(this);
            ActiveThisThread = false;
        }

        /// <summary>
        /// Deactivate this MessageFilter to stop listening for hotkeys on the UI thread of a specified Form or Control.
        /// </summary>
        /// <remarks>This will automatically BeginInvoke() as needed to the UI thread of the given Control in order
        /// to remove itself as a MessageFilter on that thread's message pump.</remarks>
        /// <param name="control">A windows Control (or Form) on the UI thread to stop listening on.</param>
        public void Deactivate(Control control)
        {
            if (control.InvokeRequired)
                control.BeginInvoke(new MethodInvoker(Deactivate)); // Deactivate it on the right thread.
            else
                Deactivate();
        }

        #region IMessageFilter Members

        /// <summary>
        /// Filters out a message before it is dispatched.
        /// </summary>
        /// <remarks>This IMessageFilter method is being used to listen for a configured hotkey, and will invoke
        /// the configured delegate when the hotkey is pressed while the appropriate modifiers are held.</remarks>
        /// <param name="m">The message to be dispatched. You cannot modify this message.</param>
        /// <returns>True to filter the message and stop it from being dispatched; false to allow the message
        /// to continue to the next filter or control.</returns>
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN)
            {
                if (Active)
                {
                    // We must not block here, so we can't use a lock.  Best we can do is a quick check of a volatile flag.
                    if (m_Disabled == false) // If our HotKeyMap is in transition, don't touch it; just skip over the filter.
                    {
                        Keys keyCode = (Keys)m.WParam.ToInt32();
                        Keys modifiers = Control.ModifierKeys & ValidModifiers;
                        Keys combined = modifiers | keyCode;

                        // Now look up the keystroke in our hotkey map.
                        MethodInvoker invokerDelegate;
                        if (keyCode != Keys.None && m_HotKeyMap.TryGetValue(combined, out invokerDelegate))
                        {
                            try
                            {
                                if (invokerDelegate != null) // Double-check for an invalid delegate.
                                    invokerDelegate(); // Call the assigned delegate.

                                return true; // True means it's handled, don't pass it on to application.
                            }
                            catch
                            {
                                Log.DebugBreak(); // Catch this in the debugger.
                                // ToDo: use error notifier or something to report this sensibly.
                            }
                        }
                    }
                }
                else
                {
                    // Uh-oh, we've been master-deactivated, so we need to remove from any thread as it hits this.
                    Deactivate();
                }
            }
            return false; // False for all other cases, or it breaks them.
        }

        #endregion

        /// <summary>
        /// A static helper method to parse a string specifying a KeyCode and Modifiers for use with this class.
        /// </summary>
        /// <remarks>The key string may specify any combination of the three general modifiers (Shift, Ctrl, Alt) and a
        /// single primary KeyCode (Function keys and letters are recommended; otherwise results may be unpredictable),
        /// separated in any order by: spaces, dashes, commas, semi-colons, plus signs or vertical bars.
        /// If more than one KeyCode is recognized in the string, only the final one is used.</remarks>
        /// <param name="keyString">The key string to parse.</param>
        /// <returns>A Keys value combining the KeyCode and Modifiers parsed from the specified string.</returns>
        public static Keys ParseHotKeyString(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
                return Keys.None; // No valid keyString argument, so no valid hotkey is found.

            Keys hotKeyCode = Keys.None; // Initialize to an empty value in case no primary KeyCode is found.
            Keys modifiers = Keys.None; // 0?

            string[] keys = keyString.ToLowerInvariant().Split(HotKeySpliters , StringSplitOptions.RemoveEmptyEntries);

            foreach (string key in keys)
            {
                try
                {
                    // We removed empty entries, so no nulls or empty strings should be here.
                    char firstChar = key[0]; // First character, already mapped to lowercase above.
                    string parseKey = key;

                    switch (firstChar)
                    {
                        case 's':
                            if (key == "shift" || key == "sh")
                            {
                                modifiers |= Keys.Shift;
                                continue; // Next key in keys
                            }
                            break;
                        case 'c':
                            if (key == "ctrl" || key == "control" || key == "ct")
                            {
                                modifiers |= Keys.Control;
                                continue; // Next key in keys
                            }
                            break;
                        case 'a':
                            if (key == "alt" || key == "al")
                            {
                                modifiers |= Keys.Alt;
                                continue; // Next key in keys
                            }
                            break;
                        default:
                            if (firstChar >= '0' && firstChar <= '9') // Map single digits for them.
                            {
                                // Enums labels can't start with digits, so they define them as "D0" - "D9".
                                if (key.Length == 1)
                                    parseKey = "D" + key; 
                                // Don't continue to next key, we still need to parse this case below.
                            }
                            else if (firstChar < 'a' || firstChar > 'z') // Outside of alpha range.
                            {
                                // Non-alpha (and non-digit) starting character is garbage.
                                Log.Write(LogMessageSeverity.Warning, "Gibraltar.Agent", "Invalid Hotkey Configuration", "Bad substring \"{0}\" in hotkey string \"{1}\".",
                                          key, keyString);
                                continue; // Won't parse this one, avoid the exception on it, try next key in keys.
                            }
                            break;
                    }

                    Keys parsedKeyCode = ParseKeyCode(parseKey); // Check the one we modified, or just the original.
                    if (parsedKeyCode != Keys.None)
                    {
                        hotKeyCode = parsedKeyCode; // Only set it if it was a valid KeyCode.
                    }
                    else
                    {
                        Log.Write(LogMessageSeverity.Warning, "Gibraltar.Agent", "Invalid Hotkey Configuration", "Unrecognizable KeyCode \"{0}\" in hotkey string \"{1}\".",
                                  key, keyString);
                    }
                }
                catch (Exception ex)
                {
                    // We catch exceptions in ParseKeyCode(), so this probably won't be hit, but just in case...
                    Log.Write(LogMessageSeverity.Warning, LogWriteMode.Queued, ex, "Gibraltar.Agent", "Invalid Hotkey Configuration", "Exception parsing expected KeyCode \"{0}\" in hotkey string \"{1}\".",
                              key, keyString);
                }
            }

            if (hotKeyCode != Keys.None)
            {
                return hotKeyCode | modifiers; // Combine key code and modifiers.
            }
            else
            {
                return Keys.None; // Didn't find a valid hotKeyCode, no hotkey gets set.
            }
        }

        /// <summary>
        /// Parse a Keys enum value from a string.
        /// </summary>
        /// <remarks>This function weeds out Modifier enum values, returning only the KeyCode portion.</remarks>
        /// <param name="key">The string label of a Keys enum KeyCode value.</param>
        /// <returns>The corresponding KeyCode, or Keys.None on any error.</returns>
        private static Keys ParseKeyCode(string key)
        {
            Keys parsedKeyCode;
            try
            {
                parsedKeyCode = (Keys) Enum.Parse(typeof (Keys), key, true); // Try to match by enum label, case-insensitive.
                parsedKeyCode &= Keys.KeyCode; // Make sure it's only a key code, no modifiers.
            }
            catch
            {
                parsedKeyCode = Keys.None; // Parsing error, return an invalid key.
            }
            return parsedKeyCode;
        }

        /// <summary>
        /// Converts a Keys enum value for a hotkey into a canonical string form for display.
        /// </summary>
        /// <param name="hotKey">The Keys enum for the hotkey.</param>
        /// <returns>A string describing the hotkey keystroke in canonical form, suitable for display.</returns>
        public static string HotKeyToString(Keys hotKey)
        {
            Keys keyCode = hotKey & Keys.KeyCode;
            Keys modifiers = hotKey & Keys.Modifiers;
            string[] KeySpliters =  new[] { " | ", " , ", ", ", ",", "|" };

            //string[] names = hotKey.ToString().Split( KeySpliters, StringSplitOptions.RemoveEmptyEntries);
            string keyName = keyCode.ToString();
            string modName = (modifiers == Keys.None) ? string.Empty : modifiers.ToString();
            string[] modNames = modName.Split(KeySpliters, StringSplitOptions.RemoveEmptyEntries);

            if (modNames.Length > 0)
            {
                modName = string.Join("-", modNames);
                keyName = modName + "-" + keyName;
                if (keyCode == Keys.None) // If there are modifiers but no keycode...
                    return modName; // ...return just the modifiers (for separate display).
            }

            return keyName; // With no keyCode or modifiers, this will be "None".
        }

        /*
        public static string GetHotKey(string keyString)
        {
            string[] keys = keyString.ToUpperInvariant().Split(new[] { ' ', ',', ';', '+', '-' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string key in keys)
            {
                switch (key)
                {
                    case "SHIFT":
                    case "CTRL":
                    case "ALT":
                        break;
                    default:
                        return key;
                }
            }
            return DefaultHotKey.ToString();
        }

        public static string GetQualifier(string keyString)
        {
            string[] keys = keyString.ToUpperInvariant().Split(new[] { ' ', ',', ';', '+', '-' }, StringSplitOptions.RemoveEmptyEntries);
            bool shift = false;
            bool ctrl = false;
            bool alt = false;
            foreach (string key in keys)
            {
                switch (key)
                {
                    case "SHIFT":
                        shift = true;
                        break;
                    case "CTRL":
                        ctrl = true;
                        break;
                    case "ALT":
                        alt = true;
                        break;

                    default:
                        break;
                }
            }

            // Now that we know which modifiers apply, formulate a string.
            // We use this somewhat involved logic because we want the 
            // strings to match the list in the combobox on the form.

            // Note: I've moved Shift first (rather than last).  Where's the combobox which needs to match this?
            string modifiers;

            if (shift)
                modifiers = "Shift";
            else
                modifiers = string.Empty;

            if (ctrl)
            {
                if (modifiers.Length > 0)
                    modifiers += "+";

                modifiers += "Ctrl";
            }
            if (alt)
            {
                if (modifiers.Length > 0)
                    modifiers += "+";

                modifiers += "Alt";
            }
            if (modifiers.Length == 0)
            {
                modifiers = "None";
            }
            return modifiers;
        }
        */
    }
}
