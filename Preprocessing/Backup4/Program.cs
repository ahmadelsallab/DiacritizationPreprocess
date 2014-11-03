using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;


namespace Preprocessing
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start the Configuration Manager
            //ConfigurationManager configManager = new ConfigurationManager(@"D:\Work\Research\PhD\Implementation\Diactrization\Preprocessing\Preprocessing\Configurations.xml");
            ConfigurationManager configManager = new ConfigurationManager(args[0]);

            // Start the logger
            Logger logger = new Logger(configManager);

            // Start the train parser
            Parser trainParser;
            switch (configManager.trainPOSInputFormat)
            {
                case "RDI":
                    trainParser = new RDIFormatParser(configManager, logger);
                    break;
                case "Stanford":
                    trainParser = new StanfordFormatParser(configManager, logger);
                    break;
                default:
                    trainParser = new RDIFormatParser(configManager, logger);
                    break;
            }          

            // Start Train Set parsing from root directory
            trainParser.Parse(configManager.rootTrainDirectory, "Train", configManager.trainPOSInputFormat);


            // Start the test parser
            Parser testParser;
            switch (configManager.testPOSInputFormat)
            {
                case "RDI":
                    testParser = new RDIFormatParser(configManager, logger);
                    break;
                case "Stanford":
                    testParser = new StanfordFormatParser(configManager, logger);
                    break;
                default:
                    testParser = new RDIFormatParser(configManager, logger);
                    break;
            }

            // Start Train Set parsing from root directory
            testParser.Parse(configManager.rootTestDirectory, "Test", configManager.testPOSInputFormat);

            // Copy files to configuration environment if required
            if (configManager.configEnvDirectory != "")
            {
                MLApp.MLAppClass matlab = new MLApp.MLAppClass();
                matlab.Execute(@"load('" + configManager.rootTrainDirectory + @"\input_data');");
                matlab.Execute(@"load('" + configManager.rootTestDirectory + @"\input_data');");
                matlab.Execute(@"save('" + configManager.configEnvDirectory + @"\input_data');");
            }
        }

    }
}
