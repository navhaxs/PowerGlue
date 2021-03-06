﻿using System;
using System.Diagnostics;
using FileLock.FileSys;
using System.Linq;

namespace FileLock
{
    public class SimpleFileLock : IFileLock
    {
        protected SimpleFileLock(string lockName)
        {
            LockName = lockName;
        }

        public string LockName { get; private set; }

        private string LockFilePath { get; set; }

        public bool TestLockIsFree()
        {
            if (LockIO.LockExists(LockFilePath))
            {
                var lockContent = LockIO.ReadLock(LockFilePath);

                //Someone else owns the lock
                if (lockContent.GetType() == typeof(OtherProcessOwnsFileLockContent))
                {
                    return false;
                }

                //the file no longer exists
                if (lockContent.GetType() == typeof(MissingFileLockContent))
                {
                    return AcquireLock();
                }

                //This lock belongs to this process - we can reacquire the lock
                if (lockContent.PID == Process.GetCurrentProcess().Id)
                {
                    return AcquireLock();
                }

                // Check if PID is still alive
                if (Process.GetProcesses().Any(x => x.Id == lockContent.PID))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryAcquireLock()
        {
            bool lockIsFree = TestLockIsFree();
            if (lockIsFree)
            {
                //Acquire the lock

                return AcquireLock();
            } else
            {
                return false;
            }
        }

        public bool ReleaseLock()
        {
            //Need to own the lock in order to release it (and we can reacquire the lock inside the current process)
            if (LockIO.LockExists(LockFilePath) && TryAcquireLock())
                LockIO.DeleteLock(LockFilePath);
            return true;
        }

        #region Internal methods

        protected FileLockContent CreateLockContent()
        {
            var process = Process.GetCurrentProcess();
            return new FileLockContent()
            {
                PID = process.Id,
                ProcessName = process.ProcessName
            };
        }

        private bool AcquireLock()
        {
            return LockIO.WriteLock(LockFilePath, CreateLockContent());
        }

        #endregion

        #region Create methods

        public static SimpleFileLock Create(string lockName)
        {
            if (string.IsNullOrEmpty(lockName))
                throw new ArgumentNullException("lockName", "lockName cannot be null or emtpy.");

            return new SimpleFileLock(lockName) { LockFilePath = LockIO.GetFilePath(lockName) };
        }

        #endregion
    }
}