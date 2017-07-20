/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  Histogram, ReadNewLog, SampleObjectTable, LiveObjectTable, etc.
 *
 * Description: Log file parser and various graph generators
 */


namespace CLRProfiler
{
    internal sealed partial class ReadNewLog
    {
        // helper class to keep track of events like log file comments, garbage collections, and heap dumps
        internal sealed class EventList
        {
            internal int count;
            internal int[] eventTickIndex;
            internal string[] eventString;

            internal EventList()
            {
                eventTickIndex = new int[10];
                eventString = new string[10];
            }

            internal bool AddEvent(int newTickIndex, string newString)
            {
                if (count > 0 && newTickIndex <= eventTickIndex[count-1])
                {
                    return false;
                }

                EnsureIntCapacity(count, ref eventTickIndex);
                EnsureStringCapacity(count, ref eventString);
                eventTickIndex[count] = newTickIndex;
                eventString[count] = newString;
                count++;

                return true;
            }
        }
    }
}
