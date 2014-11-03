using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
//using MathWorks.MATLAB.NET.Arrays;
//using MathWorks.MATLAB.NET.Utility;
//using MLApp;

namespace Preprocessing
{
    abstract class Parser
    {
        // Identifier name of the POS tagger if this instance is used as POS Tagger. For debugging purposes
        public String posTaggerName;

        // Flag to determine if this run is for max ID determination or not 
        // Set this flag to true so that parsing doesn't include writting to output file or formatting, just parsing
        public bool maxIDRun;

        // Array of items structures
        public Item[] items;

        // Instance of the pre-processing configuration manager
        protected ConfigurationManager configManager;

        // Logger instance
        protected Logger logger;

        // Maximum IDs
        public Item maxIDs;

        public String[] posNames;

        // Hashmap to indicate a word previously added or not
        private Hashtable wordsHashTable = new Hashtable();

        // The outout file writer instance. It'll be inisantiated with the begining of parse
        OutputFileWriter outFileWriter;

        //StreamWriter wordsLogFile;

        // Reference to Tagger Parser
        protected Parser[] posTaggers;

        // Reference to the classifier
        protected Parser classifier;

        private int totalNumWords;
        private int misClassifiedWords;

        // Constants
        protected const int LEAST_ID_VAL = -100;


        // Hash table to link the ASCII code of each diac class to its target code bitfieldValue
        protected Hashtable diacCodeToAscii = new Hashtable();

        // Hash table to link the target code bitfieldValue to the ASCII code of each diac class
        protected Hashtable diacAsciiToCode = new Hashtable();

        // Constructor
        public Parser(ConfigurationManager configManager, Logger logger)
        {
            int[] POS_leastID = { LEAST_ID_VAL, LEAST_ID_VAL };
            maxIDs = new Item("", LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, POS_leastID, LEAST_ID_VAL, -1);

            this.configManager = configManager;
            this.logger = logger;

            totalNumWords = 0;
            misClassifiedWords = 0;

            TargetDiacAscii[] targetAscii = (TargetDiacAscii[])Enum.GetValues(typeof(TargetDiacAscii));
            TargetDiacCode[] targetDiacCodes = (TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode));
            // -1 to remove the DEFAULT case which is not actual diac. class to be added to hash table
            for (int i = 0; i < targetAscii.Length - 1; i++)
            {
                diacCodeToAscii.Add(targetDiacCodes[i], targetAscii[i]);
                diacAsciiToCode.Add((int)targetAscii[i], targetDiacCodes[i]);
            }

            // Initialize the classifier
            
