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
            switch (configManager.trainInputFormat)
            {
                case "ReadyFeatures":
                    trainParser = new ReadyFeaturesParser(configManager, logger);
                    break;
                case "RawTxt":
                    trainParser = new RawTxtParser(configManager, logger);
                    break;
                default:
                    trainParser = new ReadyFeaturesParser(configManager, logger);
                    break;
            }          

            // Start Train Set parsing from root directory
            trainParser.Parse(configManager.rootTrainDirectory, "Train", configManager.trainInputParsingMode, configManager.trainInputFormat);


            // Start the test parser
            Parser testParser;
            switch (configManager.testInputFormat)
            {
                case "ReadyFeatures":
                    testParser = new ReadyFeaturesParser(configManager, logger);
                    break;
                case "RawTxt":
                    testParser = new RawTxtParser(configManager, logger);
                    break;
                default:
                    testParser = new ReadyFeaturesParser(configManager, logger);
                    break;
            }

            // Start Test Set parsing from root directory
            testParser.Parse(configManager.rootTestDirectory, "Test", configManager.testInputParsingMode, configManager.testInputFormat);

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
