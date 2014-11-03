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
        // Flag to determine if this run is for max ID determination or not 
        // Set this flag to true so that parsing doesn't include writting to output file or formatting, just parsing
        protected bool maxIDRun;

        // Array of words structures
        public Word[] words;

        // Instance of the pre-processing configuration manager
        protected ConfigurationManager configManager;

        // Logger instance
        protected Logger logger;

        // Maximum IDs
        public static Word maxIDs;

        public String[] posNames;

        // Hashmap to indicate a word previously added or not
        private Hashtable wordsHashTable = new Hashtable();

        // The outout file writer instance. It'll be inisantiated with the begining of parse
        OutputFileWriter outFileWriter;

        //StreamWriter wordsLogFile;

        // Constants
        protected const int LEAST_ID_VAL = -100;

        // Constructor
        public Parser(ConfigurationManager configManager, Logger logger)
        {
            int[] POS_leastID = { LEAST_ID_VAL, LEAST_ID_VAL };
            maxIDs = new Word("", LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, POS_leastID, LEAST_ID_VAL, -1);

            this.configManager = configManager;
            this.logger = logger;


            String maxIDFullFileName = configManager.rootTrainDirectory + configManager.directorySeparator + "maxIDInfo.txt";
            //wordsLogFile = new StreamWriter(File.Open(@"E:\Documents and Settings\ASALLAB\Desktop\Flash\PhD_Flash\Implementation\Diactrization\Preprocessing\words.txt", FileMode.OpenOrCreate, FileAccess.Write), Encoding.GetEncoding(1256));
            //wordsLogFile.AutoFlush = true;
            // public static int DateTime.Compare(DateTime t1,DateTime t2)
            // Less than zero : t1 is earlier than t2. 
            // Zero : 	t1 is the same as t2. 
            // Greater than zero : t1 is later than t2. 
            // The condition means: the mxIDinfo file exists and configuration is not updated; so configFile write time is earlier that maxIDInfo write time
            if (File.Exists(maxIDFullFileName) && (DateTime.Compare(File.GetLastWriteTimeUtc(configManager.configFullFileName),
                                                                    File.GetLastWriteTimeUtc(maxIDFullFileName)) < 0))
            {
                // Just read the maxID's from the existing file
                StreamReader maxIDInfoFile = new StreamReader(File.Open(maxIDFullFileName, FileMode.Open, FileAccess.ReadWrite));

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
                    maxIDInfoFile.Close();
                }
                catch // The file is empty--> re-run
                {
                    maxIDInfoFile.Close();

                    // Set this flag to true so that parsing doesn't include writting to output file or formatting, just parsing
                    maxIDRun = true;

                    // Change configuration to be All then come back again
                    String temp = configManager.outputFeatures;
                    configManager.outputFeatures = "All";
                    // Parse train files
                    Parse(configManager.rootTrainDirectory, "Train", configManager.trainInputParsingMode, configManager.trainInputFormat);
                    // Parse test files
                    Parse(configManager.rootTestDirectory, "Test", configManager.testInputParsingMode, configManager.testInputFormat);

                    configManager.outputFeatures = temp;

                    // Update the maxIDs with the maximum number of un-repeated (unique) words
                    maxIDs.vocabularyWordID = wordsHashTable.Count;

                    // You need to make a one time parse to get the maxID's needed to represent features
                    // Note the files access this time: FileMode.OpenOrCreate not CreateNew
                    StreamWriter maxIDInfoFileWrite = new StreamWriter(File.Open(maxIDFullFileName, FileMode.OpenOrCreate, FileAccess.Write));
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
                    maxIDInfoFileWrite.Close();
                    maxIDRun = false;

                }

            }
            else
            {
                maxIDRun = true;

                // Change configuration to be All then come back again
                String temp = configManager.outputFeatures;
                configManager.outputFeatures = "All";
                // Parse train files
                Parse(configManager.rootTrainDirectory, "Train", configManager.trainInputParsingMode, configManager.trainInputFormat);
                // Parse test files
                Parse(configManager.rootTestDirectory, "Test", configManager.testInputParsingMode, configManager.testInputFormat);
                configManager.outputFeatures = temp;

                // Update the maxIDs with the maximum number of un-repeated (unique) words
                maxIDs.vocabularyWordID = wordsHashTable.Count;

                // You need to make a one time parse to get the maxID's needed to represent features
                // Note the files access this time: FileMode.OpenOrCreate not CreateNew, bcz the file could exist but outdated
                StreamWriter maxIDInfoFile = new StreamWriter(File.Open(maxIDFullFileName, FileMode.OpenOrCreate, FileAccess.Write));
                maxIDInfoFile.AutoFlush = true;

                maxIDInfoFile.WriteLine(maxIDs.mrfType);
                maxIDInfoFile.WriteLine(maxIDs.p);
                maxIDInfoFile.WriteLine(maxIDs.r);
                maxIDInfoFile.WriteLine(maxIDs.f);
                maxIDInfoFile.WriteLine(maxIDs.s);
                maxIDInfoFile.WriteLine(maxIDs.POS_IDs[0]);
                maxIDInfoFile.WriteLine(maxIDs.POS_IDs[1]);
                maxIDInfoFile.WriteLine(maxIDs.equivalentPOS_ID);
                maxIDInfoFile.WriteLine(maxIDs.vocabularyWordID);
                maxIDInfoFile.Close();
                maxIDRun = false;
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


        // Method to parse the main directory
        // dataSetType: Train or Test
        // 
        public void Parse(String rootDirectory, String dataSetType, String parsingMode, String inputFilesFormat)
        {
            // Not need to write to output file if maxIDRun
            if (!maxIDRun)
            {
                // Start the file writer
                outFileWriter = new OutputFileWriter(configManager, logger, rootDirectory, dataSetType);

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

        // Method to parse raw words in any file from the root directory
        public void ParseAnyFile(String rootDirectory, String dataSetType, String inputFilesFormat)
        {
            // Counter of parsed files
            uint numFiles = 0;

            // Temp words List list over all files to accomodate words. To be converted to words [] when full length is known.
            ArrayList wordsList = new ArrayList();

            // Parse recursively all directory tree to find any files
            numFiles += ParseAnyFileRecursive(rootDirectory, ref wordsList);

            // Copy words list to words array
            // First limit the size
            wordsList.TrimToSize();

            // Copy
            this.words = (Word[])wordsList.ToArray(wordsList[0].GetType());

            // Log parsing information
            LogParsingInfo(numFiles);

        }// end ParseAnyFile()

        // Method to parse raw words in any file from the root directory
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
                // numFiles += ParseAnyFileRecursive(rootDirectory + configManager.directorySeparator + directory, ref wordsList);
                numFiles += ParseAnyFileRecursive(directory, ref wordsList);
            }


            return numFiles;

        }// end ParseAnyFile()

        // Method to parse raw words from the root directory for a known structure
        public void ParseFolderStructure(String rootDirectory, String dataSetType, String inputFilesFormat)
        {
            // Traverse the root directory
            String[] categoryFolders = Directory.GetDirectories(rootDirectory);

            // Temp string to build the current mrf folder name in it
            String currentFolderName;

            // Temp words List list over all files to accomodate words. To be converted to words [] when full length is known.
            ArrayList wordsList = new ArrayList();

            // Counter of parsed files
            uint numFiles = 0;


            // Counter of truly written words to out file
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
                numFiles += ParseDirectoryFiles(currentFolderName, ref wordsList);

                logger.LogTrace("Finished parsing of category " + category);

            } // end forach categories traversing

            // Copy words list to words array
            // First limit the size
            wordsList.TrimToSize();

            // Copy
            this.words = (Word[])wordsList.ToArray(wordsList[0].GetType());

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

        // Method to parse files in a directory. It return words list and number of parsed files in that directory.
        private uint ParseDirectoryFiles(String currentFolderName, ref ArrayList wordsList)
        {
            // Counter of parsed files
            uint numFiles = 0;

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

                logger.LogTrace("Parsing file: " + numFiles.ToString() + "- " + file + "...");

                // Temp array list to hold the words parsed from the file
                ArrayList fileWordsList = new ArrayList();

                // Temp array to hold the words parsed from the file
                Word[] fileWords;

                // Parse words in file into its structure
                fileWordsList = FileParse(file);

                //if (fileWordsList[)
                // Add the word to the global words list
                // Copy to be done by AddRange--Working
                wordsList.AddRange(fileWordsList);

                if (fileWordsList.Count != 0)
                {
                    // Set the words array to the words list parsed by FileParse
                    fileWordsList.TrimToSize();
                    fileWords = (Word[])fileWordsList.ToArray(fileWordsList[0].GetType());

                    // Don't make any formatting if parsing is for maxID only
                    if (!maxIDRun)
                    {
                        // The features formatter
                        FeaturesFormatter featuresFormatter;

                        // Decide which type of formatter to use
                        switch (configManager.featuresFormat)
                        {
                            case "Normal":
                                featuresFormatter = new NormalFeaturesFormatter(configManager, logger, fileWords);
                                break;
                            case "Binary":
                                featuresFormatter = new BinaryFeaturesFormatter(configManager, logger, fileWords);
                                break;
                            case "Bitfield":
                                featuresFormatter = new BitFieldFeaturesFormatter(configManager, logger, fileWords);
                                break;
                            case "Raw":
                                featuresFormatter = new RawFeaturesFormatter(configManager, logger, fileWords);
                                break;
                            default:
                                Console.WriteLine("Incorrect features format configuration. {0} is invalid configuration. Valid configurations are: Normal, Binary and Bitfield", configManager.featuresFormat);
                                throw (new IndexOutOfRangeException());
                        }// end switch

                        // Format the words features of the file
                        try
                        {
                            featuresFormatter.FormatFeatures();
                        }
                        catch (OutOfMemoryException)
                        {
                            Console.WriteLine("Ooops! Out of memory");
                        }

                        // Start the context extractor
                        ContextExtractor contextExtractor = new ContextExtractor(featuresFormatter.wordsFeatures, logger, configManager);

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
                    
                    // Free the file words list
                    fileWordsList = null;

                    // Force memory freeing
                    GC.Collect();                    

                }// end if(fileWordsList.Count != 0)
                else
                {
                    logger.LogTrace("Empty File");
                }

            }// end foreach file directory parse

            return numFiles;
        }

        // Method to get all {} tags in a file
        protected abstract String [] GetTags(String fileName);

        // Method to parse a file
        protected abstract ArrayList FileParse(String fullFileName);

        // Method to parse a tag into wordName and array of ID's
        protected abstract void TagParse(String tag, out String wordName, out int[] IDs);

        protected abstract void POSTagParse(String tag, out String wordName, out int[] IDs, ref String[] POS);

        // Method to log the parsing info of a parse process in the trace file
        protected void LogParsingInfo(String[] categoryFolders, uint numFiles)
        {

            logger.LogTrace("Finished parsing");
            logger.LogTrace("Total number of categories: " + categoryFolders.Length.ToString());
            logger.LogTrace("Total parsed Files: " + numFiles.ToString());
            logger.LogTrace("Total number of words: " + words.Length.ToString());
            if (outFileWriter != null)
            {
                logger.LogTrace("Total number of words actually written to file: " + outFileWriter.numExamplesInOutFile.ToString());
            }
            logger.LogTrace("Max ID of mrfType is " + maxIDs.mrfType + " needs " + GetNumBits(maxIDs.mrfType).ToString() + " bits");
            logger.LogTrace("Max ID of prefix is " + maxIDs.p + " needs " + GetNumBits(maxIDs.p).ToString() + " bits");
            logger.LogTrace("Max ID of root is " + maxIDs.r + " needs " + GetNumBits(maxIDs.r).ToString() + " bits");
            logger.LogTrace("Max ID of form is " + maxIDs.f + " needs " + GetNumBits(maxIDs.f).ToString() + " bits");
            logger.LogTrace("Max ID of suffix is " + maxIDs.s + " needs " + GetNumBits(maxIDs.s).ToString() + " bits");
            logger.LogTrace("Max ID of POS is " + maxIDs.POS_IDs[0] + " needs " + GetNumBits(maxIDs.POS_IDs[0]).ToString() + " bits");
            logger.LogTrace("Total number of needed bits for binary representation (POS is bit-field): " + (GetNumBits(maxIDs.mrfType) + GetNumBits(maxIDs.p) + GetNumBits(maxIDs.r) + GetNumBits(maxIDs.f) + GetNumBits(maxIDs.s) + maxIDs.POS_IDs[0] + 1).ToString());

            // The +1 is added because maxID value means we could have positions from 0 to this maxID, so total of maxID + 1 positions
            logger.LogTrace("Total number of needed bits for bit-field representation: " + ((maxIDs.mrfType + 1) + (maxIDs.p + 1) + (maxIDs.r + 1) + (maxIDs.f + 1) + (maxIDs.s + 1) + (maxIDs.POS_IDs[0] + 1)).ToString());

            // The words features required sizes
            logger.LogTrace("Total number of needed bits for binary representation of Words: " + (GetNumBits(maxIDs.vocabularyWordID)).ToString());
            logger.LogTrace("Total number of needed bits for bit-field representation of Words: " + (maxIDs.vocabularyWordID).ToString());

            logger.LogInfo();

        }
        
        // Method to log the parsing info of a parse process in the trace file
        protected void LogParsingInfo(uint numFiles)
        {
            logger.LogTrace("Finished parsing");
            logger.LogTrace("Total parsed Files: " + numFiles.ToString());
            logger.LogTrace("Total number of words: " + words.Length.ToString());
            if (outFileWriter != null)
            {
                logger.LogTrace("Total number of words actually written to file: " + outFileWriter.numExamplesInOutFile.ToString());
            }
            logger.LogTrace("Max ID of mrfType is " + maxIDs.mrfType + " needs " + GetNumBits(maxIDs.mrfType).ToString() + " bits");
            logger.LogTrace("Max ID of prefix is " + maxIDs.p + " needs " + GetNumBits(maxIDs.p).ToString() + " bits");
            logger.LogTrace("Max ID of root is " + maxIDs.r + " needs " + GetNumBits(maxIDs.r).ToString() + " bits");
            logger.LogTrace("Max ID of form is " + maxIDs.f + " needs " + GetNumBits(maxIDs.f).ToString() + " bits");
            logger.LogTrace("Max ID of suffix is " + maxIDs.s + " needs " + GetNumBits(maxIDs.s).ToString() + " bits");
            logger.LogTrace("Max ID of POS is " + maxIDs.POS_IDs[0] + " needs " + GetNumBits(maxIDs.POS_IDs[0]).ToString() + " bits");
            logger.LogTrace("Total number of needed bits for binary representation (POS is bit-field): " + (GetNumBits(maxIDs.mrfType) + GetNumBits(maxIDs.p) + GetNumBits(maxIDs.r) + GetNumBits(maxIDs.f) + GetNumBits(maxIDs.s) + maxIDs.POS_IDs[0] + 1).ToString());

            // The +1 is added because maxID value means we could have positions from 0 to this maxID, so total of maxID + 1 positions
            logger.LogTrace("Total number of needed bits for bit-field representation: " + ((maxIDs.mrfType + 1) + (maxIDs.p + 1) + (maxIDs.r + 1) + (maxIDs.f + 1) + (maxIDs.s + 1) + (maxIDs.POS_IDs[0] + 1)).ToString());

            // The words features required sizes
            logger.LogTrace("Total number of needed bits for binary representation of Words: " + (GetNumBits(maxIDs.vocabularyWordID)).ToString());
            logger.LogTrace("Total number of needed bits for bit-field representation of Words: " + (maxIDs.vocabularyWordID).ToString());

            logger.LogInfo();
        }
        
        // Method to fill in Word structure with POS data
        protected abstract void POSFillWordIDs(int[] IDs, ref Word word);
        
        // Method to add vocabulary ID
        protected void UpdateWordVocabularyID(ref Word word)
        {
            // Add to hashtable
            if (!wordsHashTable.Contains(word.wordName))
            {
                // New word

                // Form new record
                Word newWord = new Word(word.wordName, word.mrfType, word.r, word.r, word.f, word.s, word.POS_IDs, word.equivalentPOS_ID, wordsHashTable.Count + 1);

                // Add to hashtable
                wordsHashTable.Add(word.wordName, newWord);

                // Update the passed word vocabulary ID
                // The vocabulary ID starts from 1 not 0, that's why addition is done after inserting the new word not before
                word.vocabularyWordID = wordsHashTable.Count;

            }// end if(!wordsHashTable.Contains(word.wordName))
            else
            {
                // Word already exists

                // Update the passed word vocabulary ID from the table
                word.vocabularyWordID = ((Word)wordsHashTable[word.wordName]).vocabularyWordID;

                // Existing word, increment the frequency
                ((Word)wordsHashTable[word.wordName]).frequency++;

            }//end else

            if(word.vocabularyWordID > 47202)
            {
                int x = 0;
                x++;
            }
        }// end UpdateWordVocabularyID(ref Word word)

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

        // Method to calculate the number of bits needed for binary representation of a value
        public static int GetNumBits(int val)
        {
            double numBits = (Math.Log10((double)val) / Math.Log10(2.0));
            return (numBits%2 == 0) ? (int)numBits : (int)(Math.Floor(numBits) + 1);                 
        }


        
    }//end class
}
