using log4net;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAggregator
{
    public class Aggregator : ILogAggregator
    {
        private SortedDictionary<string, SortedDictionary<string, ValueInfo>> _dataToLog = new SortedDictionary<string, SortedDictionary<string, ValueInfo>>();
        private readonly ILog _logger;
        private readonly object _locker = new object();
        private readonly LogLevel _logLevel;
        private int _itemsCount = 0;
        private int _autoFlushAmount = 0;

        //internal class used to store counted value information along with statistics about the stats value
        private class ValueInfo
        {
            public double Count { get; set; }
            public long TotalStatsValues { get; set; }
            public double? MaxStatsValue { get; set; }
            public double? AverageStatsValue { get; set; }
            public double? MinStatsValue { get; set; }
            public double? FirstStatsValue { get; set; }
            public double?LastStatsValue { get; set; }
        }

        public enum LogLevel:int
        {
            logDebug,
            logInfo,
            lofWarn,
            logError
        }

        /// <summary>
        /// Constructtor
        /// </summary>
        /// <param name="logger">log4net log interface</param>
        /// <param name="logLevel">log4net log level on which to log the collected data</param>
        /// <param name="autoFlushAmount">treshhold value after reaching which data is flushed to the log file using log4net</param>
        public Aggregator(ILog logger, LogLevel logLevel,int autoFlushAmount)
        {
            _logger = logger;
            _logLevel = logLevel;
            _autoFlushAmount = autoFlushAmount;
            if (_autoFlushAmount == 0)
                _autoFlushAmount = 1;
        }

        public void LogData(string key, string valueForOccurencesCount,double? valueForStats=null)
        {
            if((!_logger.IsInfoEnabled && _logLevel == LogLevel.logInfo) ||
                (!_logger.IsErrorEnabled && _logLevel == LogLevel.logError) ||
                (!_logger.IsDebugEnabled && _logLevel == LogLevel.logDebug) ||
                (!_logger.IsWarnEnabled && _logLevel == LogLevel.lofWarn)
                )
            {
                //the level is not enabled no reason to collect/aggregate info
                return;
            }


            lock (_locker)
            {
                SortedDictionary<string, ValueInfo> dictionaryOfValues;
                if (_dataToLog.ContainsKey(key))
                {
                    //key exists - grab its  value- count  dictionary
                    dictionaryOfValues = _dataToLog[key];
                }
                else
                {
                    //key does not exist - create value- count dictionary and add by the key
                    dictionaryOfValues = new SortedDictionary<string, ValueInfo>();
                    _dataToLog.Add(key,  dictionaryOfValues);
                }
                
               
                if (dictionaryOfValues.ContainsKey(valueForOccurencesCount))
                {
                    //value already present
                    var logEntryInfo = dictionaryOfValues[valueForOccurencesCount];
                    logEntryInfo.Count++;
                    if (valueForStats != null)
                    {
                        if (!logEntryInfo.MaxStatsValue.HasValue || logEntryInfo.MaxStatsValue.Value< valueForStats.Value)
                        {
                            logEntryInfo.MaxStatsValue = valueForStats;
                        }
                        if (!logEntryInfo.MinStatsValue.HasValue || logEntryInfo.MinStatsValue.Value > valueForStats.Value)
                        {
                            logEntryInfo.MinStatsValue = valueForStats;
                        }
                        if (logEntryInfo.TotalStatsValues > 0)
                        {
                            logEntryInfo.AverageStatsValue = ((logEntryInfo.AverageStatsValue.Value * logEntryInfo.TotalStatsValues) + valueForStats.Value) / (logEntryInfo.TotalStatsValues + 1);
                        }
                        if (!logEntryInfo.FirstStatsValue.HasValue)
                        {
                            logEntryInfo.FirstStatsValue = valueForStats;
                        }
                        logEntryInfo.LastStatsValue = valueForStats;
                        logEntryInfo.TotalStatsValues++;
                    }
                }
                else
                {
                    //first item
                    var newValueInfo = new ValueInfo
                    {
                        Count = 1,
                        MaxStatsValue = valueForStats,
                        MinStatsValue = valueForStats,
                        AverageStatsValue = valueForStats,
                        FirstStatsValue = valueForStats,
                        LastStatsValue = valueForStats
                    }; 
                    if (valueForStats.HasValue)
                    {
                        newValueInfo.TotalStatsValues++;
                    }
                    //not in the value list - add it with count 1
                    dictionaryOfValues.Add(valueForOccurencesCount, newValueInfo);
                }
                _itemsCount++;
                //we reached the ammount that requires flush
                if (_itemsCount== _autoFlushAmount)
                {
                    
                    this.Flush();
                }
            }
        }

        public void Flush()
        {
            StringBuilder sw = new StringBuilder();
            lock (_locker)
            {
                _itemsCount = 0;
               
                foreach( var keyValue in _dataToLog)
                {
                    sw.Append(Environment.NewLine+keyValue.Key + " --------------------------------------" + Environment.NewLine);

                    var total = keyValue.Value.Sum(a => a.Value.Count);

                    foreach (var keyValueInfo in keyValue.Value)
                    {
                        var percent= (keyValueInfo.Value.Count / total) * 100;
                        sw.Append(keyValueInfo.Key + ":\t " + keyValueInfo.Value.Count + " occurrences (" + Math.Round(percent,2) + "%)" );
                        if (keyValueInfo.Value.TotalStatsValues > 0)
                        {
                            //we need to provide statistics about this value for stats
                            sw.Append(" [ Value Stats: total " + keyValueInfo.Value.TotalStatsValues + ", avg. " + keyValueInfo.Value.AverageStatsValue.Value + ", max " + keyValueInfo.Value.MaxStatsValue.Value + ", min " + keyValueInfo.Value.MinStatsValue.Value + ", first " + keyValueInfo.Value.FirstStatsValue.Value + ", last " + keyValueInfo.Value.LastStatsValue.Value + " ]");
                        }
                        sw.Append(Environment.NewLine);
                    }
                }
                _dataToLog.Clear();

            }
            var stringToLog = sw.ToString();
            //System.IO.File.WriteAllText(@"\WriteFileWithValue.txt", stringToLog);
            
            if (_logLevel == LogLevel.logDebug)
                _logger.Debug(stringToLog);
            else if (_logLevel==LogLevel.logError)
                _logger.Error(stringToLog);
            else
                _logger.Info(stringToLog);
        }
    }
}
