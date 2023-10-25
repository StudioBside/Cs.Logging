﻿namespace Cs.Logging.Test;

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class OutputTest
{
    [TestMethod]
    public void Provider_커스텀()
    {
        var provider = new MockProvider();
        Log.Initialize(provider, LogLevelConfig.All);
        
        Log.Debug("custom provider test");
        Assert.AreEqual("[DEBUG] custom provider test (OutputTest.cs:15)", provider.Buffer);
    }

    [TestMethod]
    public void 파일위치_라인수_옵션조정()
    {
        var provider = new MockProvider();
        Log.Initialize(provider, LogLevelConfig.All);
        
        Log.WriteFileLine = false;
        Log.Debug("custom provider test");
        Assert.AreEqual("[DEBUG] custom provider test", provider.Buffer);

        Log.WriteFileLine = true;
        Log.Debug("custom provider test");
        Assert.AreEqual("[DEBUG] custom provider test (OutputTest.cs:30)", provider.Buffer);
    }

    private sealed class MockProvider : ILogProvider
    {
        public string Buffer { get; set; } = string.Empty;
        public string BuildTag(string file, int line)
        {
            return string.Intern($"{Path.GetFileName(file)}:{line}");
        }

        public void Debug(string message)
        {
            this.Buffer = $"[DEBUG] {message}";
        }

        public void DebugBold(string message)
        {
            this.Buffer = $"[DEBUG] {message}";
        }

        public void Error(string message)
        {
            this.Buffer = $"[ERROR] {message}";
        }

        [DoesNotReturn]
        public void ErrorAndExit(string message)
        {
            throw new NotImplementedException();
        }

        public void Info(string message)
        {
            this.Buffer = $"[INFO] {message}";
        }

        public void Warn(string message)
        {
            this.Buffer = $"[WARN] {message}";
        }
    }
}