            // Initialize the configMgr of the classifier
            if(configManager.clsParams != null)
            {
                ConfigurationManager clsConfigMgr = new ConfigurationManager(configManager.clsParams.configurationFileFullPath);

                switch (clsConfigMgr.trainInputFormat)
                {
                    case "ReadyFeatures":
                        classifier = new ReadyFeaturesParser(clsConfigMgr, logger, configManager.clsParams.maxIDInfoFileFullPath);
                        break;
                    case "RawTxt":
                        classifier = new RawTxtParser(clsConfigMgr, logger, configManager.clsParams.maxIDInfoFileFullPath);
                        break;
                    default:
                        classifier = new RawTxtParser(clsConfigMgr, logger, configManager.clsParams.maxIDInfoFileFullPath);
                        break;
                }// end switch
                
            }// end if
            // Initialize the POS taggers
            if (configManager.numPOSTaggers > 0)
            {
                posTaggers = new Parser[configManager.numPOSTaggers];

                for (int i = 0; i < configManager.posTaggersParams.Length; i++)
                {
                    switch (configManager.posTaggersParams[i].posTaggerType)
                    {
                        case "Stanford":
                            break;

                        case "DNN":
                            ConfigurationManager posTaggerConfigMgr = new ConfigurationManager(configManager.posTaggersParams[i].DNN_POSTaggerConfigurationFileFullPath);

                            switch (posTaggerConfigMgr.trainInputFormat)
                            {
                                case "ReadyFeatures":
                                    posTaggers[i] = new ReadyFeaturesParser(posTaggerConfigMgr, logger, configManager.posTaggersParams[i].DNN_POSTaggerMaxIDInfoFileFullPath);
                                    break;
                                case "RawTxt":
                                    posTaggers[i] = new RawTxtParser(posTaggerConfigMgr, logger, configManager.posTaggersParams[i].DNN_POSTaggerMaxIDInfoFileFullPath);
                                    break;
                                default:
                                    posTaggers[i] = new RawTxtParser(posTaggerConfigMgr, logger, configManager.posTaggersParams[i].DNN_POSTaggerMaxIDInfoFileFullPath);
                                    break;
                            }
                            posTaggers[i].posTaggerName = configManager.posTaggersParams[i].posTaggerName;

                            break;
                        default:
                            Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: Stanford or DNN.", configManager.posTaggersParams[i].posTaggerType);
                            break;
                    }// end switch
                }
            }// end if
            String maxIDFullFileName;
            if ((configManager.rootTrainDirectory != "") && (configManager.rootTrainDirectory != null))
            {
                maxIDFullFileName = configManager.rootTrainDirectory + configManager.directorySeparator + "maxIDInfo.txt";
            }
            else
            {
                maxIDFullFileName = configManager.rootTestDirectory + configManager.directorySeparator + "maxIDInfo.txt";
            }
            //wordsLogFile = new StreamWriter(File.Open(@"E:\Documents and Settings\ASALLAB\Desktop\Flash\PhD_Flash\Implementation\Diactrization\Preprocessing\items.txt", FileMode.OpenOrCreate, FileAccess.Write), Encoding.GetEncoding(1256));
            //wordsLogFile.AutoFlush = true;
            // public static int DateTime.Compare(DateTime t1,DateTime t2)
            // Less than zero : t1 is earlier than t2. 
            // Zero : 	t1 is the same as t2. 
            // Greater than zero : t1 is later than t2. 
            // The condition means: the mxIDinfo file exists and configuration is not updated; so configFile write time is earlier that maxIDInfo write time
            StreamReader maxIDInfoFile;
            StreamWriter maxIDInfoFileWrite;
            switch(configManager.targetType)
            {
                case "POS":
                case "SYNT_DIAC":
                    if (File.Exists(maxIDFullFileName) && (DateTime.Compare(File.GetLastWriteTimeUtc(configManager.configFullFileName),
                                                        File.GetLastWriteTimeUtc(maxIDFullFileName)) < 0))
                    {
                        // Just read the maxID's from the existing file
                        maxIDInfoFile = new StreamReader(File.Open(maxIDFullFileName, FileMode.Open, FileAccess.ReadWrite));

                        try // The file could exist but empty
                        {
                            maxIDRun = false;

                            maxIDs.mrfType = Int32.Parse(maxIDInfoFile.ReadLine());
                            maxIDs.p = Int32.Parse(maxIDInfoFile.ReadLine());
                            maxIDs.r = Int32.Parse(maxIDInfoFile.ReadLine());
                            maxIDs.f = Int32.Parse(maxIDInfoFile.ReadLine());
                            maxIDs.s = Int32.Parse(maxIDInfoFile.ReadLine());
                            maxIDs.POS_IDs[0] = Int32.Parse(maxIDInfoFile.ReadLine());
                            maxIDs.POS_IDs[1] = Int32.Parse(maxIDInfoFile.ReadLine());
                            maxIDs.equivalentPOS_ID = double.Parse(maxIDInfoFile.ReadLine());
                            maxIDs.vocabularyWordID = Int32.Parse(maxIDInfoFile.ReadLine());
                            maxIDs.wordLength = Int32.Parse(maxIDInfoFile.ReadLine());
                            maxIDs.itemName = maxIDInfoFile.ReadLine();
                            maxIDInfoFile.Close();
                        }
                        catch // The file is empty--> re-run
                        {
                            maxIDInfoFile.Close();

                            // Set this flag to true so that parsing doesn't include writting to output file or formatting, just parsing
                            maxIDRun = true;

                            // Change configuration to be All then come back again
                            String temp = configManager.outputFeatures;
                            //configManager.outputFeatures = "All";
                            // Parse train files
                            if ((configManager.rootTrainDirectory != "") && (configManager.rootTrainDirectory != null))
                            {
                                Parse(configManager.rootTrainDirectory, "Train", configManager.trainInputParsingMode, configManager.trainInputFormat);
                            }
                            // Parse test files
                            if ((configManager.rootTestDirectory != "") && (configManager.rootTestDirectory != null))
                            {
                                Parse(configManager.rootTestDirectory, "Test", configManager.testInputParsingMode, configManager.testInputFormat);
                            }

                            configManager.outputFeatures = temp;

                            //The last place in the bit-field is reserved to the not seen items
                            // But we don't add +1, since the wordsHashTable.Count already +1 from the max vocabularyWordID
                            // Ex: if the vocabularyWordID ranges from 0..49 then wordsHashTable.Count is 50
                            // So position wordsHashTable.Count = 50 is reserved to unseen word
                            maxIDs.vocabularyWordID = wordsHashTable.Count;

                            // You need to make a one time parse to get the maxID's needed to represent features
                            // Note the files access this time: FileMode.OpenOrCreate not CreateNew
                            maxIDInfoFileWrite = new StreamWriter(File.Open(maxIDFullFileName, FileMode.OpenOrCreate, FileAccess.Write));
                            maxIDInfoFileWrite.AutoFlush = true;

                            maxIDInfoFileWrite.WriteLine(maxIDs.mrfType);
                            maxIDInfoFileWrite.WriteLine(maxIDs.p);
                            maxIDInfoFileWrite.WriteLine(maxIDs.r);
                            maxIDInfoFileWrite.WriteLine(maxIDs.f);
                            maxIDInfoFileWrite.WriteLine(maxIDs.s);
                            maxIDInfoFileWrite.WriteLine(maxIDs.POS_IDs[0]);
                            maxIDInfoFileWrite.WriteLine(maxIDs.POS_IDs[1]);
                            maxIDInfoFileWrite.WriteLine(maxIDs.equivalentPOS_ID);
                            maxIDInfoFileWrite.WriteLine(maxIDs.vocabularyWordID);
                            maxIDInfoFileWrite.WriteLine(maxIDs.wordLength);
                            maxIDInfoFileWrite.WriteLine(maxIDs.itemName);
                            maxIDInfoFileWrite.Close();
                            maxIDRun = false;

                        }

                    }
                    else
                    {
                        maxIDRun = true;

                        // Change configuration to be All then come back again
                        String temp = configManager.outputFeatures;
                        //configManager.outputFeatures = "All";
                        if ((configManager.rootTrainDirectory != "") && (configManager.rootTrainDirectory != null))
                        {
                            // Parse train files
                            Parse(configManager.rootTrainDirectory, "Train", configManager.trainInputParsingMode, configManager.trainInputFormat);
                        }
                        if ((configManager.rootTestDirectory != "") && (configManager.rootTestDirectory != null))
                        {
                            // Parse test files
                            Parse(configManager.rootTestDirectory, "Test", configManager.testInputParsingMode, configManager.testInputFormat);
                        }
                        configManager.outputFeatures = temp;

                        // Update the maxIDs with the maximum number of un-repeated (unique) items
                        //maxIDs.vocabularyWordID = wordsHashTable.Count;

                        //The last place in the bit-field is reserved to the not seen items
                        // But we don't add +1, since the wordsHashTable.Count already +1 from the max vocabularyWordID
                        // Ex: if the vocabularyWordID ranges from 0..49 then wordsHashTable.Count is 50
                        // So position wordsHashTable.Count = 50 is reserved to unseen word
                        maxIDs.vocabularyWordID = wordsHashTable.Count;

                        // You need to make a one time parse to get the maxID's needed to represent features
                        // Note the files access this time: FileMode.OpenOrCreate not CreateNew, bcz the file could exist but outdated
                        maxIDInfoFileWrite = new StreamWriter(File.Open(maxIDFullFileName, FileMode.OpenOrCreate, FileAccess.Write));
                        maxIDInfoFileWrite.AutoFlush = true;

                        maxIDInfoFileWrite.WriteLine(maxIDs.mrfType);
                        maxIDInfoFileWrite.WriteLine(maxIDs.p);
                        maxIDInfoFileWrite.WriteLine(maxIDs.r);
                        maxIDInfoFileWrite.WriteLine(maxIDs.f);
                        maxIDInfoFileWrite.WriteLine(maxIDs.s);
                        maxIDInfoFileWrite.WriteLine(maxIDs.POS_IDs[0]);
                        maxIDInfoFileWrite.WriteLine(maxIDs.POS_IDs[1]);
                        maxIDInfoFileWrite.WriteLine(maxIDs.equivalentPOS_ID);
                        maxIDInfoFileWrite.WriteLine(maxIDs.vocabularyWordID);
                        maxIDInfoFileWrite.WriteLine(maxIDs.wordLength);
                        maxIDInfoFileWrite.WriteLine(maxIDs.itemName);
                        maxIDInfoFileWrite.Close();
                        maxIDRun = false;
                    }

                    break;
                case "ClassifySyntDiac":
                    // Just read the maxID's from the existing file
                    maxIDInfoFile = new StreamReader(File.Open(maxIDFullFileName, FileMode.Open, FileAccess.ReadWrite));

                    try // The file could exist but empty
                    {
                        maxIDRun = false;

                        maxIDs.mrfType = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.p = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.r = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.f = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.s = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.POS_IDs[0] = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.POS_IDs[1] = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.equivalentPOS_ID = double.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.vocabularyWordID = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.wordLength = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.itemName = maxIDInfoFile.ReadLine();
                        maxIDInfoFile.Close();
                    }
                    catch // The file is empty--> re-run
                    {
                        maxIDInfoFile.Close();

                        // Set this flag to true so that parsing doesn't include writting to output file or formatting, just parsing
                        maxIDRun = true;

                        // Change configuration to be All then come back again
                        String temp = configManager.outputFeatures;
                        //configManager.outputFeatures = "All";
                        if ((configManager.rootTrainDirectory != "") && (configManager.rootTrainDirectory != null))
                        {
                            // Parse train files
                            Parse(configManager.rootTrainDirectory, "Train", configManager.trainInputParsingMode, configManager.trainInputFormat);
                        }
                        if ((configManager.rootTestDirectory != "") && (configManager.rootTestDirectory != null))
                        {
                            // Parse test files
                            Parse(configManager.rootTestDirectory, "Test", configManager.testInputParsingMode, configManager.testInputFormat);
                        }

                        configManager.outputFeatures = temp;

                        //The last place in the bit-field is reserved to the not seen items
                        // But we don't add +1, since the wordsHashTable.Count already +1 from the max vocabularyWordID
                        // Ex: if the vocabularyWordID ranges from 0..49 then wordsHashTable.Count is 50
                        // So position wordsHashTable.Count = 50 is reserved to unseen word
                        maxIDs.vocabularyWordID = wordsHashTable.Count;

                        // You need to make a one time parse to get the maxID's needed to represent features
                        // Note the files access this time: FileMode.OpenOrCreate not CreateNew
                        maxIDInfoFileWrite = new StreamWriter(File.Open(maxIDFullFileName, FileMode.OpenOrCreate, FileAccess.Write));
                        maxIDInfoFileWrite.AutoFlush = true;

                        maxIDInfoFileWrite.WriteLine(maxIDs.mrfType);
                        maxIDInfoFileWrite.WriteLine(maxIDs.p);
                        maxIDInfoFileWrite.WriteLine(maxIDs.r);
                        maxIDInfoFileWrite.WriteLine(maxIDs.f);
                        maxIDInfoFileWrite.WriteLine(maxIDs.s);
                        maxIDInfoFileWrite.WriteLine(maxIDs.POS_IDs[0]);
                        maxIDInfoFileWrite.WriteLine(maxIDs.POS_IDs[1]);
                        maxIDInfoFileWrite.WriteLine(maxIDs.equivalentPOS_ID);
                        maxIDInfoFileWrite.WriteLine(maxIDs.vocabularyWordID);
                        maxIDInfoFileWrite.WriteLine(maxIDs.wordLength);
                        maxIDInfoFileWrite.WriteLine(maxIDs.itemName);
                        maxIDInfoFileWrite.Close();
                        maxIDRun = false;

                    }
                    break;
                case "FULL_DIAC":
                    // Just read the maxID's from the existing file
                    maxIDInfoFile = new StreamReader(File.Open(maxIDFullFileName, FileMode.Open, FileAccess.ReadWrite));

                    try // The file could exist but empty
                    {
                        maxIDRun = false;

                        maxIDs.mrfType = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.p = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.r = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.f = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.s = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.POS_IDs[0] = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.POS_IDs[1] = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.equivalentPOS_ID = double.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.vocabularyWordID = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.wordLength = Int32.Parse(maxIDInfoFile.ReadLine());
                        maxIDs.wordLength = 1;// For character diac: the word length is 1 char.
                        maxIDs.itemName = maxIDInfoFile.ReadLine();
                        maxIDInfoFile.Close();
                    }
                    catch // The file is empty--> re-run
                    {
                        maxIDInfoFile.Close();

                        // Set this flag to true so that parsing doesn't include writting to output file or formatting, just parsing
                        maxIDRun = true;

                        // Change configuration to be All then come back again
                        String temp = configManager.outputFeatures;
                        //configManager.outputFeatures = "All";
                        if ((configManager.rootTrainDirectory != "") && (configManager.rootTrainDirectory != null))
                        {
                            // Parse train files
                            Parse(configManager.rootTrainDirectory, "Train", configManager.trainInputParsingMode, configManager.trainInputFormat);
                        }
                        if ((configManager.rootTestDirectory != "") && (configManager.rootTestDirectory != null))
                        {
                            // Parse test files
                            Parse(configManager.rootTestDirectory, "Test", configManager.testInputParsingMode, configManager.testInputFormat);
                        }

                        configManager.outputFeatures = temp;

                        //The last place in the bit-field is reserved to the not seen items
                        // But we don't add +1, since the wordsHashTable.Count already +1 from the max vocabularyWordID
                        // Ex: if the vocabularyWordID ranges from 0..49 then wordsHashTable.Count is 50
                        // So position wordsHashTable.Count = 50 is reserved to unseen word
                        maxIDs.vocabularyWordID = wordsHashTable.Count;
                        maxIDs.wordLength = 1;

                        // You need to make a one time parse to get the maxID's needed to represent features
                        // Note the files access this time: FileMode.OpenOrCreate not CreateNew
                        maxIDInfoFileWrite = new StreamWriter(File.Open(maxIDFullFileName, FileMode.OpenOrCreate, FileAccess.Write));
                        maxIDInfoFileWrite.AutoFlush = true;

                        maxIDInfoFileWrite.WriteLine(maxIDs.mrfType);
                        maxIDInfoFileWrite.WriteLine(maxIDs.p);
                        maxIDInfoFileWrite.WriteLine(maxIDs.r);
                        maxIDInfoFileWrite.WriteLine(maxIDs.f);
                        maxIDInfoFileWrite.WriteLine(maxIDs.s);
                        maxIDInfoFileWrite.WriteLine(maxIDs.POS_IDs[0]);
                        maxIDInfoFileWrite.WriteLine(maxIDs.POS_IDs[1]);
                        maxIDInfoFileWrite.WriteLine(maxIDs.equivalentPOS_ID);
                        maxIDInfoFileWrite.WriteLine(maxIDs.vocabularyWordID);
                        maxIDInfoFileWrite.WriteLine(maxIDs.wordLength);
                        
                        maxIDInfoFileWrite.WriteLine(maxIDs.itemName);
                        maxIDInfoFileWrite.Close();
                        maxIDRun = false;

                    }
                    break;
                default:
                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: ClassifySyntDiac, SYNT_DIAC or POS.", configManager.targetType);
                    break;

                
            }

