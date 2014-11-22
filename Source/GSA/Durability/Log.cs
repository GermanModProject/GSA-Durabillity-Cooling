﻿///////////////////////////////////////////////////////////////////////////////
//
//    Durability a plugin for Kerbal Space Program from SQUAD
//    (https://www.kerbalspaceprogram.com/)
//    and part of GSA Mod
//    (http://www.kerbalspaceprogram.de)
//
//    Author: runner78
//    Copyright (c) 2014 runner78
//
//    This program, coding and graphics are provided under the following Creative Commons license.
//    Attribution-NonCommercial 3.0 Unported
//    https://creativecommons.org/licenses/by-nc/3.0/
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GSA.Durability
{
    static class Debug
    {
        public static bool debug = true;

        public static void Log(object message)
        {
            if (debug)
                UnityEngine.Debug.Log(message);
        }
        public static void Log(object message, UnityEngine.Object context)
        {
            if (debug)
                UnityEngine.Debug.Log(message, context);
        }

        public static void LogError(object message)
        {
            if (debug)
                UnityEngine.Debug.LogError(message);
        }
        public static void LogError(object message, UnityEngine.Object context)
        {
            if (debug)
                UnityEngine.Debug.LogError(message, context);
        }

        public static void LogWarning(object message)
        {
            if (debug)
                UnityEngine.Debug.LogWarning(message);
        }
        public static void LogWarning(object message, UnityEngine.Object context)
        {
            if (debug)
                UnityEngine.Debug.LogWarning(message, context);
        }

        public static void LogException(Exception exception)
        {
            if (debug)
                UnityEngine.Debug.LogException(exception);
        }
        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            if (debug)
                UnityEngine.Debug.LogException(exception, context);
        }
    }
}
