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

            // Start the parser
            Parser parser = new Parser(configManager, logger);

            // Start Train Set parsing from root directory
            parser.Parse(configManager.rootTrainDirectory, "Train");

            // Start Test Set parsing from root directory
            parser.Parse(configManager.rootTestDirectory, "Test");

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