            try
            {
                posNames = new String[maxIDs.POS_IDs[0]];
            }
            catch
            {
                posNames = null;
            }

        }

        // Constructor with no need to re-run maxIDRun
        public Parser(ConfigurationManager configManager, Logger logger, string maxIDFullFileName)
        {
            int[] POS_leastID = { LEAST_ID_VAL, LEAST_ID_VAL };
            maxIDs = new Item("", LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, POS_leastID, LEAST_ID_VAL, -1);

            this.configManager = configManager;
            this.logger = logger;

            totalNumWords = 0;
            misClassifiedWords = 0;

            TargetDiacAscii[] targetAscii = (TargetDiacAscii[])Enum.GetValues(typeof(TargetDiacAscii));
            TargetDiacCode[] targetDiacCodes = (TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode));
            // -1 to remove the DEFAULT case which is not actual diac. class to be added to hash table
            for (int i = 0; i < targetAscii.Length - 1; i++)
            {
                diacCodeToAscii.Add(targetDiacCodes[i], targetAscii[i]);
                diacAsciiToCode.Add((int)targetAscii[i], targetDiacCodes[i]);
            }


            // Initialize the classifier

            // Initialize the configMgr of the classifier
            if (configManager.clsParams != null)
            {
                ConfigurationManager clsConfigMgr = new ConfigurationManager(configManager.clsParams.configurationFileFullPath);

                switch (clsConfigMgr.trainInputFormat)
                {
                    case "ReadyFeatures":
                        classifier = new ReadyFeaturesParser(clsConfigMgr, logger, configManager.clsParams.maxIDInfoFileFullPath);
                        break;
                    case "RawTxt":
                        classifier = new RawTxtParser(clsConfigMgr, logger, configManager.clsParams.maxIDInfoFileFullPath);
                        break;
                    default:
                        classifier = new RawTxtParser(clsConfigMgr, logger, configManager.clsParams.maxIDInfoFileFullPath);
                        break;
                }// end switch

            }// end if
            // Initialize the POS taggers
            if (configManager.numPOSTaggers > 0)
            {
                posTaggers = new Parser[configManager.numPOSTaggers];

                for (int i = 0; i < configManager.posTaggersParams.Length; i++)
                {
                    switch (configManager.posTaggersParams[i].posTaggerType)
                    {
                        case "Stanford":
                            break;

                        case "DNN":
                            ConfigurationManager posTaggerConfigMgr = new ConfigurationManager(configManager.posTaggersParams[i].DNN_POSTaggerConfigurationFileFullPath);

                            switch (posTaggerConfigMgr.trainInputFormat)
                            {
                                case "ReadyFeatures":
                                    posTaggers[i] = new ReadyFeaturesParser(posTaggerConfigMgr, logger, configManager.posTaggersParams[i].DNN_POSTaggerMaxIDInfoFileFullPath);
                                    break;
                                case "RawTxt":
                                    posTaggers[i] = new RawTxtParser(posTaggerConfigMgr, logger, configManager.posTaggersParams[i].DNN_POSTaggerMaxIDInfoFileFullPath);
                                    break;
                                default:
                                    posTaggers[i] = new RawTxtParser(posTaggerConfigMgr, logger, configManager.posTaggersParams[i].DNN_POSTaggerMaxIDInfoFileFullPath);
                                    break;
                            }
                            posTaggers[i].posTaggerName = configManager.posTaggersParams[i].posTaggerName;

                            break;
                        default:
                            Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: Stanford or DNN.", configManager.posTaggersParams[i].posTaggerType);
                            break;
                    }// end switch
                }
            }

            // Just read the maxID's from the existing file
            StreamReader maxIDInfoFile = new StreamReader(File.Open(maxIDFullFileName, FileMode.Open, FileAccess.ReadWrite));


            maxIDRun = false;

            maxIDs.mrfType = Int32.Parse(maxIDInfoFile.ReadLine());
            maxIDs.p = Int32.Parse(maxIDInfoFile.ReadLine());
            maxIDs.r = Int32.Parse(maxIDInfoFile.ReadLine());
            maxIDs.f = Int32.Parse(maxIDInfoFile.ReadLine());
            maxIDs.s = Int32.Parse(maxIDInfoFile.ReadLine());
            maxIDs.POS_IDs[0] = Int32.Parse(maxIDInfoFile.ReadLine());
            maxIDs.POS_IDs[1] = Int32.Parse(maxIDInfoFile.ReadLine());
            maxIDs.equivalentPOS_ID = double.Parse(maxIDInfoFile.ReadLine());
            maxIDs.vocabularyWordID = Int32.Parse(maxIDInfoFile.ReadLine());
            maxIDs.wordLength = Int32.Parse(maxIDInfoFile.ReadLine());
            maxIDs.itemName = maxIDInfoFile.ReadLine();
            maxIDInfoFile.Close();

            try
            {
                posNames = new String[maxIDs.POS_IDs[0]];
            }
            catch
            {
                posNames = null;
            }

