using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAggregator
{
    public interface ILogAggregator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">key by which all the valueForOccurencesCount are counted by i.e 'MyCache'</param>
        /// <param name="valueForOccurencesCount">value that its number of occurences is being counted i.e 'Miss' or 'Hit'</param>
        /// <param name="valueForStats">optional value that will have its statistics (min, max, first,last,average) collected per key-valueForOccurencesCount combination i,e 3000 time it rook to execute cache miss scenario </param>
        void LogData(string key, string valueForOccurencesCount, double? valueForStats=null);

        /// <summary>
        /// Flush all the data accumulated and aggregated so far to the log
        /// </summary>
        void Flush();
    }
}
