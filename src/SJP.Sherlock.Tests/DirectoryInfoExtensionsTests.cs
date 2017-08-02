using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SJP.Sherlock.Tests
{
    [TestFixture]
    public class DirectoryInfoExtensionsTests
    {
        [Test]
        public void GetLockedFiles_WhenGivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo tmp = null;
            Assert.Throws<ArgumentNullException>(() => tmp.GetLockedFiles());
        }

        [Test]
        public void EnumerateLockedFiles_WhenGivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo tmp = null;
            Assert.Throws<ArgumentNullException>(() => tmp.EnumerateLockedFiles());
        }

        [Test]
        public void GetLockingProcesses_WhenGivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo tmp = null;
            Assert.Throws<ArgumentNullException>(() => tmp.GetLockingProcesses());
        }

        [Test]
        public void ContainsLockedFiles_WhenGivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo tmp = null;
            Assert.Throws<ArgumentNullException>(() => tmp.ContainsLockedFiles());
        }

        [Test]
        public void GetLockedFiles_WhenLockingOnPathInDirectory_ReturnsListOfLockedFiles()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            using (var file = File.Open(tmpDirFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var lockedFiles = tmpDir.GetLockedFiles();
                Assert.IsTrue(lockedFiles.Any());
            }

            tmpDir.Delete(true);
        }

        [Test]
        public void GetLockedFiles_WhenNotLockingOnPathInDirectory_ReturnsEmptyList()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            var lockedFiles = tmpDir.GetLockedFiles();
            Assert.IsFalse(lockedFiles.Any());

            tmpDir.Delete(true);
        }

        [Test]
        public void EnumerateLockedFiles_WhenLockingOnPathInDirectory_ReturnsListOfLockedFiles()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            using (var file = File.Open(tmpDirFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var lockedFiles = tmpDir.EnumerateLockedFiles();
                Assert.IsTrue(lockedFiles.Any());
            }

            tmpDir.Delete(true);
        }

        [Test]
        public void EnumerateLockedFiles_WhenNotLockingOnPathInDirectory_ReturnsEmptyList()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            var lockedFiles = tmpDir.EnumerateLockedFiles();
            Assert.IsFalse(lockedFiles.Any());

            tmpDir.Delete(true);
        }

        [Test]
        public void GetLockingProcesses_WhenLockingOnPathInDirectory_ReturnsCorrectProcess()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            using (var file = File.Open(tmpDirFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var lockedProcesses = tmpDir.GetLockingProcesses();
                var process = Process.GetCurrentProcess();

                var lockingId = lockedProcesses.Single().ProcessId;
                var currentId = process.Id;

                Assert.AreEqual(currentId, lockingId);
            }

            tmpDir.Delete(true);
        }

        [Test]
        public void GetLockingProcesses_WhenNotLockingOnPathInDirectory_ReturnsEmptySet()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            var lockedProceses = tmpDir.GetLockingProcesses();
            Assert.IsTrue(lockedProceses.Count == 0);

            tmpDir.Delete(true);
        }

        [Test]
        public void ContainsLockedFiles_WhenLockingOnPathInDirectory_ReturnsTrue()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            using (var file = File.Open(tmpDirFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var containsLockedFiles = tmpDir.ContainsLockedFiles();
                Assert.IsTrue(containsLockedFiles);
            }

            tmpDir.Delete(true);
        }

        [Test]
        public void ContainsLockedFiles_WhenNotLockingOnPathInDirectory_ReturnsFalse()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            var containsLockedFiles = tmpDir.ContainsLockedFiles();
            Assert.IsFalse(containsLockedFiles);

            tmpDir.Delete(true);
        }
    }
}