            // Start the file writer
            outFileWriter = new OutputFileWriter(configManager, logger, this, null, null);

            // Start the file
            outFileWriter.WriteOutputFile(null, OutFileMode.START);
        }


        // Method to parse the main directory
        // dataSetType: Train or Test
        // 
        public void Parse(String rootDirectory, String dataSetType, String parsingMode, String inputFilesFormat)
        {
            // Not need to write to output file if maxIDRun
            if (!maxIDRun)
            {
                // Start the file writer
                outFileWriter = new OutputFileWriter(configManager, logger, this, rootDirectory, dataSetType);

                // Start the file
                outFileWriter.WriteOutputFile(null, OutFileMode.START);

            }// end if (!maxIDRun)
            switch (parsingMode)
            {
                case "AnyFile":
                    ParseAnyFile(rootDirectory, dataSetType, inputFilesFormat);
                    break;
                case "FolderStructure":
                    ParseFolderStructure(rootDirectory, dataSetType, inputFilesFormat);
                    break;

            }// end switch

            // Not need to write to output file if maxIDRun
            if (!maxIDRun)
            {
                // Finalize the output file
                outFileWriter.WriteOutputFile(null, OutFileMode.FINISH);
            }

        }// end Parse()

        // Method to parse raw items in any file from the root directory
        public void ParseAnyFile(String rootDirectory, String dataSetType, String inputFilesFormat)
        {
            // Counter of parsed files
            uint numFiles = 0;

            // Temp items List list over all files to accomodate items. To be converted to items [] when full length is known.
            ArrayList itemsList = new ArrayList();

            // Parse recursively all directory tree to find any files
            numFiles += ParseAnyFileRecursive(rootDirectory, ref itemsList);

            // Copy items list to items array
            // First limit the size
            itemsList.TrimToSize();

            // Copy
            this.items = (Item[])itemsList.ToArray(itemsList[0].GetType());

            // Log parsing information
            LogParsingInfo(numFiles);

        }// end ParseAnyFile()

        // Method to parse raw items in any file from the root directory
        public uint ParseAnyFileRecursive(String rootDirectory, ref ArrayList wordsList)
        {
            uint numFiles = 0;

            // Parse files of this directory
            numFiles += ParseDirectoryFiles(rootDirectory, ref wordsList);

            // Get all sub-directories and parse them recursively
            foreach (String directory in Directory.GetDirectories(rootDirectory))
            {
                // Form new directory name
                // directory is full name
                // numFiles += ParseAnyFileRecursive(rootDirectory + configManager.directorySeparator + directory, ref itemsList);
                numFiles += ParseAnyFileRecursive(directory, ref wordsList);
            }


            return numFiles;

        }// end ParseAnyFile()

        // Method to parse raw items from the root directory for a known structure
        public void ParseFolderStructure(String rootDirectory, String dataSetType, String inputFilesFormat)
        {
            // Traverse the root directory
            String[] categoryFolders = Directory.GetDirectories(rootDirectory);

            // Temp string to build the current mrf folder name in it
            String currentFolderName;

            // Temp items List list over all files to accomodate items. To be converted to items [] when full length is known.
            ArrayList itemsList = new ArrayList();

            // Counter of parsed files
            uint numFiles = 0;


            // Counter of truly written items to out file
            // int numExamplesInOutFile = 0;

            // Parse files of each category
            foreach (String category in categoryFolders)
            {
                logger.LogTrace("Parsing files of category: " + category + "...");

                // Form the string of folder in the current category containing the files
                switch (inputFilesFormat)
                {
                    case "ReadyFeatures":
                        currentFolderName = category + configManager.directorySeparator + configManager.mrfFolderName;
                        break;
                    case "RawTxt":
                        currentFolderName = category + configManager.directorySeparator + configManager.txtFolderName;
                        break;
                    default:
                        currentFolderName = category + configManager.directorySeparator + configManager.txtFolderName;
                        break;
                }// end switch

                // Parse the files in the directory
                numFiles += ParseDirectoryFiles(currentFolderName, ref itemsList);

                logger.LogTrace("Finished parsing of category " + category);

            } // end forach categories traversing

            // Copy items list to items array
            // First limit the size
            itemsList.TrimToSize();

            // Copy
            this.items = (Item[])itemsList.ToArray(itemsList[0].GetType());

            // Log the POS names (if applicable)
            if (posNames != null)
            {
                logger.LogTrace("POS:");
                for (int j = 0; j < posNames.Length; j++)
                {
                    logger.LogTrace(posNames[j]);
                }
            }
            // Log parsing information
            LogParsingInfo(categoryFolders, numFiles);

        }// end ParseFolderStructure()

        // Method to parse files in a directory. It return items list and number of parsed files in that directory.
        private uint ParseDirectoryFiles(String currentFolderName, ref ArrayList wordsList)
        {
            // Counter of parsed files
            uint numFiles = 0;

            // Temp array list to hold the items parsed from the file
            ArrayList fileWordsList;

            // Temp array to hold the items parsed from the file
            Item[] fileWords;

            // Parse files of the folder
            foreach (String file in Directory.GetFiles(currentFolderName))
            {
                if ((file == currentFolderName + configManager.directorySeparator + "input_data.mat") ||
                   (file == currentFolderName + configManager.directorySeparator + "maxIDInfo.txt") ||
                   (file == currentFolderName + configManager.directorySeparator + "maxIDInfo.txt.bak"))
                {
                    continue;
                }
                // Increment number of files
                numFiles++;

                switch(configManager.targetType)
                {
                    case "SYNT_DIAC":
                    case "POS":
                        logger.LogTrace("Parsing file: " + numFiles.ToString() + "- " + file + "...");

                        // Temp array list to hold the items parsed from the file
                        fileWordsList = new ArrayList();

                        // Parse items in file into its structure
                        fileWordsList = FileParse(file);

                        //if (fileWordsList[)
                        // Add the word to the global items list
                        // Copy to be done by AddRange--Working
                        wordsList.AddRange(fileWordsList);

                        if (fileWordsList.Count != 0)
                        {
                            // Set the items array to the items list parsed by FileParse
                            fileWordsList.TrimToSize();
                            fileWords = (Item[])fileWordsList.ToArray(fileWordsList[0].GetType());

                            // Don't make any formatting if parsing is for maxID only
                            if (!maxIDRun)
                            {
                                // The features formatter
                                FeaturesFormatter featuresFormatter;

                                // Decide which type of formatter to use
                                switch (configManager.featuresFormat)
                                {
                                    case "Normal":
                                        featuresFormatter = new NormalFeaturesFormatter(configManager, this, logger, fileWords);
                                        break;
                                    case "Binary":
                                        featuresFormatter = new BinaryFeaturesFormatter(configManager, this, logger, fileWords);
                                        break;
                                    case "Bitfield":
                                        featuresFormatter = new BitFieldFeaturesFormatter(configManager, this, logger, fileWords);
                                        break;
                                    case "Raw":
                                        featuresFormatter = new RawFeaturesFormatter(configManager, this, logger, fileWords);
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect features format configuration. {0} is invalid configuration. Valid configurations are: Normal, Binary and Bitfield", configManager.featuresFormat);
                                        throw (new IndexOutOfRangeException());
                                }// end switch

                                // Format the items features of the file
                                try
                                {
                                    featuresFormatter.FormatFeatures();
                                }
                                catch (OutOfMemoryException)
                                {
                                    Console.WriteLine("Ooops! Out of memory");
                                }

                                // Start the context extractor
                                ContextExtractor contextExtractor = new ContextExtractor(featuresFormatter.features, logger, configManager);

                                // Extract the context extraction
                                contextExtractor.ContextExtract();

                                // Write (append) to output file
                                outFileWriter.WriteOutputFile(contextExtractor.contextFeatures, OutFileMode.APPEND);

                                // Free the features formatter for this file
                                featuresFormatter = null;

                                // Free the context extractor
                                contextExtractor = null;

                            }// end if (!maxIDRun)

                            logger.LogTrace("Parsing done successfully for the file");

                            // Free the file items list
                            fileWordsList = null;

                            // Force memory freeing
                            GC.Collect();

                        }// end if(fileWordsList.Count != 0)
                        else
                        {
                            logger.LogTrace("Empty File");
                        }

                        break;
                    case "FULL_DIAC":
                        logger.LogTrace("Parsing file: " + numFiles.ToString() + "- " + file + "...");

                        // Temp array list to hold the items parsed from the file
                        fileWordsList = new ArrayList();

                        // Parse items in file into its structure
                        fileWordsList = FileParse(file);

                        //if (fileWordsList[)
                        // Add the word to the global items list
                        // Copy to be done by AddRange--Working
                        wordsList.AddRange(fileWordsList);

                        if (fileWordsList.Count != 0)
                        {
                            // Set the items array to the items list parsed by FileParse
                            fileWordsList.TrimToSize();
                            fileWords = (Item[])fileWordsList.ToArray(fileWordsList[0].GetType());

                            int numWords = 0;

                            // Parse each word characters
                            foreach(Item word in fileWords)
                            {
                                // Adjust before and after words
                                if (numWords > 0)
                                {
                                    word.prevWord = fileWords[numWords - 1];
                                }
                                else
                                {
                                    word.prevWord = null;
                                }

                                if (numWords < (fileWords.Length - 1))
                                {
                                    word.nextWord = fileWords[numWords + 1];
                                }
                                else
                                {
                                    word.nextWord = null;
                                }

                                logger.LogTrace("Parsing word " + (++numWords) + " of file: " + numFiles.ToString() + "- " + file + "...");


                                ArrayList wordCharList = new ArrayList();

                                
                                wordCharList = WordParse(word);

                                if ((String)configManager.suppressFeaturesHashTable["SyntacticChar"] == "Suppress")
                                {
                                    if (wordCharList.Count > 1)
                                    {
                                        // Remove last character
                                        wordCharList.RemoveAt(wordCharList.Count - 1);
                                    }

                                }

                                if (wordCharList.Count != 0)
                                {

                                    // Set the items array to the items list parsed by FileParse
                                    wordCharList.TrimToSize();
                                    Item[] wordCharacters = (Item[])wordCharList.ToArray(wordCharList[0].GetType());

                                    // The features formatter
                                    FeaturesFormatter featuresFormatter;

                                    // Decide which type of formatter to use
                                    switch (configManager.featuresFormat)
                                    {
                                        case "Normal":
                                            featuresFormatter = new NormalFeaturesFormatter(configManager, this, logger, wordCharacters);
                                            break;
                                        case "Binary":
                                            featuresFormatter = new BinaryFeaturesFormatter(configManager, this, logger, wordCharacters);
                                            break;
                                        case "Bitfield":
                                            featuresFormatter = new BitFieldFeaturesFormatter(configManager, this, logger, wordCharacters);
                                            break;
                                        case "Raw":
                                            featuresFormatter = new RawFeaturesFormatter(configManager, this, logger, wordCharacters);
                                            break;
                                        default:
                                            Console.WriteLine("Incorrect features format configuration. {0} is invalid configuration. Valid configurations are: Normal, Binary and Bitfield", configManager.featuresFormat);
                                            throw (new IndexOutOfRangeException());
                                    }// end switch

                                    // Format the items features of the file
                                    try
                                    {
                                        featuresFormatter.FormatFeatures();
                                    }
                                    catch (OutOfMemoryException)
                                    {
                                        Console.WriteLine("Ooops! Out of memory");
                                    }

                                    // Start the context extractor
                                    ContextExtractor contextExtractor = new ContextExtractor(featuresFormatter.features, logger, configManager);

                                    // Extract the context extraction
                                    contextExtractor.ContextExtract();

                                    // Write (append) to output file
                                    outFileWriter.WriteOutputFile(contextExtractor.contextFeatures, OutFileMode.APPEND);

                                    // Free the features formatter for this file
                                    featuresFormatter = null;

                                    // Free the context extractor
                                    contextExtractor = null;

                                    logger.LogTrace("Parsing done successfully for the word");
                                }
                                else
                                {
                                    logger.LogTrace("Empty Word");
                                }

                                // Free the words char list
                                wordCharList = null;

                                // Force memory freeing
                                GC.Collect();

                            }// end foreach(Item word in fileWords)


                            logger.LogTrace("Parsing done successfully for the file");

                            // Free the file items list
                            fileWordsList = null;

                            // Force memory freeing
                            GC.Collect();

                        }// end if(fileWordsList.Count != 0)
                        else
                        {
                            logger.LogTrace("Empty File");
                        }

                        break;
                    case "ClassifySyntDiac":
                        logger.LogTrace("Classifying file: " + numFiles.ToString() + "- " + file + "...");

                        // Classify the file items
                        Feature[] classifiedWords = classifier.ClassifyFile(file, configManager.clsParams.finalNetFullPath);

                        if (classifiedWords != null)
                        {
                            totalNumWords += classifiedWords.Length;
                            // Open the file for writting
                            // Get file name
                            String[] fileParts = file.Split(configManager.directorySeparator.ToCharArray());

                            String concatenatedFileName = configManager.configEnvDirectory + configManager.directorySeparator + fileParts[fileParts.Length - 1];

                            StreamWriter classifiedFile = new StreamWriter(File.Open(concatenatedFileName, FileMode.OpenOrCreate, FileAccess.Write), Encoding.UTF8);
                            classifiedFile.AutoFlush = true;


                            foreach (Feature classifiedWord in classifiedWords)
                            {
                                // Form the concatenated raw word + target

                                String classifiedString = (classifiedWord.originalItem.itemNameWithProperDiacritics + (char)((int)diacCodeToAscii[(TargetDiacCode)classifiedWord.target[0]]));

                                // Log the items in the file
                                classifiedFile.Write(classifiedString + " ");

                                if ((TargetDiacCode)classifiedWord.target[0] != FeaturesFormatter.GetSyntDiac(classifiedWord.originalItem.itemName))
                                {
                                    misClassifiedWords ++;
                                }
                            }
                            logger.LogTrace("Misclassified items " + misClassifiedWords + " out of " + totalNumWords);
                            float accuracy = (((float)(totalNumWords - misClassifiedWords) / totalNumWords) * 100);
                            logger.LogTrace("Accuracy " + accuracy.ToString() + @"%");

                            // Close the file
                            classifiedFile.Close();
                            logger.LogTrace("Classification done successfully for the file");
                        }
                        else
                        {
                            logger.LogError("Classification was not done due to some erros (may be .pos file is empty)", ErrorCode.CLASSIFICATION_ERROR);
                        }
                        
                        break;
                    default:
                        Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: ClassifySyntDiac, SYNT_DIAC or POS.", configManager.targetType);
                        break;
                }// end switch


            }// end foreach file directory parse

            return numFiles;
        }// ParseDirectoryFiles


        public Feature[] ClassifyFile(String fullFileName, String finalNetFullPath)
        {

            logger.LogTrace("Classifying file: " + fullFileName + "...");

            // Temp array list to hold the items parsed from the file
            ArrayList fileWordsList = new ArrayList();

            // Temp array to hold the items parsed from the file
            Item[] fileWords;

            // Parse items in file into its structure
            fileWordsList = FileParse(fullFileName);


            if (fileWordsList.Count != 0)
            {
                // Set the items array to the items list parsed by FileParse
                fileWordsList.TrimToSize();
                fileWords = (Item[])fileWordsList.ToArray(fileWordsList[0].GetType());

                // The features formatter
                FeaturesFormatter featuresFormatter;

                // Decide which type of formatter to use
                switch (configManager.featuresFormat)
                {
                    case "Normal":
                        featuresFormatter = new NormalFeaturesFormatter(configManager, this, logger, fileWords);
                        break;
                    case "Binary":
                        featuresFormatter = new BinaryFeaturesFormatter(configManager, this, logger, fileWords);
                        break;
                    case "Bitfield":
                        featuresFormatter = new BitFieldFeaturesFormatter(configManager, this, logger, fileWords);
                        break;
                    case "Raw":
                        featuresFormatter = new RawFeaturesFormatter(configManager, this, logger, fileWords);
                        break;
                    default:
                        Console.WriteLine("Incorrect features format configuration. {0} is invalid configuration. Valid configurations are: Normal, Binary and Bitfield", configManager.featuresFormat);
                        throw (new IndexOutOfRangeException());
                }// end switch

                // Format the items features of the file
                try
                {
                    featuresFormatter.FormatFeatures();
                }
                catch (OutOfMemoryException)
                {
                    Console.WriteLine("Ooops! Out of memory");
                }

                // Start the context extractor
                ContextExtractor contextExtractor = new ContextExtractor(featuresFormatter.features, logger, configManager);

                // Extract the context extraction
                contextExtractor.ContextExtract();

                // Write (append) to output file
                outFileWriter.ClassifyMatlab(ref contextExtractor.contextFeatures, finalNetFullPath);

                // Free the features formatter for this file
                featuresFormatter = null;

                // Free the context extractor
                Feature[] contextFeaturesLocal = (Feature[])contextExtractor.contextFeatures.Clone();
                contextExtractor = null;

                logger.LogTrace("Classification done successfully for the file");

                // Free the file items list
                fileWordsList = null;

                // Force memory freeing
                GC.Collect();

                return contextFeaturesLocal;

            }// end if(fileWordsList.Count != 0)
            else
            {
                logger.LogTrace("Empty File");
                return null;
            }// end else
           
        }// end ClassifyFile()

        // Method to get all {} tags in a file
        protected abstract String [] GetTags(String fileName);

        // Method to parse a file
        protected abstract ArrayList FileParse(String fullFileName);

        // Method to parse a tag into itemName and array of ID's
        protected abstract void TagParse(String tag, out String itemName, out int[] IDs, bool stanfordMapStanfordToRDI);

        protected abstract void POSTagParse(String tag, out String itemName, out int[] IDs, ref String[] POS);

        // Method to log the parsing info of a parse process in the trace file
        protected void LogParsingInfo(String[] categoryFolders, uint numFiles)
        {

            logger.LogTrace("Finished parsing");
            logger.LogTrace("Total number of categories: " + categoryFolders.Length.ToString());
            logger.LogTrace("Total parsed Files: " + numFiles.ToString());
            logger.LogTrace("Total number of items: " + items.Length.ToString());
            if (outFileWriter != null)
            {
                logger.LogTrace("Total number of items actually written to file: " + outFileWriter.numExamplesInOutFile.ToString());
            }
            logger.LogTrace("Max ID of mrfType is " + maxIDs.mrfType + " needs " + GetNumBits(maxIDs.mrfType).ToString() + " bits");
            logger.LogTrace("Max ID of prefix is " + maxIDs.p + " needs " + GetNumBits(maxIDs.p).ToString() + " bits");
            logger.LogTrace("Max ID of root is " + maxIDs.r + " needs " + GetNumBits(maxIDs.r).ToString() + " bits");
            logger.LogTrace("Max ID of form is " + maxIDs.f + " needs " + GetNumBits(maxIDs.f).ToString() + " bits");
            logger.LogTrace("Max ID of suffix is " + maxIDs.s + " needs " + GetNumBits(maxIDs.s).ToString() + " bits");
            logger.LogTrace("Max ID of POS is " + maxIDs.POS_IDs[0] + " needs " + GetNumBits(maxIDs.POS_IDs[0]).ToString() + " bits");
            logger.LogTrace("Total number of needed bits for binary representation (POS is bit-field): " + (GetNumBits(maxIDs.mrfType) + GetNumBits(maxIDs.p) + GetNumBits(maxIDs.r) + GetNumBits(maxIDs.f) + GetNumBits(maxIDs.s) + maxIDs.POS_IDs[0] + 1).ToString());

            // The +1 is added because maxID bitfieldValue means we could have positions from 0 to this maxID, so total of maxID + 1 positions
            logger.LogTrace("Total number of needed bits for bit-field representation: " + ((maxIDs.mrfType + 1) + (maxIDs.p + 1) + (maxIDs.r + 1) + (maxIDs.f + 1) + (maxIDs.s + 1) + (maxIDs.POS_IDs[0] + 1)).ToString());

            // The items features required sizes
            logger.LogTrace("Total number of needed bits for binary representation of Words: " + (GetNumBits(maxIDs.vocabularyWordID)).ToString());
            logger.LogTrace("Total number of needed bits for bit-field representation of Words on WordLevel encoding: " + (maxIDs.vocabularyWordID).ToString());
            logger.LogTrace("Longest word is: " + maxIDs.itemName);
            logger.LogTrace("Total number of needed bits for bit-field representation of Words on CharLevel encoding: " + (maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN).ToString());
            logger.LogInfo();

        }
        
        // Method to log the parsing info of a parse process in the trace file
        protected void LogParsingInfo(uint numFiles)
        {
            logger.LogTrace("Finished parsing");
            logger.LogTrace("Total parsed Files: " + numFiles.ToString());
            logger.LogTrace("Total number of items: " + items.Length.ToString());
            if (outFileWriter != null)
            {
                logger.LogTrace("Total number of items actually written to file: " + outFileWriter.numExamplesInOutFile.ToString());
            }
            logger.LogTrace("Max ID of mrfType is " + maxIDs.mrfType + " needs " + GetNumBits(maxIDs.mrfType).ToString() + " bits");
            logger.LogTrace("Max ID of prefix is " + maxIDs.p + " needs " + GetNumBits(maxIDs.p).ToString() + " bits");
            logger.LogTrace("Max ID of root is " + maxIDs.r + " needs " + GetNumBits(maxIDs.r).ToString() + " bits");
            logger.LogTrace("Max ID of form is " + maxIDs.f + " needs " + GetNumBits(maxIDs.f).ToString() + " bits");
            logger.LogTrace("Max ID of suffix is " + maxIDs.s + " needs " + GetNumBits(maxIDs.s).ToString() + " bits");
            logger.LogTrace("Max ID of POS is " + maxIDs.POS_IDs[0] + " needs " + GetNumBits(maxIDs.POS_IDs[0]).ToString() + " bits");
            logger.LogTrace("Total number of needed bits for binary representation (POS is bit-field): " + (GetNumBits(maxIDs.mrfType) + GetNumBits(maxIDs.p) + GetNumBits(maxIDs.r) + GetNumBits(maxIDs.f) + GetNumBits(maxIDs.s) + maxIDs.POS_IDs[0] + 1).ToString());

            // The +1 is added because maxID bitfieldValue means we could have positions from 0 to this maxID, so total of maxID + 1 positions
            logger.LogTrace("Total number of needed bits for bit-field representation: " + ((maxIDs.mrfType + 1) + (maxIDs.p + 1) + (maxIDs.r + 1) + (maxIDs.f + 1) + (maxIDs.s + 1) + (maxIDs.POS_IDs[0] + 1)).ToString());

            // The items features required sizes
            logger.LogTrace("Total number of needed bits for binary representation of Words: " + (GetNumBits(maxIDs.vocabularyWordID)).ToString());
            logger.LogTrace("Total number of needed bits for bit-field representation of Words on WordLevel encoding: " + (maxIDs.vocabularyWordID).ToString());
            logger.LogTrace("Longest word is: " + maxIDs.itemName);
            logger.LogTrace("Total number of needed bits for bit-field representation of Words on CharLevel encoding: " + (maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN).ToString());
            logger.LogInfo();
        }
        
        // Method to fill in Item structure with POS data
        protected abstract void POSFillItemIDs(int[] IDs, ref Item word);
        
        // Method to add vocabulary ID
        protected void UpdateWordVocabularyID(ref Item word)
        {
            String withoutDiacWord;
            String addedWord;
            // Check if the first character falls between 0x0620 = 1568 and 0x0652 = 1618 => the characters of Arabic including DIACS
            // This indicates it's a word, not numbers or punc. marks
            /*if (!(word.itemName[0] <= 1618 && word.itemName[0] >= 1568))
            {
                // Put at the last reserved position.
                word.vocabularyWordID = parser.maxIDs.vocabularyWordID;
                word.itemName = "";
                word.itemNameWithProperDiacritics = "";                            
                logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + parser.maxIDs.vocabularyWordID, ErrorCode.OUT_OF_VOCABULARY_WORD);
                return;
            }*/
            //String[] wordParts = RemoveNonArabic(word.itemName);
            //String addedWordParts;

            //word.itemName = wordParts[0];
            word.itemName = RemoveNonArabic(word.itemName);
            if (word.itemName == "")
            {
                return;
            }
            // Check if the word characters falls between 0x0620 = 1568 and 0x0652 = 1618 => the characters of Arabic including DIACS
            // This indicates it's a word, not numbers or punc. marks
            /*
            foreach (char wordChar in word.itemName)
            {
                if (!(wordChar <= 1618 && wordChar >= 1568))
                {
                    // Put at the last reserved position.
                    word.vocabularyWordID = parser.maxIDs.vocabularyWordID;
                    word.itemName = "";
                    word.itemNameWithProperDiacritics = "";  
                    logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + parser.maxIDs.vocabularyWordID, ErrorCode.OUT_OF_VOCABULARY_WORD);
                    return;
                }

            }
             */
            switch (configManager.wordOnlyVocabularyScope)
            {
                case "AsIs":
                    addedWord = word.itemName;

                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            // Add to hashtable
                            if (!wordsHashTable.Contains(word.itemName))
                            {
                                if ((wordsHashTable.Count < maxIDs.vocabularyWordID) || maxIDRun)
                                {
                                    // New word

                                    // Form new record
                                    Item newWord = new Item(word.itemName, word.mrfType, word.r, word.r, word.f, word.s, word.POS_IDs, word.equivalentPOS_ID, wordsHashTable.Count);

                                    // Update the passed word vocabulary ID
                                    // The vocabulary ID starts from 0 not 1, that's why addition is done before inserting the new word not after
                                    word.vocabularyWordID = wordsHashTable.Count;

                                    // Add to hashtable
                                    wordsHashTable.Add(word.itemName, newWord);

                                    // addedWord = word.itemName;
                                }
                                else
                                {
                                    // Put at the last reserved position.
                                    word.vocabularyWordID = maxIDs.vocabularyWordID;
                                    logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + maxIDs.vocabularyWordID, ErrorCode.OUT_OF_VOCABULARY_WORD);
                                    // addedWord = String.Empty;

                                }

                            }// end if(!wordsHashTable.Contains(word.itemName))
                            else
                            {
                                // Item already exists

                                // Update the passed word vocabulary ID from the table
                                word.vocabularyWordID = ((Item)wordsHashTable[word.itemName]).vocabularyWordID;

                                // Existing word, increment the frequency
                                ((Item)wordsHashTable[word.itemName]).frequency++;

                                //addedWord = word.itemName;

                            }//end else

                            // Log the word without diacritics equivalent
                            withoutDiacWord = RawTxtParser.RemoveDiacritics(word.itemName);

                            // If it already exists in the hashtable, then update its referenced word based on fequency
                            if (wordsHashTable.Contains(withoutDiacWord))
                            {
                                // if the frequency of the old reference of the correspondig diac word is less than the new word (with diac too), then refer to the new one
                                if (((Item)wordsHashTable[withoutDiacWord]).frequency < ((Item)wordsHashTable[word.itemName]).frequency)
                                {
                                    // Modify the reference to the higher frequency word
                                    wordsHashTable[withoutDiacWord] = (Item)wordsHashTable[word.itemName];
                                    //wordsHashTable.Remove(withoutDiacWord);
                                    //wordsHashTable.Add(withoutDiacWord, (Item)wordsHashTable[word.itemName]);


                                }

                            }// end if (wordsHashTable.Contains(withoutDiacWord))
                            else
                            {
                                // New withoudDiacWord--> Add with referene to new (current) word
                                wordsHashTable.Add(withoutDiacWord, word);

                            }// end else if (wordsHashTable.Contains(withoutDiacWord))


                            break;
                        case "CharacterLevel":
                            // Do nothing
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch  

                    break;

                case "NoDiac":

                    // Log the word without diacritics equivalent
                    withoutDiacWord = RawTxtParser.RemoveDiacritics(word.itemName);
                    addedWord = withoutDiacWord;

                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":

                            // Add to hashtable
                            if (!wordsHashTable.Contains(withoutDiacWord))
                            {
                                if ((wordsHashTable.Count < maxIDs.vocabularyWordID) || maxIDRun)
                                {
                                    // New word

                                    // Form new record
                                    Item newWord = new Item(word.itemName, word.mrfType, word.r, word.r, word.f, word.s, word.POS_IDs, word.equivalentPOS_ID, wordsHashTable.Count);

                                    // Update the passed word vocabulary ID
                                    // The vocabulary ID starts from 0 not 1, that's why addition is done before inserting the new word not after
                                    word.vocabularyWordID = wordsHashTable.Count;

                                    // Add to hashtable
                                    wordsHashTable.Add(withoutDiacWord, newWord);
                                    //addedWord = withoutDiacWord;
                                }
                                else
                                {
                                    // Put at the last reserved position.
                                    word.vocabularyWordID = maxIDs.vocabularyWordID;
                                    logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + maxIDs.vocabularyWordID, ErrorCode.OUT_OF_VOCABULARY_WORD);
                                    //addedWord = String.Empty;

                                }

                            }// end if(!wordsHashTable.Contains(word.itemName))
                            else
                            {
                                // Item already exists

                                // Update the passed word vocabulary ID from the table
                                word.vocabularyWordID = ((Item)wordsHashTable[withoutDiacWord]).vocabularyWordID;

                                // Existing word, increment the frequency
                                ((Item)wordsHashTable[withoutDiacWord]).frequency++;
                                //addedWord = withoutDiacWord;

                            }//end else

                            break;
                        case "CharacterLevel":
                            // Do nothing
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch 


                    break;
                case "RemoveSyntacticDiac":
                    String wordWithoutSyntacticDiac = RemoveSyntacticDiac(word.itemName);
                    addedWord = wordWithoutSyntacticDiac;
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            if (wordWithoutSyntacticDiac == "اَلْعَبَّاس")//"لِحِجَاب"
                            {
                                int x = 0;
                                x++;
                            }

                            // Add to hashtable
                            if (!wordsHashTable.Contains(wordWithoutSyntacticDiac))
                            {
                                if ((wordsHashTable.Count < maxIDs.vocabularyWordID) || maxIDRun)
                                {
                                    // New word

                                    // Form new record
                                    Item newWord = new Item(word.itemName, word.mrfType, word.r, word.r, word.f, word.s, word.POS_IDs, word.equivalentPOS_ID, wordsHashTable.Count);

                                    // Update the passed word vocabulary ID
                                    // The vocabulary ID starts from 0 not 1, that's why addition is done before inserting the new word not after
                                    word.vocabularyWordID = wordsHashTable.Count;

                                    // Add to hashtable
                                    wordsHashTable.Add(wordWithoutSyntacticDiac, newWord);

                                    //addedWord = wordWithoutSyntacticDiac;
                                }
                                else
                                {
                                    // Put at the last reserved position.
                                    word.vocabularyWordID = maxIDs.vocabularyWordID;
                                    logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + maxIDs.vocabularyWordID, ErrorCode.OUT_OF_VOCABULARY_WORD);
                                    //addedWord = String.Empty;
                                }

                            }// end if(!wordsHashTable.Contains(word.itemName))
                            else
                            {
                                // Item already exists

                                // Update the passed word vocabulary ID from the table
                                word.vocabularyWordID = ((Item)wordsHashTable[wordWithoutSyntacticDiac]).vocabularyWordID;

                                // Existing word, increment the frequency
                                ((Item)wordsHashTable[wordWithoutSyntacticDiac]).frequency++;

                                //addedWord = wordWithoutSyntacticDiac;

                            }//end else

                            // Log the word without diacritics equivalent
                            withoutDiacWord = RawTxtParser.RemoveDiacritics(wordWithoutSyntacticDiac);

                            // If it already exists in the hashtable, then update its referenced word based on fequency
                            if (wordsHashTable.Contains(withoutDiacWord))
                            {

                                // if the frequency of the old reference of the correspondig diac word is less than the new word (with diac too), then refer to the new one
                                if (((Item)wordsHashTable[withoutDiacWord]).frequency < ((Item)wordsHashTable[wordWithoutSyntacticDiac]).frequency)
                                {
                                    // Modify the reference to the higher frequency word
                                    wordsHashTable[withoutDiacWord] = (Item)wordsHashTable[wordWithoutSyntacticDiac];
                                    //wordsHashTable.Remove(withoutDiacWord);
                                    //wordsHashTable.Add(withoutDiacWord, (Item)wordsHashTable[word.itemName]);

                                }


                            }// end if (wordsHashTable.Contains(withoutDiacWord))
                            else
                            {
                                // New withoudDiacWord--> Add with referene to new (current) word
                                wordsHashTable.Add(withoutDiacWord, (Item)wordsHashTable[wordWithoutSyntacticDiac]);

                            }// end else if (wordsHashTable.Contains(withoutDiacWord))

                            break;
                        case "CharacterLevel":
                            // Do nothing
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch 

                    break;
                default:
                    addedWord = String.Empty;
                    Console.WriteLine("Incorrect WordOnlyVocabularyScope configuration. {0} is invalid configuration. Valid configurations are: AsIs, NoDiac and RemoveSyntacticDiac.", configManager.wordOnlyVocabularyScope);
                    break;
            }
            //if(word.vocabularyWordID > 144533)
            //{
            //    int x = 0;
            //    x++;
            //}

            // Update the itemNameWithProperDiacritics
            word.itemNameWithProperDiacritics = addedWord;

            // Update the maximum word length
            if (maxIDRun)
            {
                if (addedWord.Length > maxIDs.wordLength)
                {
                    maxIDs.wordLength = addedWord.Length;
                    maxIDs.itemName = addedWord;
                }
            }

            // Set the no diac word
            word.wordNameWithNoDiac = RawTxtParser.RemoveDiacritics(word.itemNameWithProperDiacritics);

        }// end UpdateWordVocabularyID(ref Item word)

        // Method to convert array of positions in bitfield into equivalent ID
        // Ex: POS_Ids = {1, 3, 0}-->1011-->
        protected double MapPOSToEquivalentID(int[] POS_IDs)
        {
            double equivalentID = 0;

            foreach (int ID in POS_IDs)
            {
                equivalentID += Math.Pow(2, ID);
            }// end foreach

            return equivalentID;
        }// end MapPOSToEquivalentID

        // Method to calculate the number of bits needed for binary representation of a bitfieldValue
        public static int GetNumBits(int val)
        {
            double numBits = (Math.Log10((double)val) / Math.Log10(2.0));
            return (numBits%2 == 0) ? (int)numBits : (int)(Math.Floor(numBits) + 1);
        }// end GetNumBits

        // Utility to remove the syntactic diac sign
        private String RemoveSyntacticDiac(String itemName)
        {
            /*String wordWithoutSyntacticDiac = String.Empty;
            
            // Detect diac sign by traversing the string reversely
            bool found = false;

            // Counter of number of depth characters
            int i = 0;

            // Position of the character
            int j = 0;

            while (!found)
            {
                // Check if the character fall between 0x064B and 0x0652=> the diac sign, then the syntactic diag sign is found
                if ((int)itemName.ToCharArray()[itemName.Length - j] >= 1611 && (int)itemName.ToCharArray()[itemName.Length - j] <= 1618)
                {
                    found = false;
                    break;
                }

                // Move to next character before the current one
                j++;

            }*/

            // Check if the character fall between 0x064B and 0x0652=> the diac sign, then the syntactic diag sign is found
            // Otherwise return the word as is
            return  ((int)itemName.ToCharArray()[itemName.Length - 1] >= 1611 && (int)itemName.ToCharArray()[itemName.Length - 1] <= 1618) ? itemName.Remove(itemName.Length - 1): itemName;
        }// end RemoveSyntacticDiac


        // Method to remove non-arabic characters and punctuation signs
        //private String[] RemoveNonArabic(String initialInputTxt)
        private String RemoveNonArabic(String initialInputTxt)
        {
            String stringWithoutNonArabic = String.Empty;
            //char [] wordCharacters = itemName.ToCharArray();
            String[] inputTxtParts = Regex.Split(initialInputTxt, @"[-\s\(\)\\\.\،\*\;\:\{\}""]+");

            //for (int i = 0; i < inputTxtParts.Length; i++)
            //{
                // Loop on all characters of the given word
                //foreach (char wordCharacter in inputTxtParts[i])
                foreach (char wordCharacter in inputTxtParts[0])
                {
                    // Check if the first character falls between 0x0620 = 1568 and 0x0652 = 1618 => the characters of Arabic including DIACS
                    // This indicates it's a word, not numbers or punc. marks
                    if (wordCharacter <= 1618 && wordCharacter >= 1568)
                    {
                        stringWithoutNonArabic += wordCharacter;
                    }// end if
                    else
                    {
                        //logger.LogError("Word " + inputTxtParts[i] + " contains non-arabic character " + wordCharacter, ErrorCode.NON_ARABIC_CHAR);
                        logger.LogError("Word " + inputTxtParts[0] + " contains non-arabic character " + wordCharacter, ErrorCode.NON_ARABIC_CHAR);
                    }

                }// end foreach
              //  inputTxtParts[i] = stringWithoutNonArabic;
            //}

            //return inputTxtParts;
                return stringWithoutNonArabic;
        }// end RemoveNonArabic

        // Method to parse the word string, and return array of items, each has itemName = character
        // target = the DIAC sign assigned to it, which can be skipped in case of NoDiac is set in WordOnlyVocabularyScope
        private ArrayList WordParse(Item word)
        {
            /*if (word.itemNameWithProperDiacritics == "د")
            {
                int x = 0;
                x++;
            }*/
            //String wordName = RemoveNonArabic(word.itemName);
            ArrayList wordCharList = new ArrayList();
            bool diac;
            Item newChar = null;
            int charPosition = 0;
            switch (configManager.wordOnlyVocabularyScope)
            {
                case "AsIs":
                case "RemoveSyntacticDiac":
                    // First character is not expected to be diac sign
                    diac = false;

                    // Loop on all characters and fill in the array list
                    foreach (char character in word.itemNameWithProperDiacritics)
                    {
                        if (diac)
                        {
                            // Expecting diac sign
                            if (diacAsciiToCode[(int)character] != null)
                            {
                                // It's true diac sign
                                // Append the diac sign
                                newChar.target = (int)diacAsciiToCode[(int)character];

                                if ((newChar.target == 1) || (newChar.target == 2) || (newChar.target == 3))
                                {
                                    int x = 0;
                                    x++;
                                }

                                // Add char position
                                newChar.charPosition = charPosition;

                                charPosition++;

                                // Insert it in the list
                                wordCharList.Add(newChar);

                                // return it to null again to wait its normal character
                                newChar = null;

                                GC.Collect();

                                // Wait for normal character
                                diac = false;


                            }
                            else
                            {
                                // Keep waiting the diac sign to come
                            }
                        }// end if (diac == true)
                        else
                        {
                            // Expecting normal character
                            // Check if the character falls between 0x0620 and 0x064A=> the characters of Arabic
                            if ((int)character >= 1568 &&
                                (int)character <= 1610)
                            {
                                // Keep the same parameters of the original words for the individual characters
                                //newChar = new Item();
                                newChar = new Item(word);// All parsed parameters before will be kept except the itemName, which will now be the character name
                                newChar.itemName = character.ToString();
                                newChar.itemNameWithProperDiacritics = newChar.itemName;
                                //newChar.wordNameWithNoDiac = RawTxtParser.RemoveDiacritics(word.itemNameWithProperDiacritics);
                                
                                diac = true;
                            }
                            else
                            {
                                // Keep waiting for a normal character
                            }
                        }// end else (diac == false)
                    }// end foreach
                    break;
                case "NoDiac": // No need to wait diac sign, just log the item (char) itself
                    // Log the word without diacritics equivalent
                    //wordName = RawTxtParser.RemoveDiacritics(wordName);

                    // Loop on all characters and fill in the array list
                    foreach (char character in word.itemNameWithProperDiacritics)
                    {
                        newChar = new Item(word);
                        newChar.target = (int)TargetDiacCode.DEFAULT;
                        newChar.itemName = character.ToString();
                        newChar.itemNameWithProperDiacritics = newChar.itemName;
                        //newChar.wordNameWithNoDiac = RawTxtParser.RemoveDiacritics(word.itemNameWithProperDiacritics);
                        // Add char position
                        newChar.charPosition = charPosition;

                        charPosition++;

                        wordCharList.Add(newChar);

                    }// end foreach
                    break;

                default:                   
                    Console.WriteLine("Incorrect WordOnlyVocabularyScope configuration. {0} is invalid configuration. Valid configurations are: AsIs, NoDiac and RemoveSyntacticDiac.", configManager.wordOnlyVocabularyScope);
                    break;
            }// end switch
            return wordCharList;
        }// end WordParse

    }//end class
}
