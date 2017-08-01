﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EnumsNET;
using static SJP.Sherlock.NativeMethods;

namespace SJP.Sherlock
{
    public static class ExceptionExtensions
    {
        public static bool IsFileLocked(this IOException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            // Generally it is not safe / stable to convert HRESULTs to Win32 error codes. It works here,
            // because we exactly know where we're at. So resist refactoring the following code into an
            // (maybe even externally visible) method.
            var numericErrorCode = GetHResult(exception) & ((1 << 16) - 1);

            if (!Enums.TryToObject<WinErrorCode>(numericErrorCode, out var errorCode))
                return false; // don't know the error code so we know it's at least not locked

            return errorCode == WinErrorCode.ERROR_LOCK_VIOLATION
                || errorCode == WinErrorCode.ERROR_SHARING_VIOLATION;
        }

        public static bool RethrowWithLockingInformation(this Exception exception, params string[] fileNames) =>
            RethrowWithLockingInformation(exception, fileNames as IEnumerable<string>);

        public static bool RethrowWithLockingInformation(this Exception exception, IEnumerable<string> fileNames)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            if (fileNames == null)
                throw new ArgumentNullException(nameof(fileNames));

            var ioex = exception as IOException;
            if (ioex == null || !ioex.IsFileLocked())
                return false;

            var lockers = RestartManager.GetLockingProcesses(fileNames);
            if (lockers.Count == 0)
                return false;

            const int max = 10;
            var sb = new StringBuilder();
            sb.Append(exception.Message);
            sb.Append(" ");

            var message = FormatLockingMessage(lockers, fileNames, max);
            sb.Append(message);

            // Unable to set HResult *and* InnerException via public methods/ctors.
            // Must use reflection to set the HResult while using the ctor to set the InnerException.
            // Nasty but necessary.
            var ex = new IOException(sb.ToString(), exception);
            SetErrorCodeMethod?.Invoke(ex, new object[] { GetHResult(exception) });

            throw ex;
        }

        private static string FormatLockingMessage(IEnumerable<IProcessInfo> lockers, IEnumerable<string> fileNames, int? max = null)
        {
            if (lockers == null || !lockers.Any())
                return string.Empty;

            var lockerList = lockers.ToList();

            fileNames = fileNames ?? Enumerable.Empty<string>();
            var fileNameList = fileNames.ToList();
            if (fileNameList.Count == 0)
                throw new ArgumentException("At least one filename must be provided, none given.", nameof(fileNames));

            var builder = new StringBuilder();

            var fileNameCount = fileNames.Count();
            if (fileNameCount == 1)
                builder.AppendFormat("File {0} locked by: ", fileNameList[0]);
            else
                builder.AppendFormat("Files [{0}] locked by: ", string.Join(", ", fileNameList));

            var truncatedLockers = lockerList.Take(max ?? Int32.MaxValue);
            foreach (var locker in truncatedLockers)
            {
                builder
                    .Append("[")
                    .Append(locker.ApplicationName)
                    .Append(", pid=")
                    .Append(locker.ProcessId)
                    .Append(", started ")
                    .Append(locker.StartTime.ToString("o"))
                    .AppendLine("]");
            }

            var count = lockerList.Count;
            if (count > max)
            {
                builder
                    .Append("[")
                    .Append(count - max)
                    .AppendLine(" more processes...]");
            }

            return builder.ToString();
        }

        private static int GetHResult(Exception ex)
        {
#if NET40
            return (int)HResultProperty.GetValue(ex, null);
#else
            return ex.HResult;
#endif
        }

        private static MethodInfo SetErrorCodeMethod => _setErrorCodeMethod.Value;

#if NETFX
        private static readonly Lazy<MethodInfo> _setErrorCodeMethod = new Lazy<MethodInfo>(() =>
            typeof(Exception)
                .GetMethod("SetErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod));
#else
        private readonly static Lazy<MethodInfo> _setErrorCodeMethod = new Lazy<MethodInfo>(() =>
            typeof(Exception)
                .GetTypeInfo()
                .GetMethod("SetErrorCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod));
#endif

#if NET40
        private static PropertyInfo HResultProperty => _getHResultProperty.Value;

        private readonly static Lazy<PropertyInfo> _getHResultProperty = new Lazy<PropertyInfo>(() =>
            typeof(Exception)
                .GetProperty("HResult", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod));
#endif
    }
}