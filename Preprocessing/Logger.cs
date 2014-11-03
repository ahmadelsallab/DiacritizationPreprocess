using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Preprocessing
{


    class Logger
    {
        // Array of error logs counters
        private int[] errLogs;

        // Array that contains the configuration of errors suppression
        private int[] errSupressArr;

        // Logger file name. Only in debug mode.
        private StreamWriter logFile;

        // Debug mode flag
        private bool debugMode;

        // Flag to indicate if error logging is enabled or not
        private bool errorLogMode;

        // Flag to indicate if logger shall log trace msgs to console or not
        private bool consoleLogMode;

        // Total number of errors
        private int totalNumErrors;

        // Constants
        public const int SUPRESS_ERR_CODE = -1;
        public const int NOT_SUPRESS_ERR_CODE = 1;

        

        // Constructor
        public Logger(ConfigurationManager configManager)
        {
            this.logFile = configManager.logFile;
            this.debugMode = configManager.debugMode;
            this.errorLogMode = configManager.errorLogMode;
            this.consoleLogMode = configManager.consoleLogMode;
            this.errSupressArr = configManager.errSupressArr;
            this.totalNumErrors = 0;

            errLogs = new int[Enum.GetValues(typeof(ErrorCode)).Length];
            for(int i = 0; i < errLogs.Length; i++)
            {
                errLogs[i] = 0;
            }
        }// end constructor

        public void LogTrace(String msg)
        {
            if (debugMode)
            {
                logFile.WriteLine(msg);
            } // end if

            if (consoleLogMode)
            {
                Console.WriteLine(msg);
            }// end if
        } // end LogTrace

        public void LogError(String msg, ErrorCode Err)
        {
            // Increment errors
            totalNumErrors++;

            // Log error according to type
            errLogs[(int)Err - 1]++;

            if ((errorLogMode == true) && (errSupressArr[(int)Err - 1] != SUPRESS_ERR_CODE))
            {
                logFile.WriteLine("Error (" + ((int)Err).ToString() +"): " + msg);
            } // end if
        }// end LogError

        public void LogInfo()
        {
            logFile.WriteLine("Total number of errors: " + totalNumErrors);

            for(int i = 0; i < errLogs.Length; i++)
            {               
                logFile.WriteLine("Count of Errors Type (" + (ErrorCode)(i + 1) + ") is " + errLogs[i]);
            }

            if (consoleLogMode)
            {
                Console.WriteLine("Total number of errors: " + totalNumErrors);
                for (int i = 0; i < errLogs.Length; i++)
                {

                    Console.WriteLine("Count of Errors Type (" + (ErrorCode)(i + 1) + ") with Error Code " + (i + 1) + " is " + errLogs[i]);
                }

            } // end if
        } // end LogInfo
    }
}
