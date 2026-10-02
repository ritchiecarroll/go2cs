// Test code based on source from Chan4Net:
// https://github.com/superopengl/Chan4Net
// Copyright (c) 2016 Jun Shao

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace ChannelTests
{
    [TestClass]
    public class BufferedTests
    {
        private readonly TimeSpan m_awaitTimeout = TimeSpan.FromMilliseconds(1000);
        private readonly TimeSpan m_slowActionLatency = TimeSpan.FromMilliseconds(500);

        [TestMethod]
        public void Ctor_ChanSizeZero_IsUnbuffered()
        {
            // Size 0 is the VALID unbuffered channel (make(chan T)) since the channels
            // redesign: cap and len are both 0 and no buffer exists. Negative sizes throw.
            channel<int> sut = new channel<int>(0);

            Assert.AreEqual(0, sut.Capacity);
            Assert.AreEqual(0, sut.Length);
            Assert.IsTrue(sut.IsUnbuffered);
        }

        // Go's make(chan T, n) with a negative n panics "makechan: size out of range" — a
        // runtime.Error that recover() sees — and the constructor raises exactly that. It raised
        // the CLR's ArgumentOutOfRangeException until Go's runtime panics were given their Go
        // values, and this test still expected the CLR exception.
        [TestMethod]
        public void Ctor_ChanSizeNegative_ShouldThrow()
        {
            PanicException panic = Assert.ThrowsException<PanicException>(() => new channel<int>(-1));

            Assert.AreEqual("makechan: size out of range", panic.Message);
        }

        [TestMethod]
        public void Send_LessThanSize_SendShouldNotBeBlocked()
        {
            channel<int> sut = new channel<int>(2);
            bool called = false;

            Task producer = Task.Run(() =>
            {
                sut.Send(1);
                sut.Send(2);
                called = true;
            });

            producer.Wait();

            Assert.AreEqual(2, sut.Length);
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void Send_AfterClosed_ShouldThrow()
        {
            channel<int> sut = new channel<int>(2);
            sut.Send(1);
            sut.Close();

            Exception exception = null;
            try
            {
                sut.Send(2);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.AreEqual(1, sut.Length);
            Assert.IsTrue(sut.IsClosed);
            Assert.IsInstanceOfType(exception, typeof (PanicException));
        }

        [TestMethod]
        public void Send_MoreThanSize_SendShouldBeBlocked()
        {
            channel<int> sut = new channel<int>(2);
            bool called = false;
            Task producer = Task.Run(() =>
            {
                sut.Send(1);
                sut.Send(2);
                sut.Send(3);
                called = true;
            });

            producer.Wait(m_awaitTimeout);

            Assert.AreEqual(2, sut.Length);
            Assert.IsFalse(called);
        }

        [TestMethod]
        public void SendMany_ReceiveFew_SendShouldBeBlocked()
        {
            channel<int> sut = new channel<int>(2);
            bool? called = null;
            Task producer = Task.Run(() =>
            {
                sut.Send(1);
                sut.Send(2);
                sut.Send(3);
                sut.Send(4);
                sut.Send(5);
                called = false;
                sut.Send(6);
                called = true;
            });

            List<int> items = new List<int> {sut.Receive(), sut.Receive(), sut.Receive()};

            producer.Wait(m_awaitTimeout);

            Assert.AreEqual(2, sut.Length);
            Assert.IsFalse(called.GetValueOrDefault());
            CollectionAssert.AreEquivalent(new[] {1, 2, 3}, items.ToArray());
        }

        [TestMethod]
        public void SendFew_ReceiveMany_ReceiveShouldBeBlocked()
        {
            channel<int> sut = new channel<int>(2);
            sut.Send(1);
            sut.Send(2);

            bool? called = null;
            List<int> items = new List<int>();
            Task consumer = Task.Run(() =>
            {
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                called = false;
                items.Add(sut.Receive());
                called = true;
            });

            consumer.Wait(m_awaitTimeout);

            Assert.AreEqual(0, sut.Length);
            Assert.IsFalse(called.GetValueOrDefault());
            CollectionAssert.AreEquivalent(new[] {1, 2}, items.ToArray());
        }

        [TestMethod]
        public void Receive_FromEmptyChan_ReceiveShouldBeBlocked()
        {
            channel<int> sut = new channel<int>(2);
            bool called = false;
            Task producer = Task.Run(() =>
            {
                _ = sut.Receive();
                called = true;
            });

            producer.Wait(m_awaitTimeout);

            Assert.AreEqual(0, sut.Length);
            Assert.IsFalse(called);
        }

        [TestMethod]
        public void Receive_FromEmptyChanAfterClosed_ShouldReturnZero()
        {
            channel<int> sut = new channel<int>(2);
            sut.Send(1);
            sut.Send(2);
            sut.Close();
            Exception exception = null;
            int result;
            try
            {
                Assert.AreEqual(1, sut.Receive());
                Assert.AreEqual(2, sut.Receive());
                Assert.AreEqual(default(int), sut.Receive());
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.AreEqual(0, sut.Length);
            Assert.IsTrue(sut.IsClosed);
            Assert.IsNull(exception);
        }

        [TestMethod]
        public void Receive_FromNonEmptyChan_ShouldNotBeBlocked()
        {
            channel<int> sut = new channel<int>(2);
            sut.Send(1);
            sut.Send(2);

            int item1 = sut.Receive();
            int item2 = sut.Receive();

            Assert.AreEqual(0, sut.Length);
            Assert.AreEqual(1, item1);
            Assert.AreEqual(2, item2);
        }

        [TestMethod]
        public void Yield_NotClosedChan_ShouldBeBlocked()
        {
            channel<int> sut = new channel<int>(2);
            Task producer = Task.Run(() =>
            {
                sut.Send(1);
                sut.Send(2);
                sut.Send(3);
            });

            List<int> items = new List<int>();
            bool called = false;
            Task consumer = Task.Run(() =>
            {
                foreach (int i in sut)
                {
                    items.Add(i);
                }
                called = true;
            });

            producer.Wait();
            consumer.Wait(m_awaitTimeout);

            Assert.AreEqual(3, items.Count);
            Assert.IsFalse(called);
        }

        [TestMethod]
        public void Yield_ClosedChan_ShouldNotBeBlocked()
        {
            channel<int> sut = new channel<int>(2);
            Task producer = Task.Run(() =>
            {
                sut.Send(1);
                sut.Send(2);
                sut.Send(3);
            });

            List<int> items = new List<int>();
            bool called = false;
            Task consumer = Task.Run(() =>
            {
                foreach (int i in sut)
                {
                    items.Add(i);
                }
                called = true;
            });

            producer.Wait();
            sut.Close();
            //consumer.Wait(m_awaitTimeout);
            consumer.Wait();

            Assert.AreEqual(3, items.Count);
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void ProducerConsumer_MoreThanChanSize()
        {
            channel<int> sut = new channel<int>(2);
            bool producerCalled = false;
            int totalItemCount = 100;
            Task producer = Task.Run(() =>
            {
                for (int i = 1; i <= totalItemCount; i++)
                {
                    sut.Send(i);
                }
                producerCalled = true;
            });

            bool consumerCalled = false;
            List<int> items = new List<int>();
            Task consumer = Task.Run(() =>
            {
                for (int i = 1; i <= totalItemCount; i++)
                {
                    items.Add(sut.Receive());
                }
                consumerCalled = true;
            });

            Task.WaitAll(producer, consumer);

            Assert.AreEqual(0, sut.Length);
            Assert.IsTrue(producerCalled);
            Assert.IsTrue(consumerCalled);
            CollectionAssert.AreEquivalent(Enumerable.Range(1, totalItemCount).ToArray(), items.ToArray());
        }

        [TestMethod]
        public void ProducerConsumer_SlowProducer_FastConsumer()
        {
            channel<int> sut = new channel<int>(2);
            bool producerCalled = false;
            Task producer = Task.Run(async () =>
            {
                await Task.Delay(m_slowActionLatency);
                sut.Send(1);
                await Task.Delay(m_slowActionLatency);
                sut.Send(2);
                await Task.Delay(m_slowActionLatency);
                sut.Send(3);
                producerCalled = true;
            });

            bool consumerCalled = false;
            List<int> items = new List<int>();
            Task consumer = Task.Run(() =>
            {
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                consumerCalled = true;
            });

            Task.WaitAll(producer, consumer);

            Assert.AreEqual(0, sut.Length);
            Assert.IsTrue(producerCalled);
            Assert.IsTrue(consumerCalled);
            CollectionAssert.AreEquivalent(new[] {1, 2, 3}, items.ToArray());
        }

        [TestMethod]
        public void ProducerConsumer_FastProducer_SlowConsumer()
        {
            channel<int> sut = new channel<int>(2);
            bool producerCalled = false;
            Task producer = Task.Run(() =>
            {
                sut.Send(1);
                sut.Send(2);
                sut.Send(3);
                producerCalled = true;
            });

            bool consumerCalled = false;
            List<int> items = new List<int>();
            Task consumer = Task.Run(async () =>
            {
                await Task.Delay(m_slowActionLatency);
                items.Add(sut.Receive());
                await Task.Delay(m_slowActionLatency);
                items.Add(sut.Receive());
                await Task.Delay(m_slowActionLatency);
                items.Add(sut.Receive());
                consumerCalled = true;
            });

            Task.WaitAll(producer, consumer);

            Assert.AreEqual(0, sut.Length);
            Assert.IsTrue(producerCalled);
            Assert.IsTrue(consumerCalled);
            CollectionAssert.AreEquivalent(new[] {1, 2, 3}, items.ToArray());
        }

        [TestMethod]
        public void ProducerConsumer_MultipleProducers_MultipleConsumers()
        {
            channel<int> sut = new channel<int>(2);
            bool producer1Called = false;
            Task producer1 = Task.Run(() =>
            {
                sut.Send(1);
                sut.Send(2);
                sut.Send(3);
                producer1Called = true;
            });

            bool producer2Called = false;
            Task producer2 = Task.Run(() =>
            {
                sut.Send(4);
                sut.Send(5);
                sut.Send(6);
                producer2Called = true;
            });

            ConcurrentBag<int> items = new ConcurrentBag<int>();
            bool consumer1Called = false;
            Task consumer1 = Task.Run(() =>
            {
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                consumer1Called = true;
            });

            bool consumer2Called = false;
            Task consumer2 = Task.Run(() =>
            {
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                consumer2Called = true;
            });

            Task.WaitAll(producer1, producer2, consumer1, consumer2);

            Assert.AreEqual(0, sut.Length);
            Assert.IsTrue(producer1Called);
            Assert.IsTrue(producer2Called);
            Assert.IsTrue(consumer1Called);
            Assert.IsTrue(consumer2Called);
            CollectionAssert.AreEquivalent(new[] {1, 2, 3, 4, 5, 6}, items.ToArray());
        }

        [TestMethod]
        public void ProducerConsumer_SingleProducer_MultipleConsumers()
        {
            channel<int> sut = new channel<int>(2);
            bool producerCalled = false;
            Task producer = Task.Run(() =>
            {
                sut.Send(1);
                sut.Send(2);
                sut.Send(3);
                sut.Send(4);
                sut.Send(5);
                sut.Send(6);
                producerCalled = true;
            });

            ConcurrentBag<int> items = new ConcurrentBag<int>();
            bool consumer1Called = false;
            Task consumer1 = Task.Run(() =>
            {
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                consumer1Called = true;
            });

            bool consumer2Called = false;
            Task consumer2 = Task.Run(() =>
            {
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                consumer2Called = true;
            });

            Task.WaitAll(producer, consumer1, consumer2);

            Assert.AreEqual(0, sut.Length);
            Assert.IsTrue(producerCalled);
            Assert.IsTrue(consumer1Called);
            Assert.IsTrue(consumer2Called);
            CollectionAssert.AreEquivalent(new[] {1, 2, 3, 4, 5, 6}, items.ToArray());
        }

        [TestMethod]
        public void ProducerConsumer_MultipleProducers_SingleConsumer()
        {
            channel<int> sut = new channel<int>(2);
            bool producer1Called = false;
            Task producer1 = Task.Run(() =>
            {
                sut.Send(1);
                sut.Send(2);
                sut.Send(3);
                producer1Called = true;
            });

            bool producer2Called = false;
            Task producer2 = Task.Run(() =>
            {
                sut.Send(4);
                sut.Send(5);
                sut.Send(6);
                producer2Called = true;
            });

            ConcurrentBag<int> items = new ConcurrentBag<int>();
            bool consumerCalled = false;
            Task consumer = Task.Run(() =>
            {
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                items.Add(sut.Receive());
                consumerCalled = true;
            });

            Task.WaitAll(producer1, producer2, consumer);

            Assert.AreEqual(0, sut.Length);
            Assert.IsTrue(producer1Called);
            Assert.IsTrue(producer2Called);
            Assert.IsTrue(consumerCalled);
            CollectionAssert.AreEquivalent(new[] {1, 2, 3, 4, 5, 6}, items.ToArray());
        }
    }
}
