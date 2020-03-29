﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UtilityHelper
{
    public class ClipboardHelper
    {
        /// <summary>
        /// Sets clipboard to value.
        /// </summary>
        /// <param name="value">String to set the clipboard to.</param>
        public static void SetClipboard(string value)
        {
            if (value == null)
                throw new ArgumentNullException("Attempt to set clipboard with null");

            Process clipboardExecutable = new Process
            {
                StartInfo = new ProcessStartInfo // Creates the process
                {
                    RedirectStandardInput = true,
                    FileName = @"clip",
                }
            };
            clipboardExecutable.Start();

            clipboardExecutable.StandardInput.Write(value); // CLIP uses STDIN as input.
            // When we are done writing all the string, close it so clip doesn't wait and get stuck
            clipboardExecutable.StandardInput.Close();

        }
    }
}
