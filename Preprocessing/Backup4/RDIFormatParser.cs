using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace Preprocessing
{
    class RDIFormatParser : Parser
    {
        // Flag to determine if this run is for max ID determination or not 
        private bool maxIDRun;

        // Constructor
        public RDIFormatParser(ConfigurationManager configManager, Logger logger)
              : base(configManager, logger)
        {
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
                    maxIDInfoFile.Close();
                }
                catch // The file is empty--> re-run
                {
                    maxIDInfoFile.Close();

                    maxIDRun = true;

                    // You need to make a one time parse to get the maxID's needed to represent features
                    // Note the files access this time: FileMode.OpenOrCreate not CreateNew
                    StreamWriter maxIDInfoFileWrite = new StreamWriter(File.Open(maxIDFullFileName, FileMode.OpenOrCreate, FileAccess.Write));
                    maxIDInfoFileWrite.AutoFlush = true;
                    ParseForMaxID();
                    maxIDInfoFileWrite.WriteLine(maxIDs.mrfType);
                    maxIDInfoFileWrite.WriteLine(maxIDs.p);
                    maxIDInfoFileWrite.WriteLine(maxIDs.r);
                    maxIDInfoFileWrite.WriteLine(maxIDs.f);
                    maxIDInfoFileWrite.WriteLine(maxIDs.s);
                    maxIDInfoFileWrite.WriteLine(maxIDs.POS_IDs[0]);
                    maxIDInfoFileWrite.WriteLine(maxIDs.POS_IDs[1]);
                    maxIDInfoFileWrite.WriteLine(maxIDs.equivalentPOS_ID);
                    maxIDInfoFileWrite.Close();

                }

            }
            else
            {
                maxIDRun = true;

                // You need to make a one time parse to get the maxID's needed to represent features
                // Note the files access this time: FileMode.OpenOrCreate not CreateNew, bcz the file could exist but outdated
                StreamWriter maxIDInfoFile = new StreamWriter(File.Open(maxIDFullFileName, FileMode.OpenOrCreate, FileAccess.Write));
                maxIDInfoFile.AutoFlush = true;
                // Change configuration to be MrfAndPOS then come back again
                String temp = configManager.outputFeatures;
                configManager.outputFeatures = "MrfAndPOS";
                ParseForMaxID();
                configManager.outputFeatures = temp;
                maxIDInfoFile.WriteLine(maxIDs.mrfType);
                maxIDInfoFile.WriteLine(maxIDs.p);
                maxIDInfoFile.WriteLine(maxIDs.r);
                maxIDInfoFile.WriteLine(maxIDs.f);
                maxIDInfoFile.WriteLine(maxIDs.s);
                maxIDInfoFile.WriteLine(maxIDs.POS_IDs[0]);
                maxIDInfoFile.WriteLine(maxIDs.POS_IDs[1]);
                maxIDInfoFile.WriteLine(maxIDs.equivalentPOS_ID);
                maxIDInfoFile.Close();
            }

            posNames = new String[maxIDs.POS_IDs[0]];
            

        }// end constructor

        // Method to parse the main directory
        private void ParseForMaxID()
        {
            // Traverse the root directory
            String[] categoryFolders = Directory.GetDirectories(configManager.rootTrainDirectory);

            // Temp string to build the current mrf folder name in it
            String currentMrfFolderName;

            // Temp words List list over all files to accomodate words. To be converted to words [] when full length is known.
            ArrayList wordsList = new ArrayList();

            // Counter of parsed files
            uint numFiles = 0;

            // Parse files of each category
            foreach (String category in categoryFolders)
            {
                logger.LogTrace("Parsing files of category: " + category + "...");

                // Form the string of mrf folder in the current category
                currentMrfFolderName = category + configManager.directorySeparator + configManager.mrfFolderName;

                // Parse files of mrf folder
                foreach (String file in Directory.GetFiles(currentMrfFolderName))
                {
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

                        /*if (numExamplesInOutFile == 460)
                        {
                            int x = 1;
                        }*/

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


                }// end foreach mrf directory parse

                logger.LogTrace("Finished parsing of category " + category);

            } // end forach categories traversing

            // Copy words list to words array
            // First limit the size
            wordsList.TrimToSize();

            // Copy
            this.words = (Word[])wordsList.ToArray(wordsList[0].GetType());

            // Log parsing information
            LogParsingInfo(categoryFolders, numFiles, null);


        }// end ParseForMaxID()

        // Method to get all {} tags in a file
        protected override String [] GetTags(String fileName)
        {
            // Temp array list for tags. To be converted to array.
            ArrayList tagList = new ArrayList();

            // The array to be built and returned
            String[] tagStrings = null;

            // Open the iput file. BE careful with the encoding
            StreamReader inputFile = new StreamReader(File.Open(fileName,
                                                      FileMode.OpenOrCreate, FileAccess.Read),
                                                      Encoding.GetEncoding(1256)); // Encoding is windows-1256 like in Notepad ++: Encoding-->Character Sets --> Arabic--> Windows-1256

            logger.LogTrace("Getting tags of: " + fileName);

            // Read input file in one string
            String inputTxt = inputFile.ReadToEnd();

            if (inputTxt != "") 
            {

                // Make regular expression to catch anything between {*} this is the tag
                String tagExpression = @"{.*?}";

                // Match all tags
                MatchCollection tags = Regex.Matches(inputTxt, tagExpression);

                // Insert them in the array list
                foreach (Match tag in tags)
                {
                    tagList.Add(tag.Value);
                }// end foreach

                tagList.TrimToSize(); // Final size decided
                tagStrings = (String[])tagList.ToArray(tagList[0].GetType());
                
                try
                {
                    tagList.TrimToSize(); // Final size decided
                    tagStrings = (String[])tagList.ToArray(tagList[0].GetType());
                }
                catch (ArgumentOutOfRangeException)
                {
                    logger.LogError("File " + fileName + " is empty", ErrorCode.FILE_IS_EMPTY);
                }//end try-catch*/
            }// end if(inputTxt != "")
            else
            {
                logger.LogError("File " + fileName + " is empty", ErrorCode.FILE_IS_EMPTY);
                tagStrings = null;
            } // end else

            inputFile.Close();

            // Return the full list of tags
            return tagStrings;

        } // end GetTags()

        // Method to parse a file
        protected override ArrayList FileParse(String mrfFullFileName)
        {
            // Temp array list to accomodate words structs in the files
            ArrayList fileWordsList = new ArrayList();

            // The temp mrf word name of each tag
            String mrfWordName;

            // The temp mrf IDs of each tag
            int[] mrfIDs;

            // The temp POS word name of each tag
            String posWordName;

            // The temp POS IDs of each tag
            int[] posIDs;

            // The tags string of POS file
            String[] posTags;

            // Get tags of mrf file
            String[] mrfTags;

            // Get POS file name
            String posFullFileName = GetPOSFullFileName(mrfFullFileName);

            switch (configManager.outputFeatures)
            {
                case "MrfAndPOS":
                    // Check 1: The POS file exists
                    try
                    {
                        // Get POS file name
                        posFullFileName = GetPOSFullFileName(mrfFullFileName);

                        // Get tags of mrf file
                        mrfTags = GetTags(mrfFullFileName);

                        // Get tags of POS file
                        posTags = GetTags(posFullFileName);

                        if ((mrfTags != null) && (posTags != null))
                        {
                            if (mrfTags.Length != posTags.Length)
                                logger.LogError("Number of tags don't match: POS tags number is " + posTags.Length + " while mrf tags number is " + mrfTags.Length, ErrorCode.NUM_TAGS_DONT_MATCH);
                        } // end if Check 2
                        else
                        {
                            if (mrfTags == null)
                            {
                                logger.LogError("Configuration is MrfAndPOS. Mrf file is empty. Nothing will be added.", ErrorCode.FILE_IS_EMPTY);
                            }
                            else if (posTags == null)
                            {
                                logger.LogError("Configuration is MrfAndPOS. POS file is empty. Nothing will be added.", ErrorCode.FILE_IS_EMPTY);
                            }

                            return fileWordsList;
                        }

                        // Parse tags one by one
                        for (int i = 0; (mrfTags != null) && (i < mrfTags.Length); i++)
                        {
                            // Temp word structure to be built by each tag parse
                            Word tagWord = new Word(); // The tag of each new word should be re-allocated each time, otherwise all the added objects to the list will always point to the latest version of the object.

                            // Parse mrf tag
                            TagParse(mrfTags[i], out mrfWordName, out mrfIDs);

                            // Parse POS tag
                            if (posTags != null)
                            {
                                TagParse(posTags[i], out posWordName, out posIDs);
                                POSTagParse(posTags[i], out posWordName, out posIDs, ref posNames);

                                // Check 3: mrf and POS word names are the same
                                if ((mrfWordName != posWordName) && (posWordName != null) && (mrfWordName != null))
                                {
                                    logger.LogError("Word names don't match: POS word name is " + posWordName + " while mrf word name is " + mrfWordName, ErrorCode.WORDS_NAMES_DONT_MATCH);
                                }// end if Check 3

                                // POS IDs
                                if (posIDs != null)
                                {
                                    POSFillWordIDs(posIDs, ref tagWord);
                                }

                            }// end if
                            else
                            {
                                // Add nothing
                                tagWord = null;
                                continue;
                            }

                            // Fill in word structure

                            // Word name
                            if (mrfWordName != null)
                            {
                                tagWord.wordName = mrfWordName;
                            }// end if
                            else
                            {
                                // Add nothing
                                tagWord = null;
                                continue;
                            }

                            // Mrf IDs
                            if (mrfIDs != null)
                            {
                                MrfFillWordIDs(mrfIDs, ref tagWord);
                            }// end if
                            else
                            {
                                // Add nothing
                                tagWord = null;
                                continue;
                            }

                            // Insert the word in the array list
                            fileWordsList.Add(tagWord);

                            tagWord = null;

                        } // end for: Parse tags one by one


                    }
                    catch (IOException)
                    {
                        logger.LogError("POS file: " + posFullFileName + "doesn't exist in: " + configManager.posFolderName, ErrorCode.PEER_POS_FILE_DOESNT_EXIST);
                    }// end try-catch IOException

                    logger.LogTrace("Words of file: " + mrfFullFileName + " are successufuly parsed");

                    return fileWordsList;
                //break;
                case "MrfOnly":

                    // Get tags of mrf file
                    mrfTags = GetTags(mrfFullFileName);

                    if (mrfTags == null)
                    {
                        logger.LogError("Configuration is MrfOnly. Mrf file empty. Nothing will be added.", ErrorCode.FILE_IS_EMPTY);
                        return fileWordsList;
                    } // end if

                    // Parse tags one by one
                    for (int i = 0; (mrfTags != null) && (i < mrfTags.Length); i++)
                    {
                        // Temp word structure to be built by each tag parse
                        Word tagWord = new Word(); // The tag of each new word should be re-allocated each time, otherwise all the added objects to the list will always point to the latest version of the object.

                        // Parse mrf tag
                        TagParse(mrfTags[i], out mrfWordName, out mrfIDs);

                        // Fill in word structure

                        // Word name
                        if (mrfWordName != null)
                        {
                            tagWord.wordName = mrfWordName;

                        }// end if
                        else
                        {
                            // Add nothing
                            tagWord = null;
                            continue;
                        }

                        // Mrf IDs
                        if (mrfIDs != null)
                        {
                            MrfFillWordIDs(mrfIDs, ref tagWord);
                        }// end if
                        else
                        {
                            // Add nothing
                            tagWord = null;
                            continue;
                        }

                        // Insert the word in the array list
                        fileWordsList.Add(tagWord);

                        tagWord = null;

                    } // end for: Parse tags one by one


                    logger.LogTrace("Words of file: " + mrfFullFileName + " are successufuly parsed");

                    return fileWordsList;
                //break;

                case "POSOnly":
                    // Get POS file name
                    posFullFileName = GetPOSFullFileName(mrfFullFileName);

                    // Get tags of POS file
                    posTags = GetTags(posFullFileName);

                    if (posTags == null)
                    {
                        logger.LogError("Configuration is POSOnly. POS file or POS file is empty. Nothing will be added.", ErrorCode.FILE_IS_EMPTY);
                        return fileWordsList;
                    }

                    // Parse tags one by one
                    for (int i = 0; (posTags != null) && (i < posTags.Length); i++)
                    {
                        // Temp word structure to be built by each tag parse
                        Word tagWord = new Word(); // The tag of each new word should be re-allocated each time, otherwise all the added objects to the list will always point to the latest version of the object.

                        // Parse mrf tag
                        TagParse(posTags[i], out posWordName, out posIDs);
                        POSTagParse(posTags[i], out posWordName, out posIDs, ref posNames);

                        try
                        {
                            int j = 2;
                            while ((int)posWordName.ToCharArray()[posWordName.Length - j] < 1568 ||
                                   (int)posWordName.ToCharArray()[posWordName.Length - j] > 1610)
                            {
                                j--;
                            }

                            //wordsLogFile.WriteLine(posWordName);

                            //wordsLogFile.WriteLine(posWordName.ToCharArray()[posWordName.Length - j]);
                            //wordsLogFile.WriteLine("0x{0:X4}", (int)posWordName.ToCharArray()[posWordName.Length - j]);
                        }
                        catch
                        {
                            int x;
                        }
                        // Fill in word structure

                        // Word name
                        if (posWordName != null)
                        {
                            tagWord.wordName = posWordName;


                        }// end if
                        else
                        {
                            // Add nothing
                            tagWord = null;
                            continue;
                        }

                        // Mrf IDs
                        if (posIDs != null)
                        {
                            POSFillWordIDs(posIDs, ref tagWord);
                        }// end if
                        else
                        {
                            // Add nothing
                            tagWord = null;
                            continue;
                        }

                        // Insert the word in the array list
                        fileWordsList.Add(tagWord);

                        tagWord = null;

                    } // end for: Parse tags one by one
                    logger.LogTrace("Words of file: " + mrfFullFileName + " are successufuly parsed");

                    return fileWordsList;
                //break;
                default:
                    Console.WriteLine("Incorrect features format configuration. {0} is invalid configuration. Valid configurations are: MrfAndPOS, MrfOnly, POSOnly.", configManager.outputFeatures);
                    throw (new IndexOutOfRangeException());
                //break;
            }// end switch

        } // end FileParse()
       
        // Method to parse a tag into wordName and array of ID's
        protected override void TagParse(String tag, out String wordName, out int[] IDs)
        {
            String[] tempPOS = new String[50];
            if (tag != null)
            {
                // Example of Morph tag:
                // {بَعْدَ;(مُصرَّفة منتظِمة)1:()0,(ب ع د)361,(فَعْل)817,()0}

                // Array list to hold all IDs. To be coverted to array after all IDs are parsed.
                ArrayList IDs_List = new ArrayList();

                // The regular expression to catch word name. 
                // The following regex espression takes into consideration is the word is surrounded by () or not--> \(?<...>\)? means 0 or more ( <Word regex) 0 or more ).
                // Also it removes the last ; and the first { by using memory container feature of the Regex in C#
                //String wordExpression = @"\{+\(?(?<wordName>.+?)\)?;"; //@"[^{].+?;"; // some characters followed by ;
                // \{+\s*\{*\(? matches: {
                String wordExpression = @"\{+\s*\{*\(?(?<wordName>.+?)\)?;"; //@"[^{].+?;"; // some characters followed by ;

                // The regular expression of IDs. All of them are numbers
                String ID_expressions = @"[0-9]+";// Any number


                // Match the word name.
                try
                {
                    wordName = Regex.Match(tag, wordExpression).Result("${wordName}");
                }
                catch
                {
                    wordName = null;
                    logger.LogError("Empty word", ErrorCode.WORD_NAME_IS_EMPTY);
                }

                // Match all ID's
                MatchCollection ID_Matches = Regex.Matches(tag, ID_expressions);

                // Insert the matches into ArrayList
                foreach (Match ID in ID_Matches)
                {
                    IDs_List.Add(Int32.Parse(ID.Value));

                }

                // Limit list size
                IDs_List.TrimToSize();

                // Copy List into array
                IDs = (int[])IDs_List.ToArray(IDs_List[0].GetType());
            }// end if(tag != null)
            else
            {
                wordName = null;
                IDs = null;
            }// end else

        } // end TagParse()

        // Method used to parse POS tags sepcifically
        protected override void POSTagParse(String tag, out String wordName, out int[] IDs, ref String[] POS)
        {
            String[] tempPOS = new String[50];
            if (tag != null)
            {
                // Example of Morph tag:
                // {بَعْدَ;(مُصرَّفة منتظِمة)1:()0,(ب ع د)361,(فَعْل)817,()0}

                // Array list to hold all IDs. To be coverted to array after all IDs are parsed.
                ArrayList IDs_List = new ArrayList();

                // The regular expression to catch word name. 
                // The following regex espression takes into consideration is the word is surrounded by () or not--> \(?<...>\)? means 0 or more ( <Word regex) 0 or more ).
                // Also it removes the last ; and the first { by using memory container feature of the Regex in C#
                //String wordExpression = @"\{+\(?(?<wordName>.+?)\)?;"; //@"[^{].+?;"; // some characters followed by ;
                // \{+\s*\{*\(? matches: {
                String wordExpression = @"\{+\s*\{*\(?(?<wordName>.+?)\)?;"; //@"[^{].+?;"; // some characters followed by ;

                // The regular expression of IDs. All of them are numbers
                String ID_expressions = @"[0-9]+";// Any number

                // POS name expression
                //String POSnameExpression = @"\((?<posName>.+?)\)\s+[0-9]+";//@"\(.*?\)";
                //String POSnameExpression = @"\((?<posName>.+?)\)\s[0-9]";
                String POSnameExpression = @"\(.[^;]+?\) [0-9]+";
                //String POSnameExpression = @"\((?<posName>.+?)+?\)\s[0-9]";


                // Match the word name.
                try
                {
                    wordName = Regex.Match(tag, wordExpression).Result("${wordName}");
                }
                catch
                {
                    wordName = null;
                    logger.LogError("Empty word", ErrorCode.WORD_NAME_IS_EMPTY);
                }

                // Match all ID's
                MatchCollection ID_Matches = Regex.Matches(tag, ID_expressions);

                // Insert the matches into ArrayList
                foreach (Match ID in ID_Matches)
                {
                    IDs_List.Add(Int32.Parse(ID.Value));
                }

                try
                {
                    //String s = Regex.Match(tag, POSnameExpression).Result("${posName}");

                    foreach (Match posName in Regex.Matches(tag, POSnameExpression))
                    {
                        String[] parts = posName.Value.Split(" ".ToCharArray());
                        POS[Int32.Parse(parts[1])] = parts[0];
                    }
                }
                catch
                {
                }


                // Limit list size
                IDs_List.TrimToSize();

                // Copy List into array
                IDs = (int[])IDs_List.ToArray(IDs_List[0].GetType());
            }// end if(tag != null)
            else
            {
                wordName = null;
                IDs = null;
            }// end else

            //POS = tempPOS;
        } // end POSTagParse()
        
        // Method to fill in Word structure with Mrf data
        private void MrfFillWordIDs(int[] IDs, ref Word word)
        {
            try
            {
                // First ID is for mrfType
                word.mrfType = IDs[0];

                // Second ID is for prefix
                word.p = IDs[1];

                // Third ID is for root
                word.r = IDs[2];

                // Fourth ID is for form (wazn)
                word.f = IDs[3];

                // Fifth ID is for suffix
                word.s = IDs[4];

                // If this run is Max ID's determination then store them
                if (maxIDRun)
                {
                    // Store max IDs
                    if (word.mrfType > maxIDs.mrfType)
                    {
                        maxIDs.mrfType = word.mrfType;
                    }

                    if (word.p > maxIDs.p)
                    {
                        maxIDs.p = word.p;
                    }

                    if (word.r > maxIDs.r)
                    {
                        maxIDs.r = word.r;
                    }

                    if (word.f > maxIDs.f)
                    {
                        maxIDs.f = word.f;
                    }

                    if (word.s > maxIDs.s)
                    {
                        maxIDs.s = word.s;
                    }
                }// end if

            }
            catch (IndexOutOfRangeException)
            {
                logger.LogError("Arabized word: " + word.wordName, ErrorCode.ARABIZED_WORD);
            }// end try-catch
        } // end MrfFillWordIDs()
        
        // Method to fill in Word structure with POS data
        protected override void POSFillWordIDs(int[] IDs, ref Word word)
        {
            word.POS_IDs = IDs;

            foreach (int ID in word.POS_IDs)
            {
                // If this run is Max ID's determination then store them
                if (maxIDRun)
                {
                    // only maxIDs.POS_IDs[0] holds the maximum value. It contains only 1 element
                    if (ID > maxIDs.POS_IDs[0])
                    {
                        maxIDs.POS_IDs[0] = ID;
                    }// end if
                }// end if
            }// end foreach



            // Fill inthe equivalent ID
            word.equivalentPOS_ID = MapPOSToEquivalentID(IDs);

            // If this run is Max ID's determination then store them
            if (maxIDRun)
            {
                if (word.equivalentPOS_ID > maxIDs.equivalentPOS_ID)
                {
                    maxIDs.equivalentPOS_ID = word.equivalentPOS_ID;
                }// end if

                // Get maximum number of POS fields
                if (word.POS_IDs.Length > maxIDs.POS_IDs[1])
                {
                    maxIDs.POS_IDs[1] = word.POS_IDs.Length;
                }

            }// end if


        } // end POSFillWordIDs

        // Method to obtain pos file full path from mrf file full path
        private String GetPOSFullFileName(String mrfFullFileName)
        {
            // The string to return
            String posFullFileName = "";

            // Split the mrf file path to get parent directory and file name
            String[] parts = mrfFullFileName.Split(configManager.directorySeparator.ToCharArray());

            // Replace mrf folder name by POS one
            parts[parts.Length - 2] = configManager.posFolderName;// @"الأنواع الكلامية";

            // Split the mrf file name to get pure name and extension
            String[] fileParts = parts[parts.Length - 1].Split(configManager.fileExtensionSeparator.ToCharArray());

            // Replace mrf file extension by POS one
            fileParts[fileParts.Length - 1] = configManager.posFileExtension;

            // Assemble file name with .pos extension
            //parts[parts.Length - 1] = AssembleStringParts(fileParts, configManager.fileExtensionSeparator);
            parts[parts.Length - 1] = String.Join(configManager.fileExtensionSeparator, fileParts);


            // Assemble full POS file path
            //posFullFileName = AssembleStringParts(parts, configManager.directorySeparator);
            posFullFileName = String.Join(configManager.directorySeparator, parts);

            return posFullFileName;

        } // end GetPOSFullFileName ()


    }
}
