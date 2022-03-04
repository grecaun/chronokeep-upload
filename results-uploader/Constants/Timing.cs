using System;
using System.Collections.Generic;

namespace results_uploader.Constants
{
    public class Timing
    {
        // These values are what are sent in the API Result and indicate the type of the result.
        public const int RESULT_TYPE_NORMAL = 0;
        public const int RESULT_TYPE_EARLY = 11;
        public const int RESULT_TYPE_UNOFFICIAL = 12;
        public const int RESULT_TYPE_VIRTUAL = 13;
        public const int RESULT_TYPE_DNF = 23;
        public const int RESULT_TYPE_DNS = 24;

        // API Upload Count
        public const int API_LOOP_COUNT = 20;

        public static string ToTime(long seconds, int milliseconds)
        {
            return string.Format("{0:D}:{1:D2}:{2:D2}.{3:D3}", seconds / 3600, seconds % 3600 / 60, seconds % 60, milliseconds);
        }
    }
}