using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace Preprocessing
{
    class ReadyFeaturesParser : Parser
    {

        // Constructor
        public ReadyFeaturesParser(ConfigurationManager configManager, Logger logger)
              : base(configManager, logger)
        {
            

        }// end constructor

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
                case "All":
                case "MrfAndPOS":
                    // Check 1: The POS file exists
                    try
                    {
                        // Get POS file name
//                        posFullFileName = GetPOSFullFileName(mrfFullFileName);

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

                            // Update the hash table and vocabulary ID
                            UpdateWordVocabularyID(ref tagWord);
                            
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
                case "MrfAndWord":
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

                        // Update the hash table and vocabulary ID
                        UpdateWordVocabularyID(ref tagWord);

                        // Insert the word in the array list
                        fileWordsList.Add(tagWord);

                        tagWord = null;

                    } // end for: Parse tags one by one


                    logger.LogTrace("Words of file: " + mrfFullFileName + " are successufuly parsed");

                    return fileWordsList;
                //break;

                case "POSAndWord":
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

                        // Update the hash table and vocabulary ID
                        UpdateWordVocabularyID(ref tagWord);

                        // Insert the word in the array list
                        fileWordsList.Add(tagWord);

                        tagWord = null;

                    } // end for: Parse tags one by one
                    logger.LogTrace("Words of file: " + mrfFullFileName + " are successufuly parsed");

                    return fileWordsList;
                //break;

                case "WordOnly":
                    /* txt files names are not exactly corresponding to mrf and POS names
                     * It's better to parse also mrf or POS files since we r in ReadyFeatures case
                    // Get the corresponding txt file name
                    String txtFullFileName = GetTxtFullFileName(mrfFullFileName);

                    // Get the words list
                    GetWordsListInRawTxtFile(txtFullFileName, ref fileWordsList);*/


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

                            // Update the hash table and vocabulary ID
                            UpdateWordVocabularyID(ref tagWord);

                            // Insert the word in the array list
                            fileWordsList.Add(tagWord);

                            tagWord = null;

                        }// end if
                        else
                        {
                            // Add nothing
                            tagWord = null;
                            continue;
                        }



                    } // end for: Parse tags one by one


                    logger.LogTrace("Words of file: " + mrfFullFileName + " are successufuly parsed for WordOnly target, no mrf fields are parsed");

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

        // Method to obtain pos file full path from mrf file full path
        private String GetTxtFullFileName(String mrfFullFileName)
        {
            // The string to return
            String txtFullFileName = "";

            // Split the mrf file path to get parent directory and file name
            String[] parts = mrfFullFileName.Split(configManager.directorySeparator.ToCharArray());

            // Replace mrf folder name by POS one
            parts[parts.Length - 2] = configManager.txtFolderName;// @"ملفات النصوص";

            // Split the mrf file name to get pure name and extension
            String[] fileParts = parts[parts.Length - 1].Split(configManager.fileExtensionSeparator.ToCharArray());

            // Replace mrf file extension by POS one
            fileParts[fileParts.Length - 1] = configManager.txtFileExtension;

            // Assemble file name with .pos extension
            //parts[parts.Length - 1] = AssembleStringParts(fileParts, configManager.fileExtensionSeparator);
            parts[parts.Length - 1] = String.Join(configManager.fileExtensionSeparator, fileParts);


            // Assemble full POS file path
            //posFullFileName = AssembleStringParts(parts, configManager.directorySeparator);
            txtFullFileName = String.Join(configManager.directorySeparator, parts);

            return txtFullFileName;

        } // end GetTxtFullFileName ()


    }
}
