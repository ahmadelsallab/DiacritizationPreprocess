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
        // Array of words structures
        public Word[] words;

        // Instance of the pre-processing configuration manager
        protected ConfigurationManager configManager;

        // Logger instance
        protected Logger logger;

        // Maximum IDs
        public static Word maxIDs;

        public String[] posNames;

        //StreamWriter wordsLogFile;

        // Constants
        protected const int LEAST_ID_VAL = -100;

        // Constructor
        public Parser(ConfigurationManager configManager, Logger logger)
        {
            int[] POS_leastID = { LEAST_ID_VAL, LEAST_ID_VAL };
            maxIDs = new Word("", LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, LEAST_ID_VAL, POS_leastID, LEAST_ID_VAL);
        }


        // Method to parse the main directory
        public void Parse(String rootDirectory, String mode, String inputPOSFormat)
        {
            // Traverse the root directory
            String[] categoryFolders = Directory.GetDirectories(rootDirectory);

            // Temp string to build the current mrf folder name in it
            String currentFolderName;

            // Temp words List list over all files to accomodate words. To be converted to words [] when full length is known.
            ArrayList wordsList = new ArrayList();

            // The features formatter
            FeaturesFormatter featuresFormatter;

            // Start the file writer
            OutputFileWriter outFileWriter = new OutputFileWriter(configManager, logger, rootDirectory, mode);

            // Start the file
            outFileWriter.WriteOutputFile(null, OutFileMode.START);

            // Counter of parsed files
            uint numFiles = 0;

            // Counter of truly written words to out file
            // int numExamplesInOutFile = 0;

            // Parse files of each category
            foreach (String category in categoryFolders)
            {
                logger.LogTrace("Parsing files of category: " + category + "...");

                // Form the string of mrf folder in the current category
                switch (inputPOSFormat)
                {
                    case "RDI":
                        currentFolderName = category + configManager.directorySeparator + configManager.mrfFolderName;
                        break;
                    case "Stanford":
                        currentFolderName = category + configManager.directorySeparator + configManager.txtFolderName;
                        break;
                    default:
                        currentFolderName = category + configManager.directorySeparator + configManager.txtFolderName;
                        break;
                }// end switch
               

                // Parse files of mrf folder
                foreach (String file in Directory.GetFiles(currentFolderName))
                {
                    // Increment number of files
                    numFiles++;

                    logger.LogTrace("Parsing file: " + numFiles.ToString() + "- " + file + "...");

                    if (numFiles == 44)
                    {
                        int x;
                    }
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

                        // Accumulate numExamplesInOutFile 
                        //numExamplesInOutFile += outFileWriter.numExamplesInOutFile;

                        /*if (numExamplesInOutFile == 460)
                        {
                            int x = 1;
                        }*/

                        logger.LogTrace("Parsing done successfully for the file");

                        // Free the file words list
                        fileWordsList = null;

                        // Free the features formatter for this file
                        featuresFormatter = null;

                        // Free the context extractor
                        contextExtractor = null;

                        // Force memory freeing
                        GC.Collect();

                    }// end if(fileWordsList.Count != 0)
                    else
                    {
                        logger.LogTrace("Empty File");
                    }


                }// end foreach mrf directory parse

                logger.LogTrace("Finished parsing of category " + category);

            } // end forach categories traversing

            // Copy words list to words array
            // First limit the size
            wordsList.TrimToSize();
            logger.LogTrace("POS:");
            for (int j = 0; j < posNames.Length; j++)
            {
                logger.LogTrace(posNames[j]);
            }
            // Copy
            this.words = (Word[])wordsList.ToArray(wordsList[0].GetType());

            // Log parsing information
            LogParsingInfo(categoryFolders, numFiles, outFileWriter);

            // Finalize the file
            outFileWriter.WriteOutputFile(null, OutFileMode.FINISH);

        }// end Parse()
        

        // Method to get all {} tags in a file
        protected abstract String [] GetTags(String fileName);

        // Method to parse a file
        protected abstract ArrayList FileParse(String fullFileName);

        // Method to parse a tag into wordName and array of ID's
        protected abstract void TagParse(String tag, out String wordName, out int[] IDs);


        protected abstract void POSTagParse(String tag, out String wordName, out int[] IDs, ref String[] POS);

        // Method to log the parsing info of a parse process in the trace file
        protected void LogParsingInfo(String[] categoryFolders, uint numFiles, OutputFileWriter outFileWriter)
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
            logger.LogInfo();

        }

        // Method to fill in Word structure with POS data
        protected abstract void POSFillWordIDs(int[] IDs, ref Word word);

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
