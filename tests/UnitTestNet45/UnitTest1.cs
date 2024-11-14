﻿using ContextFreeTasks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest
{
    public class UnitTest1 : IDisposable
    {
        SingleThreadedSynchronizationContext _syncContext;

        public UnitTest1()
        {
            _syncContext = new SingleThreadedSynchronizationContext();
        }

        public void Dispose()
        {
            _syncContext.Dispose();
        }

#pragma warning disable xUnit1031
        [Fact]
        public void TestMethod1()
        {
            TestMethod1Async().Wait();
        }
#pragma warning restore xUnit1031

        private async Task TestMethod1Async()
        {
            await Task.Run(() => { });
            OnMainThread();
            await A1(10);
            OnMainThread();
            await B1(10);
            OnMainThread();
        }

        private void OnThread(int expectedThreadId)
        {
            var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            Assert.Equal(expectedThreadId, threadId);
        }

        private void OnMainThread() => OnThread(_syncContext.ThreadId);

        private void OnPoolThread()
        {
            var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            Assert.NotEqual(_syncContext.ThreadId, threadId);
        }

        private async Task<string> A1(int n)
        {
            OnMainThread();
            var s = await A2(n);
            OnMainThread();
            await Task.Delay(100);
            OnMainThread();
            await A3();
            OnMainThread();
            return s;
        }

        private async ContextFreeTask<string> A2(int n)
        {
            await Task.Delay(100);
            OnPoolThread();
            await Task.Delay(100);
            OnPoolThread();
            await A3();
            OnPoolThread();
            return n.ToString();
        }

        private async ContextFreeTask A3()
        {
            await Task.Delay(100);
            OnPoolThread();
        }

        private async Task<string> B1(int n)
        {
            OnMainThread();
            var s = await B2(n);
            OnMainThread();
            await Task.Delay(100);
            OnMainThread();
            await B3();
            OnMainThread();
            return s;
        }

        private async Task<string> B2(int n)
        {
            await Task.Delay(100);
            OnMainThread();
            await Task.Delay(100);
            OnMainThread();
            await B3();
            OnMainThread();
            return n.ToString();
        }

        private async Task B3()
        {
            await Task.Delay(100);
            OnMainThread();
        }
    }
}
