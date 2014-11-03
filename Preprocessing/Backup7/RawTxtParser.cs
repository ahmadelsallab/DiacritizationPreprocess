using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Preprocessing
{
    class RawTxtParser : Parser
    {
        // Hash table of which POS names has been added
        private Hashtable posNamesHashTable = new Hashtable();
        
        // Hash table of POS/Name map
        private Hashtable posNames_IDMapRDI = new Hashtable()
            {
                // RDI Map:
                // Starts from 1, no 0 index
                // 1 is reserved for (NullPrefix)-->Equivalent to any non DT POS
                //
                /*RDI*/
                /*(NullPrefix)*/{"NoDT", 1},
                /*(Conj)*/{"CC", 3},// 	Coordinating conjunction e.g. and,but,or...
                /*NA:(Noun)*/{"CD", 14},// 	Cardinal Number
                /*(Definit)*/{"DT", 7},// 	Determiner
                /*NA: (Prepos)*/{"EX", 5},// 	Existential there
                /*NA:(Noun)*/{"FW", 14},// 	Foreign Word
                /*(Prepos)*/{"IN", 5},// 	Preposision or subordinating conjunction
                /*(RelAdj)*/{"JJ", 47},// 	Adjective
                /*(RelAdj)*/{"ADJ", 47},// 	Adjective
                /*(RelAdj)*/{"JJR", 47},// 	Adjective, comparative
                /*(ExaggAdj)*/{"JJS", 18},// 	Adjective, superlative
                /*NA(Noun)*/{"LS", 14},// 	List Item Marker
                ///*NA*/{"MD", 10},// 	Modal e.g. can, could, might, may...
                /*(Noun)*/{"NN", 14}, // 	Noun, singular or mass
                /*(Noun)*/{"NOUN", 14}, // 	Noun, singular or mass
                /*(Noun)+ (Single)*/{"NNP", 14}, // 	Proper Noun, singular
                /*(Noun)+ (Plural)*/{"NNPS", 14}, // 	Proper Noun, plural
                /*(Noun)+ (Plural)*/{"NNS", 14}, // 	Noun, plural
                /*NA:(Noun)*/{"PDT", 15}, // 	Predeterminer e.g. all, both ... when they precede an article
                /*NA:(Noun)*/{"POS", 14}, // 	Possessive Ending e.g. Nouns ending in 's
                /*(SubjPro)*/{"PRP", 58}, // 	Personal Pronoun e.g. I, me, you, he...
                /*(PossessPro)*/{"PRP$", 49}, // 	Possessive Pronoun e.g. my, your, mine, yours...
                /*NA:(Noun)*/{"RB", 14},// 	Adverb Most words that end in -ly as well as degree words like quite, too and very
                /*NA:(Noun)*/{"RBR", 14},// 	Adverb, comparative Adverbs with the comparative ending -er, with a strictly comparative meaning.
                /*NA:(Noun)*/{"RBS", 14},// 	Adverb, superlative
                /*NA:(Noun)*/{"RP", 14}, //	Particle
                /*NA:(Noun)*/{"SYM", 14}, //	Symbol Should be used for mathematical, scientific or technical symbols
                /*NA:(Conj)*/{"TO", 3},// 	to
                /*(Confirm)*/{"UH", 4},// 	Interjection e.g. uh, well, yes, my...
                /*(Verb)*/{"VB", 37},// 	Verb, base form subsumes imperatives, infinitives and subjunctives
                /*(Past)*/{"VBD", 39},// 	Verb, past tense includes the conditional form of the verb to be
                /*(Present)*/{"VBG", 10},// 	Verb, gerund or persent participle
                /*(Past)*/{"VBN", 39},// 	Verb, past participle
                /*(Verb)*/{"VBP", 37},// 	Verb, non-3rd person singular present
                /*(Verb)*/{"VBZ", 37},// 	Verb, 3rd person singular present
                /*RelPro*/{"WDT", 23},// 	Wh-determiner e.g. which, and that when it is used as a relative pronoun
                /*(RelPro)*/{"WP", 23},// 	Wh-pronoun e.g. what, who, whom...
                /*(PossessPro)*/{"WP$", 49},// 	Possessive wh-pronoun
                /*(Interrog)*/{"WRB", 2},// 	Wh-adverbe.g. how, where why
                /*NA:(Noun)*/{"VN", 14},// 	??--> In RDI, this kind of words are parsed as Noun
                /*(Plural)*/{"Plural", 43},
                /*(Single)*/{"Single", 51},
                /*(NoSyntaEffect)*/{"PUNC", 32}, // 	Puncituation is not in RDI, so put as NoSyntaEffect
            };
        
        private Hashtable posNames_IDMapOriginal = new Hashtable()
            {
                {"CC", 1},// 	Coordinating conjunction e.g. and,but,or...
                {"CD", 2},// 	Cardinal Number
                {"DT", 3},// 	Determiner
                {"EX", 4},// 	Existential there
                {"FW", 5},// 	Foreign Word
                {"IN", 6},// 	Preposision or subordinating conjunction
                {"JJ", 7},// 	Adjective
                {"JJR", 8},// 	Adjective, comparative
                {"JJS", 9},// 	Adjective, superlative
                {"LS", 10},// 	List Item Marker
                {"MD", 11},// 	Modal e.g. can, could, might, may...
                {"NN", 12}, // 	Noun, singular or mass
                {"NNP", 13}, // 	Proper Noun, singular
                {"NNPS", 14}, // 	Proper Noun, plural
                {"NNS", 15}, // 	Noun, plural
                {"PDT", 16}, // 	Predeterminer e.g. all, both ... when they precede an article
                {"POS", 17}, // 	Possessive Ending e.g. Nouns ending in 's
                {"PRP", 18}, // 	Personal Pronoun e.g. I, me, you, he...
                {"PRP$", 19}, // 	Possessive Pronoun e.g. my, your, mine, yours...
                {"RB", 20},// 	Adverb Most words that end in -ly as well as degree words like quite, too and very
                {"RBR", 21},// 	Adverb, comparative Adverbs with the comparative ending -er, with a strictly comparative meaning.
                {"RBS", 22},// 	Adverb, superlative
                {"RP", 23}, //	Particle
                {"SYM", 24}, //	Symbol Should be used for mathematical, scientific or technical symbols
                {"TO", 25},// 	to
                {"UH", 26},// 	Interjection e.g. uh, well, yes, my...
                {"VB", 27},// 	Verb, base form subsumes imperatives, infinitives and subjunctives
                {"VBD", 28},// 	Verb, past tense includes the conditional form of the verb to be
                {"VBG", 29},// 	Verb, gerund or persent participle
                {"VBN", 30},// 	Verb, past participle
                {"VBP", 31},// 	Verb, non-3rd person singular present
                {"VBZ", 32},// 	Verb, 3rd person singular present
                {"WDT", 33},// 	Wh-determiner e.g. which, and that when it is used as a relative pronoun
                {"WP", 34},// 	Wh-pronoun e.g. what, who, whom...
                {"WP$", 35},// 	Possessive wh-pronoun
                {"WRB", 36},// 	Wh-adverbe.g. how, where why
                {"NOUN", 12},// 	Wh-adverbe.g. how, where why
                {"ADJ", 37},// 	Wh-adverbe.g. how, where why
                {"VN", 38},// 	??--> In RDI, this kind of words are parsed as Noun
                {"PUNC", 39}, // 	Puncituation is not in RDI, so put as NoSyntaEffect
            };

        // Constants

        // Number of POS names
        private const int STANFORD_NUM_POS_NAMES= 72;// To be changed when correct number is obtained

        // Order of word name in the tag
        private const int INDEX_WORD_IN_TAG = 0;

        // Order of POS name in the tag
        private const int INDEX_POS_IN_TAG = 1;

        // Maximum number of POS IDs for 1 word
        // It's constant and can be known from the TagParse() function. Normally the tagger puts 1 tag + 1 for pre-fix (DT or NoDT) + 1 for Single or plural (optional) = 3 max.
        // Currently it's set to 1 not 3, dropping the DT and single/plural info
        private const int MAX_NUM_POS_IDS_PER_WORD = 1;

        // Maximum number of POS IDs for 1 word, COMPLIANT TO RDI
        private const int RDI_MAX_NUM_POS_IDS_PER_WORD = 11;

        // Default POS Name
        private const String DEFAULT_POS_NAME = "NN";

        // RDI Bit-field length
        private const int RDI_BITFIELD_LEN = 61;

        // Standford Bit-field length
        //private const int STANFORD_BITFIELD_LEN = 36;


        // Constructor
        public RawTxtParser(ConfigurationManager configManager, Logger logger)
              : base(configManager, logger)
        {
            this.configManager = configManager;
            this.logger = logger;
            
            posNames = new String[STANFORD_NUM_POS_NAMES];
            

            if (configManager.mapStanfordToRDI)
            {
                Parser.maxIDs.POS_IDs[0] = RDI_BITFIELD_LEN;
            }
            else
            {
                Parser.maxIDs.POS_IDs[0] = posNames_IDMapOriginal.Count;
                
            }

            if (configManager.mapStanfordToRDI)
            {
                // For RDI, we must be compliant to the length of RDI string
                Parser.maxIDs.POS_IDs[1] = RDI_MAX_NUM_POS_IDS_PER_WORD;
            }
            else
            {
                // The max. number of POS IDs for one word for Stanford. It's constant and can be known from the TagParse() function. Normally the tagger puts 1 tag + 1 for pre-fix (DT or NoDT) + 1 for Single or plural (optional) = 3 max.
                Parser.maxIDs.POS_IDs[1] = MAX_NUM_POS_IDS_PER_WORD;
            }


        }// end constructor


        
        // Method to parse a file
        protected override ArrayList FileParse(String fullFileName)
        {
            // Temp array list to accomodate words structs in the files
            ArrayList fileWordsList = new ArrayList();

            // The temp mrf word name of each tag
            String wordName;

            // The temp POS IDs of each tag. Only posIDs[0] is filled for Stanford format
            int [] posIDs;

            // The tags string of POS file
            String[] posTags;

            switch (configManager.outputFeatures)
            {
                case "All":// "All" is added only to support the maxID parse run

                    // Get the words list
                    GetWordsListInRawTxtFile(fullFileName, ref fileWordsList);
                    
                    return fileWordsList;

                    //break;

                case "POSAndWord":
                case "POSOnly":

                    // Obtain the POS tags using Stanford Tagger

                    // Read the whole txt file as string
                    // Open the input file. BE careful with the encoding
                    StreamReader inputFile = new StreamReader(File.Open(fullFileName,
                                                              FileMode.OpenOrCreate, FileAccess.Read),
                                                              Encoding.GetEncoding(1256)); // Encoding is windows-1256 like in Notepad ++: Encoding-->Character Sets --> Arabic--> Windows-1256

                    logger.LogTrace("Reading txt file: " + fullFileName);

                    // Read input file in one string
                    String inputTxt = inputFile.ReadToEnd();

                    inputFile.Close();

                    // Remove diacritics if needed
                    if (configManager.stanfordRemoveDiacritics)
                    {
                        // Remove the diacs
                        String noDiacTxt = RemoveDiacritics(inputTxt);

                        // Write the resulted file
                        StreamWriter fileWithoutDiacs = new StreamWriter(File.Open(configManager.stanfordFileWithoutDiacs,
                                                                          FileMode.OpenOrCreate,
                                                                          FileAccess.Write),
                                                                Encoding.UTF8);

                        fileWithoutDiacs.AutoFlush = true;

                        fileWithoutDiacs.Write(noDiacTxt);

                        fileWithoutDiacs.Close();

                        // Run the Stanford Tagger
                        RunStanfordTagger(configManager.stanfordFileWithoutDiacs);

                    }
                    else
                    {
                        // Run the Stanford Tagger on the input txt directly
                        RunStanfordTagger(fullFileName);

                    }

                    // Get tags of POS file
                    posTags = GetTags(configManager.stanfordTaggerOutFileName);

                    if (posTags == null)
                    {
                        logger.LogError("POS file or POS file is empty. Nothing will be added.", ErrorCode.FILE_IS_EMPTY);
                        return fileWordsList;
                    }

                    // Parse tags one by one
                    for (int i = 0; (posTags != null) && (i < posTags.Length); i++)
                    {
                        // Temp word structure to be built by each tag parse
                        Word tagWord = new Word(); // The tag of each new word should be re-allocated each time, otherwise all the added objects to the list will always point to the latest version of the object.

                        // Parse the tag
                        TagParse(posTags[i], out wordName, out posIDs);
                        if (posNames != null)
                        {
                            POSTagParseStanford(posTags[i], wordName, posIDs, ref posNames);
                        }

                        // Fill in word structure

                        // Word name
                        if (wordName != null)
                        {
                            tagWord.wordName = wordName;


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

                        // Add the word only if the wordName is not ""
                        if (tagWord.wordName != "")
                        {
                            // Insert the word in the array list
                            fileWordsList.Add(tagWord);
                        }

                        tagWord = null;

                    } // end for: Parse tags one by one
                    logger.LogTrace("Words of file: " + fullFileName + " are successufuly parsed");

                    return fileWordsList;
                //break;
                case "WordOnly":

                    // Get the words list
                    GetWordsListInRawTxtFile(fullFileName, ref fileWordsList);

                    return fileWordsList;
                    //break;

                default:
                    Console.WriteLine("Incorrect features format configuration. {0} is invalid configuration. Valid configurations are: MrfAndPOS, MrfOnly, POSOnly.", configManager.outputFeatures);
                    throw (new IndexOutOfRangeException());
                //break;
            }// end switch

        } // end FileParse()

        protected override String[] GetTags(String fileName)
        {

            // The array to be built and returned
            String[] tagStrings = null;

            // Open the iput file. BE careful with the encoding
            StreamReader inputFile = new StreamReader(File.Open(fileName,
                                                      FileMode.OpenOrCreate, FileAccess.Read),
                                                      Encoding.GetEncoding(1256)); // Encoding is windows-1256 like in Notepad ++: Encoding-->Character Sets --> Arabic--> Windows-1256
            logger.LogTrace("Getting tags of: " + fileName);

            // Read input file in one string
            String inputTxt = inputFile.ReadToEnd();
            
            // Tags are separated by spaces
            // الطيب/DTNNP صالح/NNP نال/VBD جائزتين/NNS طوال/NN مسيرته/NN الإبداعية/DTNN الجزيرة/DTNN نت/NNP عبد/NNP العزيز/DTNNP بركة/NN ساكن/NN ستطل/NN علينا/NN الذكرى/DTNN 
            // Tag = الطيب/DTNNP and صالح/NNP,...etc

            if (inputTxt != "")
            {
                tagStrings = inputTxt.Split(" ".ToCharArray());
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

        // Method to remove diacritics
        public static String RemoveDiacritics(String inputTxt)
        {
            String stringWithoutDiacritics = String.Empty;
            //char [] wordCharacters = wordName.ToCharArray();

            // Loop on all characters of the given word
            foreach (char wordCharacter in inputTxt)
            {
                // Check if the character doesn't fall between 0x064B and 0x0652=> the diac sign, then log it
                if ((int)wordCharacter < 1611 || (int)wordCharacter > 1618)
                {
                    stringWithoutDiacritics = stringWithoutDiacritics + wordCharacter;
                }// end if
            }// end foreach

            return stringWithoutDiacritics;
        }// end RemoveDiacritics


        // Method to run the Stanford Tagger
        private void RunStanfordTagger(String fileName)
        {
            File.Copy(fileName, "temp.txt", true);
            //String s = "java" + @"-cp ""stanford-postagger.jar;"" StafordPOSTaggerModified " + fileName + " " + configManager.stanfordTaggerModelName + " " + configManager.stanfordTaggerOutFileName;

            // WaitForExit(): This is mandatory so that the .POS file is released before next file is processed
            //Process.Start("java", @"-cp ""stanford-postagger.jar;"" StafordPOSTaggerModified temp.txt " + configManager.stanfordTaggerModelName + " " + configManager.stanfordTaggerOutFileName);
            Process.Start("java", @"-cp ""stanford-postagger.jar;"" StafordPOSTaggerModified temp.txt " + configManager.stanfordTaggerModelName + " " + configManager.stanfordTaggerOutFileName).WaitForExit();
            /*Process tagger = new Process();
            tagger.StartInfo.FileName = "java";
            tagger.StartInfo.Arguments = @"-cp ""stanford-postagger.jar;"" StafordPOSTaggerModified temp.txt " + configManager.stanfordTaggerModelName + " " + configManager.stanfordTaggerOutFileName;
            tagger.Start();
            tagger.WaitForExit();// This is mandatory so that the .POS file is released before next file is processed*/
            File.Delete("temp.txt");
        }// end RunStanfordTagger

        // Method used to parse POS tags sepcifically
        private void POSTagParseStanford(String tag, String wordName, int[] IDs, ref String[] POS)
        {

            // Tag example:
            // الطيب/DTNNP صالح/NNP 
            String[] parts = tag.Split("/".ToCharArray());

            // Check if the tag parsed contains exactly 2 elements: word and tag
            if (parts.Length == 2)
            {
                // Obtain the word name
                wordName = parts[INDEX_WORD_IN_TAG];

                // Obtain the POS name
                String POSName = parts[INDEX_POS_IN_TAG];

                // Check if it was already added, else add it
                if (posNamesHashTable[POSName] != "Exists")
                {
                    POS[posNamesHashTable.Count] = POSName;
                    posNamesHashTable.Add(POSName, "Exists");                   
                }

                //IDs = tempIDs;
            }// end if not empty tag                      

        } // end POSTagParse()

        // Method to parse a tag into wordName and array of ID's
        protected override void TagParse(String tag, out String wordName, out int[] IDs)
        {
            ArrayList tempIDs = new ArrayList();

            // Tag example:
            // الطيب/DTNNP صالح/NNP 
            String[] parts = tag.Split("/".ToCharArray());

            // Check if the tag parsed contains exactly 2 elements: word and tag
            if (parts.Length == 2 && parts[INDEX_WORD_IN_TAG] != "" && parts[INDEX_POS_IN_TAG] != "")
            {
                // Obtain the word name
                wordName = parts[INDEX_WORD_IN_TAG];

                // Obtain the POS name
                String POSName = parts[INDEX_POS_IN_TAG];

                // Handle special POS for RDI
                if (configManager.mapStanfordToRDI)
                {
                    // Check if it contains DT + Other POS
                    if (POSName.Substring(0, 2) == "DT")
                    {
                        try
                        {
                            /*Add the DT*/
                            tempIDs.Add(GetPOSNameID("DT"));

                            /*Add the rest*/
                            tempIDs.Add(GetPOSNameID(POSName.Substring(2)));
                            // Check if plural
                            if (POSName.Substring(2) == "NNPS" || POSName.Substring(2) == "NNS")
                            {
                                tempIDs.Add(GetPOSNameID("Plural"));
                            }
                            else if (POSName.Substring(2) == "NNP")
                            {
                                tempIDs.Add(GetPOSNameID("Single"));
                            }
                        }
                        catch
                        {
                            // Put default POS
                            tempIDs.Add(GetPOSNameID(DEFAULT_POS_NAME));
                        }
                    }// end if "DT"
                    else
                    {
                        tempIDs.Add(GetPOSNameID("NoDT"));// Equivalent to NullPrefix in RDI

                        // Get POS Name ID
                        tempIDs.Add(GetPOSNameID(POSName));

                        // Check if plural
                        if (POSName == "NNPS" || POSName == "NNS")
                        {
                            tempIDs.Add(GetPOSNameID("Plural"));
                        }
                        else if (POSName == "NNP")
                        {
                            tempIDs.Add(GetPOSNameID("Single"));
                        }
                    }// end else "DT"
                }// end if RDI mapping
                else
                {
                    // Check if it contains DT + Other POS
                    if (POSName.Substring(0, 2) == "DT")
                    {
                        try
                        {
                            /*Add the rest*/
                            tempIDs.Add(GetPOSNameID(POSName.Substring(2)));
                        }
                        catch
                        {
                            // Put default POS
                            tempIDs.Add(GetPOSNameID(DEFAULT_POS_NAME));
                        }
                    }// end if "DT"
                    else
                    {

                        // Get POS Name ID
                        tempIDs.Add(GetPOSNameID(POSName));

                    }// end else "DT"
                }
                IDs = (int[])tempIDs.ToArray(tempIDs[0].GetType());

            }// end if not empty tag
            else
            {
                IDs = null;
                wordName = null;
            }


        }// end TagParse

        // Method to get the ID of POS Name
        private int GetPOSNameID(String POSName)
        {
            if (configManager.mapStanfordToRDI)
            {
                try
                {
                    return (int)posNames_IDMapRDI[POSName];
                }
                catch
                {
                    logger.LogError("No RDI equivalent for " + POSName, ErrorCode.NO_RDI_EQUIVALENT);
                    return -1;
                }
            }
            else
            {
                return (int)posNames_IDMapOriginal[POSName];
            }


        }// end GetPOSNameID

        // Method to fill in Word structure with POS data
        protected override void POSFillWordIDs(int[] IDs, ref Word word)
        {
            word.POS_IDs = IDs;

            // Fill inthe equivalent ID
            word.equivalentPOS_ID = MapPOSToEquivalentID(IDs);

        } // end POSFillWordIDs

        // Method used to parse POS tags sepcifically--> NOT USED IN StanfordFormatParser
        protected override void POSTagParse(String tag, out String wordName, out int[] IDs, ref String[] POS)
        {
            IDs = null;
            POS = null;
            wordName = null;
        }

        // Method to parse raw words of txt file
        private void GetWordsListInRawTxtFile(String fullFileName, ref ArrayList fileWordsList)
        {
            // Open the iput file. BE careful with the encoding
            StreamReader inputFile = new StreamReader(File.Open(fullFileName,
                                                      FileMode.OpenOrCreate, FileAccess.Read),
                                                      Encoding.GetEncoding(1256)); // Encoding is windows-1256 like in Notepad ++: Encoding-->Character Sets --> Arabic--> Windows-1256

            logger.LogTrace("Getting words of: " + fullFileName);

            // Read input file in one string
            String inputTxt = inputFile.ReadToEnd();

            inputFile.Close();

            // File words
            String[] words;

            if (inputTxt != "")
            {

                //words = Regex.Split(inputTxt, @"[()،\.-\s]+");
                words = Regex.Split(inputTxt, @"[-\s\(\)\\\.\،\*\;\:\{\}""]+");

                foreach (String word in words)
                {
                    // Form new record for the new word
                    Word newWord = new Word();

                    // Set the word name
                    newWord.wordName = word;

                    // Update the hash table and vocabulary ID
                    UpdateWordVocabularyID(ref newWord);

                    // Add the word only if the wordName is not ""
                    if (newWord.wordName != "")
                    {
                        // Insert the word in the array list
                        fileWordsList.Add(newWord);
                    }
                }
            }
            else
            {
                logger.LogError("File " + fullFileName + " is empty", ErrorCode.FILE_IS_EMPTY);
            }
        }
    }
}
