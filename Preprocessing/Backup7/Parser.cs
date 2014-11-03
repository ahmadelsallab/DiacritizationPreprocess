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
                    maxIDs.wordLength = Int32.Parse(maxIDInfoFile.ReadLine());
                    maxIDs.wordName = maxIDInfoFile.ReadLine();
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
                    Parse(configManager.rootTrainDirectory, "Train", configManager.trainInputParsingMode, configManager.trainInputFormat);
                    // Parse test files
                    Parse(configManager.rootTestDirectory, "Test", configManager.testInputParsingMode, configManager.testInputFormat);

                    configManager.outputFeatures = temp;

                    //The last place in the bit-field is reserved to the not seen words
                    // But we don't add +1, since the wordsHashTable.Count already +1 from the max vocabularyWordID
                    // Ex: if the vocabularyWordID ranges from 0..49 then wordsHashTable.Count is 50
                    // So position wordsHashTable.Count = 50 is reserved to unseen word
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
                    maxIDInfoFileWrite.WriteLine(maxIDs.wordLength);
                    maxIDInfoFileWrite.WriteLine(maxIDs.wordName);
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
                // Parse train files
                Parse(configManager.rootTrainDirectory, "Train", configManager.trainInputParsingMode, configManager.trainInputFormat);
                // Parse test files
                Parse(configManager.rootTestDirectory, "Test", configManager.testInputParsingMode, configManager.testInputFormat);
                configManager.outputFeatures = temp;

                // Update the maxIDs with the maximum number of un-repeated (unique) words
                //maxIDs.vocabularyWordID = wordsHashTable.Count;

                //The last place in the bit-field is reserved to the not seen words
                // But we don't add +1, since the wordsHashTable.Count already +1 from the max vocabularyWordID
                // Ex: if the vocabularyWordID ranges from 0..49 then wordsHashTable.Count is 50
                // So position wordsHashTable.Count = 50 is reserved to unseen word
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
                maxIDInfoFile.WriteLine(maxIDs.wordLength);
                maxIDInfoFile.WriteLine(maxIDs.wordName);
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

            // The +1 is added because maxID bitfieldValue means we could have positions from 0 to this maxID, so total of maxID + 1 positions
            logger.LogTrace("Total number of needed bits for bit-field representation: " + ((maxIDs.mrfType + 1) + (maxIDs.p + 1) + (maxIDs.r + 1) + (maxIDs.f + 1) + (maxIDs.s + 1) + (maxIDs.POS_IDs[0] + 1)).ToString());

            // The words features required sizes
            logger.LogTrace("Total number of needed bits for binary representation of Words: " + (GetNumBits(maxIDs.vocabularyWordID)).ToString());
            logger.LogTrace("Total number of needed bits for bit-field representation of Words on WordLevel encoding: " + (maxIDs.vocabularyWordID).ToString());
            logger.LogTrace("Longest word is: " + maxIDs.wordName);
            logger.LogTrace("Total number of needed bits for bit-field representation of Words on CharLevel encoding: " + (maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN).ToString());
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

            // The +1 is added because maxID bitfieldValue means we could have positions from 0 to this maxID, so total of maxID + 1 positions
            logger.LogTrace("Total number of needed bits for bit-field representation: " + ((maxIDs.mrfType + 1) + (maxIDs.p + 1) + (maxIDs.r + 1) + (maxIDs.f + 1) + (maxIDs.s + 1) + (maxIDs.POS_IDs[0] + 1)).ToString());

            // The words features required sizes
            logger.LogTrace("Total number of needed bits for binary representation of Words: " + (GetNumBits(maxIDs.vocabularyWordID)).ToString());
            logger.LogTrace("Total number of needed bits for bit-field representation of Words on WordLevel encoding: " + (maxIDs.vocabularyWordID).ToString());
            logger.LogTrace("Longest word is: " + maxIDs.wordName);
            logger.LogTrace("Total number of needed bits for bit-field representation of Words on CharLevel encoding: " + (maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN).ToString());
            logger.LogInfo();
        }
        
        // Method to fill in Word structure with POS data
        protected abstract void POSFillWordIDs(int[] IDs, ref Word word);
        
        // Method to add vocabulary ID
        protected void UpdateWordVocabularyID(ref Word word)
        {
            String withoutDiacWord;
            String addedWord;
            // Check if the first character falls between 0x0620 = 1568 and 0x0652 = 1618 => the characters of Arabic including DIACS
            // This indicates it's a word, not numbers or punc. marks
            /*if (!(word.wordName[0] <= 1618 && word.wordName[0] >= 1568))
            {
                // Put at the last reserved position.
                word.vocabularyWordID = Parser.maxIDs.vocabularyWordID;
                word.wordName = "";
                word.wordNameWithProperDiacritics = "";                            
                logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + Parser.maxIDs.vocabularyWordID, ErrorCode.OUT_OF_VOCABULARY_WORD);
                return;
            }*/
            word.wordName = RemoveNonArabic(word.wordName);
            if (word.wordName == "")
            {
                return;
            }
            // Check if the word characters falls between 0x0620 = 1568 and 0x0652 = 1618 => the characters of Arabic including DIACS
            // This indicates it's a word, not numbers or punc. marks
            /*
            foreach (char wordChar in word.wordName)
            {
                if (!(wordChar <= 1618 && wordChar >= 1568))
                {
                    // Put at the last reserved position.
                    word.vocabularyWordID = Parser.maxIDs.vocabularyWordID;
                    word.wordName = "";
                    word.wordNameWithProperDiacritics = "";  
                    logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + Parser.maxIDs.vocabularyWordID, ErrorCode.OUT_OF_VOCABULARY_WORD);
                    return;
                }

            }
             */
            switch (configManager.wordOnlyVocabularyScope)
            {
                case "AsIs":
                    // Add to hashtable
                    if (!wordsHashTable.Contains(word.wordName))
                    {
                        if ((wordsHashTable.Count < Parser.maxIDs.vocabularyWordID) || maxIDRun)
                        {
                            // New word

                            // Form new record
                            Word newWord = new Word(word.wordName, word.mrfType, word.r, word.r, word.f, word.s, word.POS_IDs, word.equivalentPOS_ID, wordsHashTable.Count);

                            // Update the passed word vocabulary ID
                            // The vocabulary ID starts from 0 not 1, that's why addition is done before inserting the new word not after
                            word.vocabularyWordID = wordsHashTable.Count;                            

                            // Add to hashtable
                            wordsHashTable.Add(word.wordName, newWord);

                            addedWord = word.wordName;
                        }
                        else
                        {
                            // Put at the last reserved position.
                            word.vocabularyWordID = Parser.maxIDs.vocabularyWordID;
                            logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + Parser.maxIDs.vocabularyWordID, ErrorCode.OUT_OF_VOCABULARY_WORD);
                            addedWord = String.Empty;

                        }

                    }// end if(!wordsHashTable.Contains(word.wordName))
                    else
                    {
                        // Word already exists

                        // Update the passed word vocabulary ID from the table
                        word.vocabularyWordID = ((Word)wordsHashTable[word.wordName]).vocabularyWordID;

                        // Existing word, increment the frequency
                        ((Word)wordsHashTable[word.wordName]).frequency++;

                        addedWord = word.wordName;

                    }//end else

                    // Log the word without diacritics equivalent
                    withoutDiacWord = RawTxtParser.RemoveDiacritics(word.wordName);

                    // If it already exists in the hashtable, then update its referenced word based on fequency
                    if (wordsHashTable.Contains(withoutDiacWord))
                    {
                        // if the frequency of the old reference of the correspondig diac word is less than the new word (with diac too), then refer to the new one
                        if (((Word)wordsHashTable[withoutDiacWord]).frequency < ((Word)wordsHashTable[word.wordName]).frequency)
                        {
                            // Modify the reference to the higher frequency word
                            wordsHashTable[withoutDiacWord] = (Word)wordsHashTable[word.wordName];
                            //wordsHashTable.Remove(withoutDiacWord);
                            //wordsHashTable.Add(withoutDiacWord, (Word)wordsHashTable[word.wordName]);
                            

                        }

                    }// end if (wordsHashTable.Contains(withoutDiacWord))
                    else
                    {
                        // New withoudDiacWord--> Add with referene to new (current) word
                        wordsHashTable.Add(withoutDiacWord, word);

                    }// end else if (wordsHashTable.Contains(withoutDiacWord))

                    break;

                case "NoDiac":

                    // Log the word without diacritics equivalent
                    withoutDiacWord = RawTxtParser.RemoveDiacritics(word.wordName);

                    // Add to hashtable
                    if (!wordsHashTable.Contains(withoutDiacWord))
                    {
                        if ((wordsHashTable.Count < Parser.maxIDs.vocabularyWordID) || maxIDRun)
                        {
                            // New word

                            // Form new record
                            Word newWord = new Word(word.wordName, word.mrfType, word.r, word.r, word.f, word.s, word.POS_IDs, word.equivalentPOS_ID, wordsHashTable.Count);

                            // Update the passed word vocabulary ID
                            // The vocabulary ID starts from 0 not 1, that's why addition is done before inserting the new word not after
                            word.vocabularyWordID = wordsHashTable.Count;

                            // Add to hashtable
                            wordsHashTable.Add(withoutDiacWord, newWord);
                            addedWord = withoutDiacWord;
                        }
                        else
                        {
                            // Put at the last reserved position.
                            word.vocabularyWordID = Parser.maxIDs.vocabularyWordID;
                            logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + Parser.maxIDs.vocabularyWordID, ErrorCode.OUT_OF_VOCABULARY_WORD);
                            addedWord = String.Empty;

                        }

                    }// end if(!wordsHashTable.Contains(word.wordName))
                    else
                    {
                        // Word already exists

                        // Update the passed word vocabulary ID from the table
                        word.vocabularyWordID = ((Word)wordsHashTable[withoutDiacWord]).vocabularyWordID;

                        // Existing word, increment the frequency
                        ((Word)wordsHashTable[withoutDiacWord]).frequency++;
                        addedWord = withoutDiacWord;

                    }//end else


                    break;
                case "RemoveSyntacticDiac":
                    String wordWithoutSyntacticDiac = RemoveSyntacticDiac(word.wordName);
                    if (wordWithoutSyntacticDiac == "لِحِجَاب")
                    {
                        int x = 0;
                        x++;
                    }

                    // Add to hashtable
                    if (!wordsHashTable.Contains(wordWithoutSyntacticDiac))
                    {
                        if ((wordsHashTable.Count < Parser.maxIDs.vocabularyWordID) || maxIDRun)
                        {
                            // New word

                            // Form new record
                            Word newWord = new Word(word.wordName, word.mrfType, word.r, word.r, word.f, word.s, word.POS_IDs, word.equivalentPOS_ID, wordsHashTable.Count);

                            // Update the passed word vocabulary ID
                            // The vocabulary ID starts from 0 not 1, that's why addition is done before inserting the new word not after
                            word.vocabularyWordID = wordsHashTable.Count;

                            // Add to hashtable
                            wordsHashTable.Add(wordWithoutSyntacticDiac, newWord);

                            addedWord = wordWithoutSyntacticDiac;
                        }
                        else
                        {
                            // Put at the last reserved position.
                            word.vocabularyWordID = Parser.maxIDs.vocabularyWordID;
                            logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + Parser.maxIDs.vocabularyWordID, ErrorCode.OUT_OF_VOCABULARY_WORD);
                            addedWord = String.Empty;
                        }

                    }// end if(!wordsHashTable.Contains(word.wordName))
                    else
                    {
                        // Word already exists

                        // Update the passed word vocabulary ID from the table
                        word.vocabularyWordID = ((Word)wordsHashTable[wordWithoutSyntacticDiac]).vocabularyWordID;

                        // Existing word, increment the frequency
                        ((Word)wordsHashTable[wordWithoutSyntacticDiac]).frequency++;

                        addedWord = wordWithoutSyntacticDiac;

                    }//end else

                    // Log the word without diacritics equivalent
                    withoutDiacWord = RawTxtParser.RemoveDiacritics(wordWithoutSyntacticDiac);

                    // If it already exists in the hashtable, then update its referenced word based on fequency
                    if (wordsHashTable.Contains(withoutDiacWord))
                    {
                        // if the frequency of the old reference of the correspondig diac word is less than the new word (with diac too), then refer to the new one
                        if (((Word)wordsHashTable[withoutDiacWord]).frequency < ((Word)wordsHashTable[wordWithoutSyntacticDiac]).frequency)
                        {
                            // Modify the reference to the higher frequency word
                            wordsHashTable[withoutDiacWord] = (Word)wordsHashTable[wordWithoutSyntacticDiac];
                            //wordsHashTable.Remove(withoutDiacWord);
                            //wordsHashTable.Add(withoutDiacWord, (Word)wordsHashTable[word.wordName]);

                        }

                    }// end if (wordsHashTable.Contains(withoutDiacWord))
                    else
                    {
                        // New withoudDiacWord--> Add with referene to new (current) word
                        wordsHashTable.Add(withoutDiacWord, (Word)wordsHashTable[wordWithoutSyntacticDiac]);

                    }// end else if (wordsHashTable.Contains(withoutDiacWord))

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

            // Update the wordNameWithProperDiacritics
            word.wordNameWithProperDiacritics = addedWord;

            // Update the maximum word length
            if (maxIDRun)
            {
                if (addedWord.Length > maxIDs.wordLength)
                {
                    maxIDs.wordLength = addedWord.Length;
                    maxIDs.wordName = addedWord;
                }
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

        // Method to calculate the number of bits needed for binary representation of a bitfieldValue
        public static int GetNumBits(int val)
        {
            double numBits = (Math.Log10((double)val) / Math.Log10(2.0));
            return (numBits%2 == 0) ? (int)numBits : (int)(Math.Floor(numBits) + 1);
        }// end GetNumBits

        // Utility to remove the syntactic diac sign
        private String RemoveSyntacticDiac(String wordName)
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
                if ((int)wordName.ToCharArray()[wordName.Length - j] >= 1611 && (int)wordName.ToCharArray()[wordName.Length - j] <= 1618)
                {
                    found = false;
                    break;
                }

                // Move to next character before the current one
                j++;

            }*/

            // Check if the character fall between 0x064B and 0x0652=> the diac sign, then the syntactic diag sign is found
            // Otherwise return the word as is
            return  ((int)wordName.ToCharArray()[wordName.Length - 1] >= 1611 && (int)wordName.ToCharArray()[wordName.Length - 1] <= 1618) ? wordName.Remove(wordName.Length - 1): wordName;
        }// end RemoveSyntacticDiac


        // Method to remove non-arabic characters and punctuation signs
        private String RemoveNonArabic(String inputTxt)
        {
            String stringWithoutNonArabic = String.Empty;
            //char [] wordCharacters = wordName.ToCharArray();

            // Loop on all characters of the given word
            foreach (char wordCharacter in inputTxt)
            {
                // Check if the first character falls between 0x0620 = 1568 and 0x0652 = 1618 => the characters of Arabic including DIACS
                // This indicates it's a word, not numbers or punc. marks
                if (wordCharacter <= 1618 && wordCharacter >= 1568)
                {
                    stringWithoutNonArabic += wordCharacter;
                }// end if
                else
                {
                    logger.LogError("Word " + inputTxt + " contains non-arabic character " + wordCharacter, ErrorCode.NON_ARABIC_CHAR);
                }
            }// end foreach

            return stringWithoutNonArabic;
        }// end RemoveNonArabic

    }//end class
}
