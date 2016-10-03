LogAggregator is a small .NET library that allows you to aggregate your repeating log outputs (those you usually put in debug level) and generates useful statistics about the data. The library works with log4net although can be easily ported to work with any other provides such as NLog. It does not open any threads but it does use lock object.

Typical usage scenario:

Scenario #1

You have a custom Cache mechanism that you implemented in your MVC .NET web site. You want to know how efficient is your cache and the best metric for this is Hit/Miss rate. Ways to resolve it :

One option is to have Debug level print to file that you can look and see how many times it was hit or missed. Number of drawbacks: in production environment it would not be recommended to put low Log treshhold as it might affect performance and problem number two is that highly like that number of log entries would be high and it basically make them unusable. Log Aggregator offers you a very simple solution. Each time you want to log either Hit or Miss call LogAggregator _LogData _ method and it will record and keep track of number of occurrences of the value i.e 'Cache Hit' and at the time it reaches threshold it will write to log4net interface you passed a summary of the Cache hits/misses entries passed. Example report given (threshold of 1000 entries) would be:

Cache ------------------------------------

Cache Hit 400 (40%)

Cache Miss 600 (60%)

Scenario #2

You want to know how long it take to execute some frequently called queries in your application as you made some SQL code optimization and want to produce a benchmark of your performance. You could write to log file the duration it took to execute a query. But in production or even load test environment you would not be allowed to use this log level. And once again the output would require some post processing to see if on average there is significant speed improvement. Using LogAggregator you can call LogData method pass key, value and numeric value of the time it took to execute the query and once the log entry threshold is reached you would get a report. In the following sample report we assume that there were 40 query calls - 20 out of which to Query1 and 20 Query 2. And the time it took to execute first Query1 call was 22 milliseconds, last 17 and on average it took 21 milliseconds per this query.

Example report in such case:

QueryPerformance -------------------------------------

Query1 20 50%, [Stats: 20 calls, first 22, last 17, average 21 ,max 22, min 14]

Query2 20 50%, [Stats: 20 calls, first 202, last 133, average 199 ,max 202, min 122]

PLEASE NOTE

There might be a case where last log entries will be lost in case your application reaches its end before LogAggregator reached its flush threshold . If it is important to make sure that the last batch is not missed LogAggregator has a _Flush _method that can be called to make sure all the contained data is persisted to the log target file.
