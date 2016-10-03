using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogAggregator;
using LogAggregator;

namespace RiskAdminAPI.Tests
{
    [TestClass]
    public class LogAggregatorTest
    {

        [TestMethod]
        public void TestAggregate2KeysNoFlushExpected()
        {
            //No flush expected if number of items is less than 20 as mentioned in the LogAggregator constructor
            var moqLog = new Moq.Mock<ILog>();
            ILogAggregator logger = new Aggregator(moqLog.Object, Aggregator.LogLevel.logInfo, 20);
            moqLog.SetupGet(a=>a.IsInfoEnabled).Returns(true);
            //ACT
            string key = "Key1";
            for (var i = 0; i < 3; i++)
            {
                string value = "Value_1";
                if (i < 5)
                {
                    value = "Value_2";
                }
                logger.LogData(key, value);
            }
            key = "Key2";
            for (var i = 0; i < 2; i++)
            {
                string value = "Value_3";
                logger.LogData(key, value);
            }
            //ASSERT
            moqLog.Verify(a => a.Info(null), Moq.Times.Exactly(0));
            moqLog.Verify(a => a.Debug(null), Moq.Times.Exactly(0));
            moqLog.Verify(a => a.Error(null), Moq.Times.Exactly(0));
        }

        [TestMethod]
        public void TestAggregate2Keys_WithStatsValue()
        {
            var moqLog = new Moq.Mock<ILog>();
            ILogAggregator logger = new Aggregator(moqLog.Object, Aggregator.LogLevel.logInfo, 20);
            moqLog.SetupGet(a => a.IsInfoEnabled).Returns(true);
            //ACT
            string key = "Key1";
            for (var i = 0; i < 10; i++)
            {
                string value = "Value_1";
                if (i < 5)
                {
                    value = "Value_2";
                }
                logger.LogData(key, value,i);
            }
            key = "Key2";
            for (var i = 0; i < 10; i++)
            {
                string value = "Value_3";
                if (i < 5)
                {
                    value = "Value_4";
                }
                logger.LogData(key, value,i*2);
            }
            //ASSERT
            moqLog.Verify(a => a.Info("\r\nKey1 --------------------------------------\r\nValue_1:\t 5 occurrences (50%) [ Value Stats: total 5, avg. 7, max 9, min 5, first 5, last 9 ]\r\nValue_2:\t 5 occurrences (50%) [ Value Stats: total 5, avg. 2, max 4, min 0, first 0, last 4 ]\r\n\r\nKey2 --------------------------------------\r\nValue_3:\t 5 occurrences (50%) [ Value Stats: total 5, avg. 14, max 18, min 10, first 10, last 18 ]\r\nValue_4:\t 5 occurrences (50%) [ Value Stats: total 5, avg. 4, max 8, min 0, first 0, last 8 ]\r\n"), Moq.Times.Exactly(1));
            moqLog.Verify(a => a.Debug(null), Moq.Times.Exactly(0));
            moqLog.Verify(a => a.Error(null), Moq.Times.Exactly(0));
        }


        [TestMethod]
        public void TestAggregate2Keys()
        {
            var moqLog = new Moq.Mock<ILog>();
            ILogAggregator logger = new Aggregator(moqLog.Object, Aggregator.LogLevel.logInfo, 20);
            moqLog.SetupGet(a => a.IsInfoEnabled).Returns(true);
            //ACT
            string key = "Key1";
            for( var i = 0; i < 10; i++)
            {
                string value = "Value_1";
                if (i < 5)
                {
                    value = "Value_2";
                }
                logger.LogData(key, value);
            }
            key = "Key2";
            for (var i = 0; i < 10; i++)
            {
                string value = "Value_3";
                if (i < 5)
                {
                    value = "Value_4";
                }
                logger.LogData(key, value);
            }
            //ASSERT
            moqLog.Verify(a => a.Info("\r\nKey1 --------------------------------------\r\nValue_1:\t 5 occurrences (50%)\r\nValue_2:\t 5 occurrences (50%)\r\n\r\nKey2 --------------------------------------\r\nValue_3:\t 5 occurrences (50%)\r\nValue_4:\t 5 occurrences (50%)\r\n"), Moq.Times.Exactly(1));
            moqLog.Verify(a => a.Debug(null), Moq.Times.Exactly(0));
            moqLog.Verify(a => a.Error(null), Moq.Times.Exactly(0));
        }

        public void TestAggregate2KeysWithThreads_LogLevelNotEnabled()
        {
            var moqLog = new Moq.Mock<ILog>();
            ILogAggregator logger = new Aggregator(moqLog.Object, Aggregator.LogLevel.logInfo, 20);
            moqLog.SetupGet(a => a.IsInfoEnabled).Returns(true);
            List<Task> tasks = new List<Task>();
            //ACT
            tasks.Add(Task.Run(() =>
            {
                string key = "Key1";
                for (var i = 0; i < 10; i++)
                {
                    string value = "Value_1";
                    if (i < 5)
                    {
                        value = "Value_2";
                    }
                    logger.LogData(key, value);
                }
            }));
            tasks.Add(Task.Run(() =>
            {
                string key = "Key2";
                for (var i = 0; i < 10; i++)
                {
                    string value = "Value_3";
                    if (i < 5)
                    {
                        value = "Value_4";
                    }
                    logger.LogData(key, value);
                }
            }));
            Task.WaitAll(tasks.ToArray());
            //ASSERT
            moqLog.Verify(a => a.Info(null), Moq.Times.Exactly(0));
            moqLog.Verify(a => a.Debug(null), Moq.Times.Exactly(0));
            moqLog.Verify(a => a.Error(null), Moq.Times.Exactly(0));

        }


        [TestMethod]
        public void TestAggregate2KeysWithThreads()
        {
            var moqLog = new Moq.Mock<ILog>();
            ILogAggregator logger = new Aggregator(moqLog.Object, Aggregator.LogLevel.logInfo, 20);
            moqLog.SetupGet(a => a.IsInfoEnabled).Returns(true);
            List<Task> tasks = new List<Task>();
            //ACT
            tasks.Add( Task.Run(() =>
           {
               string key = "Key1";
               for (var i = 0; i < 10; i++)
               {
                   string value = "Value_1";
                   if (i < 5)
                   {
                       value = "Value_2";
                   }
                   logger.LogData(key, value);
               }
           }));
            tasks.Add(Task.Run(() =>
            {
                string key = "Key2";
                for (var i = 0; i < 10; i++)
                {
                    string value = "Value_3";
                    if (i < 5)
                    {
                        value = "Value_4";
                    }
                    logger.LogData(key, value);
                }
            }));
            Task.WaitAll(tasks.ToArray());
            //ASSERT
            moqLog.Verify(a => a.Info("\r\nKey1 --------------------------------------\r\nValue_1:\t 5 occurrences (50%)\r\nValue_2:\t 5 occurrences (50%)\r\n\r\nKey2 --------------------------------------\r\nValue_3:\t 5 occurrences (50%)\r\nValue_4:\t 5 occurrences (50%)\r\n"), Moq.Times.Exactly(1));
            moqLog.Verify(a => a.Debug(null), Moq.Times.Exactly(0));
            moqLog.Verify(a => a.Error(null), Moq.Times.Exactly(0));

        }
    }
}
