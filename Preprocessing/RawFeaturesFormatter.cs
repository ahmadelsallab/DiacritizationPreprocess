using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Preprocessing
{
    class RawFeaturesFormatter : FeaturesFormatter
    {
        int bitfieldLength;
        // Constructor
        public RawFeaturesFormatter(ConfigurationManager configManager, Parser parser, Logger logger, Item[] items)
            : base(configManager, parser, logger, items)
        {

            stringLength = ComputeStringLengthRaw(configManager, parser);

            // mrfType + p + r + f + s = 5, each 1 number
            //stringLength = (5 + parser.maxIDs.POS_IDs[0] + 1);

            // Form the empty feature string
            emptyFeatureString = EmptyFeatureString();

            // Form the empty target string
            emptyTargetString = EmptyTargetString();

            bitfieldLength = ComputeBitfieldLength(configManager, parser);
            
        }

        // Method to put the features in their format
        protected override void FormatMrfFeatures(Item word, ref Feature wordFeatures)
        {
            /*wordFeatures.features = ((double)word.mrfType / (double)parser.maxIDs.mrfType).ToString() + "," +
                                    ((double)word.p / (double)parser.maxIDs.p).ToString() + "," +
                                    ((double)word.r / (double)parser.maxIDs.r).ToString() + "," +
                                    ((double)word.f / (double)parser.maxIDs.f).ToString() + "," +
                                    ((double)word.s / (double)parser.maxIDs.s).ToString() + ",";*/

            if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
            {
                wordFeatures.features += (offset + word.mrfType + 1).ToString() + ",";
                if ((offset + word.mrfType + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + word.mrfType + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                offset += parser.maxIDs.mrfType + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
            {
                wordFeatures.features += (offset + word.p + 1).ToString() + ",";
                if ((offset + word.p + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + word.p + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                offset += parser.maxIDs.p + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
            {
                wordFeatures.features += (offset + word.r + 1).ToString() + ",";
                if ((offset + word.r + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + word.r + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                offset += parser.maxIDs.r + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
            {
                wordFeatures.features += (offset + word.f + 1).ToString() + ",";
                if ((offset + word.f + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + word.f + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                offset += parser.maxIDs.f + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
            {
                wordFeatures.features += (offset + word.s + 1).ToString() + ",";
                if ((offset + word.s + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + word.s + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                offset += parser.maxIDs.s + 1;
                addedFeatures += 1;
            }
        }

        // Override the POS word features format
        protected override void FormatPOSFeatures(Item word, ref Feature wordFeatures)
        {
            // Add POS ID's
            foreach (int ID in word.POS_IDs)
            {
                wordFeatures.features += (offset + ID + 1).ToString() + ",";
                if ((offset + ID + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + ID + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                addedFeatures += 1;
            }

            
            // Now Pad the rest of POS field with zeros
            for (int i = word.POS_IDs.Length + 1; i <= parser.maxIDs.POS_IDs[1]; i++)
            {
                wordFeatures.features += "0,";
            }//end for
            
            // Remove the last ","--> No need the comparison is with Length - 1 already
            //wordFeatures.features = wordFeatures.features.Remove(wordFeatures.features.Length - 1);

            offset += parser.maxIDs.POS_IDs[0] + 1;
        }
        // Method to pad the string if needed
        /*protected override void PadRestOfFeaturesString(ref WordFeatures wordFeatures)
        {
            // Now Pad the rest of stringLength with zeros
            for (int i = addedFeatures + 1; i <= stringLength; i++)
            {
                wordFeatures.features += "0,";
            }//end for
        }*/
        // Method to put the features in their format
        protected override void FormatItemIDFeatures(Item item, ref Feature wordFeatures)
        {
            int ID;
            if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
            {
                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        if (item != null)
                        {
                            ID = offset + item.vocabularyWordID + 1;
                            wordFeatures.features += (ID).ToString() + ",";
                            if (ID > bitfieldLength)
                            {
                                logger.LogError("Raw ID is " + ID + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                            }
                        }
                        else
                        {
                            // Log empty features
                            wordFeatures.features += (0).ToString() + ",";
                        }
                        offset += parser.maxIDs.vocabularyWordID;
                        addedFeatures += 1;
                        break;
                    case "CharacterLevel":
                        // Loop on characters of the word
                        if (item != null)
                        {
                            foreach (char wordChar in item.itemNameWithProperDiacritics)
                            {
                                ID = offset + wordChar % 1568 + 1;
                                // Check if the character falls between 0x0620 = 1568 and 0x0652 = 1618 => the characters of Arabic including DIACS
                                wordFeatures.features += (ID).ToString() + ",";
                                if (ID > bitfieldLength)
                                {
                                    logger.LogError("Raw ID is " + ID + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                                }

                                offset += FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                                addedFeatures += 1;
                            }// end foreach

                            // Now, pad the rest of the word to the max word length
                            for (int i = item.itemNameWithProperDiacritics.Length + 1; i <= parser.maxIDs.wordLength; i++)
                            {
                                wordFeatures.features += "0,";
                                offset += FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            }//end for
                        }
                        else
                        {
                            // Log empty features
                            for (int i = 0; i <= parser.maxIDs.wordLength; i++)
                            {
                                wordFeatures.features += "0,";
                                offset += FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            }//end for
                        }

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;
                }// end switch
            }// end if


        }

        // Method to put the word name feature in its char level encoded format
        protected override String FormatWordFeaturesCharLevel(String wordName)
        {
            String features = String.Empty;
            int ID;

            // Loop on characters of the word
            if (wordName != null)
            {
                foreach (char wordChar in wordName)
                {
                    ID = offset + wordChar % START_OF_ARABIC_CHAR_ASCII_CODE + 1;
                    // Check if the character falls between 0x0620 = 1568 and 0x0652 = 1618 => the characters of Arabic including DIACS
                    features += (ID).ToString() + ",";
                    if (ID > bitfieldLength)
                    {
                        logger.LogError("Raw ID is " + ID + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                    }
                    offset += FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;

                }// end foreach

                // Now, pad the rest of the word to the max word length
                for (int i = wordName.Length + 1; i <= MAX_WORD_LEN; i++)
                {
                    features += "0,";
                    offset += FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                }//end for
            }
            else
            {
                // Log empty features
                for (int i = 0; i < MAX_WORD_LEN; i++)
                {
                    features += "0,";
                    offset += FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                }//end for
            }

            return features;
        }

        // Method to check if the wordFeature string conforms to the expected string length or not
        protected override bool IsConformantStringLen(Feature wordFeature)
        {
            // Split the features string
            String[] features = wordFeature.features.Split(",".ToCharArray());

            return (features.Length - 1 == stringLength) ? true : false;
            /*
            switch (configManager.outputFeatures)
            {
                // -1 to remove the last split bitfieldValue after the last , in the features
                case "MrfAndPOS":
                    return (features.Length - 1 == stringLength) ? true : false;
                //break;
                case "MrfOnly":
                    return (features.Length - 1 == stringLength) ? true : false;
                //break;
                case "POSOnly":
                    return (features.Length - 1 == stringLength) ? true : false;
                //break;
                default:
                    return false;

            }// end switch
             * */
            // TODO: check if features without the mrf part conforms to parser.maxIDs.POS_IDs[0] + 1 or not

        }// end IsConformantStringLen

        
        // Method to format the central context word feature
        protected override String FormatCentralContextFeatures(Item word, String features)
        {
            String centralWordFeatures = features;

            if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                centralWordFeatures += (word.mrfType + 1).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                centralWordFeatures += (word.p + 1).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                centralWordFeatures += (word.r + 1).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                centralWordFeatures += (word.f + 1).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                centralWordFeatures += (word.s + 1).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                centralWordFeatures += (word.vocabularyWordID + 1).ToString() + ",";

                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        centralWordFeatures += (word.vocabularyWordID + 1).ToString() + ",";
                        if (word.vocabularyWordID + 1 > bitfieldLength)
                        {
                            logger.LogError("Raw ID is " + word.vocabularyWordID + 1 + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                        }
  
                        break;
                    case "CharacterLevel":
                        // Loop on characters of the word
                        offset = 0;
                        foreach (char wordChar in word.itemNameWithProperDiacritics)
                        {
                            int ID = offset + wordChar % START_OF_ARABIC_CHAR_ASCII_CODE + 1;
                            // Check if the character falls between 0x0620 = 1568 and 0x0652 = 1618 => the characters of Arabic including DIACS
                            centralWordFeatures += (ID).ToString() + ",";
                            if (ID > bitfieldLength)
                            {
                                logger.LogError("Raw ID is " + ID + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                            }
                            offset += FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                        }// end foreach

                        // Now, pad the rest of the word to the max word length
                        for (int i = word.itemNameWithProperDiacritics.Length + 1; i <= parser.maxIDs.wordLength; i++)
                        {
                            centralWordFeatures += "0,";
                            offset += FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                        }//end for
  

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;
                }// end switch
            }

            if ((String)configManager.addFeaturesToCentralContextWord["CharPosition"] == "Add")
            {
                centralWordFeatures += (word.charPosition + 1).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["WordContext"] == "Add")
            {
                centralWordFeatures += ExtractCentralWordContextFeatures(word);
            }

            return centralWordFeatures;
        }
      

        // Utility to compute the required string length in case of Raw features
        public static int [] ComputeChunksLength(ConfigurationManager configManager, Parser parser)
        {
            ArrayList chunksList = new ArrayList();
            int chunkOffset = 0;
            int stringLength = 0;
            switch (configManager.outputFeatures)
            {
                case "All":
                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    // For POS
                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += parser.maxIDs.POS_IDs[1];

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }


                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += 1;
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength;
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch
                    }

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    break;
                case "POSAndWord":

                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += parser.maxIDs.POS_IDs[1];
                    
                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += 1;
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength;
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch
                    }

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    break;
                case "MrfAndWord":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += 1;
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength;
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch
                    }

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    break;
                case "WordOnly":

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += 1;
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength;
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch
                    }

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    break;

                case "MrfAndPOS":
                    // mrfType + p + r + f + s = 5, each 1 number
                    // POS = 1 number
                    //stringLength = 6;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    // For POS
                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += parser.maxIDs.POS_IDs[1];

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    break;
                case "MrfOnly":
                    // mrfType + p + r + f + s = 5, each 1 number
                    //stringLength = 5;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    break;
                case "POSOnly":

                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += parser.maxIDs.POS_IDs[1];

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    break;

            }// end switch

            // Format of word in case of Type 1:
            // <Before Word 1><Target 1><Before Word 2><Target 2>...<Before Word n><Target 2><Word><Specific Mrf features><Last Char features><After Word 1><Target 1><After Word 2><Target 2>...<After Word n><Target n>

            switch (configManager.contextType)
            {
                case "Type 1":

                    // First add the before context offsets
                    for (int i = 0; i < configManager.contextBeforeLength; i++)
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += stringLength;
                        

                        if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                        {
                            // Add the CRF before target
                            chunksList.Add(chunkOffset);
                            switch (configManager.targetType)
                            {
                                case "SYNT_DIAC":
                                case "FULL_DIAC":
                                case "ClassifySyntDiac":
                                    chunkOffset += 1;
                                    break;
                                case "POS":
                                    chunkOffset += parser.maxIDs.POS_IDs[1] + 1;
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                    break;
                            }// end switch    
                        }// end if(!Suppress("ContextTargets"))
                    }// end for

                    // Add the word itself
                    chunksList.Add(chunkOffset);
                    chunkOffset += stringLength;

                    // Add any specific Mrf features
                    if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["CharPosition"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["WordContext"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += (configManager.centralWordContextBeforeLength + configManager.centralWordContextAfterLength + 1) * MAX_WORD_LEN;
                    }

                    
                    // Add the last character offset
                    if (configManager.lastCharFeaturesDepth != 0)
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += configManager.lastCharFeaturesDepth;                        
                    }

                    // Last add the after context offsets
                    for (int i = 0; i < configManager.contextAfterLength; i++)
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += stringLength;

                        // Add the CRF before target
                        chunksList.Add(chunkOffset);
                        if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                        {
                            switch (configManager.targetType)
                            {
                                case "SYNT_DIAC":
                                case "FULL_DIAC":
                                case "ClassifySyntDiac":
                                    chunkOffset += 1;
                                    break;
                                case "POS":
                                    chunkOffset += parser.maxIDs.POS_IDs[1] + 1;
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                    break;
                            }// end switch    
                        }// end if(!Suppress("ContextTargets"))

                    }// end for

                    break;

                // Format of Type 2:
                // <Concerned word>+<Context word 1><Target 1><Context word 2><Target 2>...<Context word n><Target n>
                case "Type 2":

                    // Concerned word
                    chunksList.Add(chunkOffset);
                    chunkOffset += stringLength;

                    // Context items
                    for (int i = 1; i < configManager.contextLength; i++)
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += stringLength;

                        // Add the CRF target
                        chunksList.Add(chunkOffset);
                        if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                        {
                            switch (configManager.targetType)
                            {
                                case "SYNT_DIAC":
                                case "FULL_DIAC":
                                case "ClassifySyntDiac":
                                    chunkOffset += 1;
                                    break;
                                case "POS":
                                    chunkOffset += parser.maxIDs.POS_IDs[1] + 1;
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                    break;
                            }// end switch    
                        }// end if(!Suppress("ContextTargets"))
                    }//end for
                    break;
            }// end switch

            return (int[])chunksList.ToArray(chunksList[0].GetType());

        }// end ComputeChunksLength

        // Utility to compute the required bitfield length in case of Raw features
        public static int ComputeBitfieldLength(ConfigurationManager configManager, Parser parser)
        {
            int bitfieldLength = 0;
            switch (configManager.outputFeatures)
            {
                case "All":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.s + 1);
                    }


                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (parser.maxIDs.POS_IDs[0] + 1);


                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "POSAndWord":

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (parser.maxIDs.POS_IDs[0] + 1);

                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch
                    break;

                case "MrfAndWord":
                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.s + 1);
                    }



                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "WordOnly":

                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "MrfAndPOS":
                    // mrfType + p + r + f + s = 5, each 1 number
                    // POS = 1 number
                    //stringLength = 6;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.s + 1);
                    }

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (parser.maxIDs.POS_IDs[0] + 1);

                    break;
                case "MrfOnly":
                    // mrfType + p + r + f + s = 5, each 1 number
                    //stringLength = 5;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.s + 1);
                    }

                    break;
                case "POSOnly":

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch

            // Add central word features
            if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.mrfType + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.p + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.r + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.f + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.s + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.vocabularyWordID + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["CharPosition"] == "Add")
            {
                //bitfieldLength += parser.maxIDs.wordLength;
                bitfieldLength += MAX_WORD_LEN;
            }

            if ((String)configManager.addFeaturesToCentralContextWord["WordContext"] == "Add")
            {
                //bitfieldLength += (configManager.centralWordContextBeforeLength + configManager.centralWordContextAfterLength + 1)*parser.maxIDs.wordLength;
                bitfieldLength += (configManager.centralWordContextBeforeLength + configManager.centralWordContextAfterLength + 1) * MAX_WORD_LEN * CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
            }

            return bitfieldLength;

        }// end ComputeBitfieldLength

        // Utility to compute the required bitfield length in case of Raw features
        public static int ComputeBitfieldLengthAndOffset(ConfigurationManager configManager, out int[] offset, Parser parser)
        {
            int bitfieldLength = 0;
            int contextBitfieldLength = 0;
            ArrayList offsetList = new ArrayList();

            int bitfieldStartBoundary = 0;

            switch (configManager.outputFeatures)
            {
                case "All":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.s + 1);
                    }

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (parser.maxIDs.POS_IDs[0] + 1);


                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch
                    
                    break;

                case "POSAndWord":

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (parser.maxIDs.POS_IDs[0] + 1);


                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch
                    break;
                
                case "MrfAndWord":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.s + 1);
                    }

                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "WordOnly":

                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "MrfAndPOS":

                    // mrfType + p + r + f + s = 5, each 1 number
                    // POS = 1 number
                    //stringLength = 6;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.s + 1);
                    }


                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (parser.maxIDs.POS_IDs[0] + 1);

                    break;
                case "MrfOnly":


                    // mrfType + p + r + f + s = 5, each 1 number
                    //stringLength = 5;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (parser.maxIDs.s + 1);
                    }

                    break;
                case "POSOnly":

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch

            // Add central context word features--> No need, will be added next in contextBitfieldLength as separate bitfield domain
            /*if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.mrfType + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.p + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.r + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.f + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.s + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                bitfieldLength += (parser.maxIDs.vocabularyWordID + 1);
            }*/

            // Add context targets to bitfield length--> No need, will be added next in contextBitfieldLength as separate bitfield domain
            /*if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
            {
                switch (configManager.targetType)
                {
                    case "SYNT_DIAC":
                        bitfieldLength += ((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1;
                        break;
                    case "POS":
                        bitfieldLength += parser.maxIDs.POS_IDs[0] + 1;
                        break;
                    default:
                        Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC or POS.", configManager.targetType);
                        break;
                }// end switch    
            }// end if !Suppress("ContextTargets")
            */


            // Format of word in case of Type 1:
            // <Before Word 1><Target 1><Before Word 2><Target 2>...<Before Word n><Target 2><Word><Specific Mrf features><Last Char features><After Word 1><Target 1><After Word 2><Target 2>...<After Word n><Target n>
            // Adjust context length and offset
            switch (configManager.contextType)
            {
                case "Type 1":

                    // Add before and after bitfield lengths
                    //contextBitfieldLength = (configManager.contextBeforeLength + 1 + configManager.contextAfterLength) * bitfieldLength;

                    // Adjust the before boundary
                    for (int i = 0; i < configManager.contextBeforeLength; i++)
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += bitfieldLength;
                        

                        if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                        {
                            // Add the CRF before target
                            offsetList.Add(bitfieldStartBoundary);
                            switch (configManager.targetType)
                            {
                                case "SYNT_DIAC":
                                case "FULL_DIAC":
                                case "ClassifySyntDiac":
                                    bitfieldStartBoundary += ((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1;
                                    break;
                                case "POS":
                                    switch (configManager.targetMode)
                                    {
                                        case "Single":
                                            bitfieldStartBoundary += ((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length - 1;
                                            break;
                                        case "Multiple":
                                            bitfieldStartBoundary += parser.maxIDs.POS_IDs[0] + 1;
                                            break;
                                        default:
                                            Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                            break;
                                    }// end switch (configManager.targetMode)

                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                    break;
                            }// end switch    
                            //offsetList.Add(bitfieldStartBoundary);
                        }// end if(!Suppress("ContextTargets"))

                    }// end for(before)

                    // Insert the word itself boundary
                    offsetList.Add(bitfieldStartBoundary);
                    bitfieldStartBoundary += bitfieldLength;


                    // Add any specific Mrf features
                    if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (parser.maxIDs.mrfType + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (parser.maxIDs.p + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (parser.maxIDs.r + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (parser.maxIDs.f + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (parser.maxIDs.s + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (parser.maxIDs.vocabularyWordID + 1);
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["CharPosition"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        //bitfieldStartBoundary += (parser.maxIDs.wordLength + 1); 
                        bitfieldStartBoundary += MAX_WORD_LEN + 1; 
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["WordContext"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        //bitfieldStartBoundary += (configManager.centralWordContextBeforeLength + configManager.centralWordContextAfterLength + 1) * parser.maxIDs.wordLength;
                        bitfieldStartBoundary += (configManager.centralWordContextBeforeLength + configManager.centralWordContextAfterLength + 1) * MAX_WORD_LEN * CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                    }

                    // Add the last character offset
                    if (configManager.lastCharFeaturesDepth != 0)
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (int)FeaturesFormatter.CHAR_FEATURE_BITFIELD_LEN + 1;                        
                    }

                    // Last add the after bitfield offsets
                    for (int i = 0; i < configManager.contextAfterLength; i++)
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += bitfieldLength;
                        
                        
                        // Add the CRF after target
                        offsetList.Add(bitfieldStartBoundary);
                        if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                        {
                            switch (configManager.targetType)
                            {
                                case "SYNT_DIAC":
                                case "FULL_DIAC":
                                case "ClassifySyntDiac":
                                    bitfieldStartBoundary += ((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1;
                                    break;
                                case "POS":
                                    switch (configManager.targetMode)
                                    {
                                        case "Single":
                                            bitfieldStartBoundary += ((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length - 1;
                                            break;
                                        case "Multiple":
                                            bitfieldStartBoundary += parser.maxIDs.POS_IDs[0] + 1;
                                            break;
                                        default:
                                            Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                            break;
                                    }// end switch (configManager.targetMode)
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                    break;
                            }// end switch    
                            //offsetList.Add(bitfieldStartBoundary);
                        }// end if(!Suppress("ContextTargets"))
                        
                    }// end for

                    //contextBitfieldLength = bitfieldStartBoundary + bitfieldLength;
                    contextBitfieldLength = bitfieldStartBoundary;

                    break;

                // Format of Type 2:
                // <Concerned word>+<Context word 1><Target 1><Context word 2><Target 2>...<Context word n><Target n>
                case "Type 2":
                    // Concerned word
                    offsetList.Add(bitfieldStartBoundary);
                    bitfieldStartBoundary += bitfieldLength;

                    // Context items
                    for (int i = 1; i < configManager.contextLength; i++)
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += bitfieldLength;

                        // Add the CRF target
                        offsetList.Add(bitfieldStartBoundary);
                        if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                        {
                            switch (configManager.targetType)
                            {
                                case "SYNT_DIAC":
                                case "FULL_DIAC":
                                case "ClassifySyntDiac":
                                    bitfieldStartBoundary += ((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1;
                                    break;
                                case "POS":
                                    switch (configManager.targetMode)
                                    {
                                        case "Single":
                                            bitfieldStartBoundary += ((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length - 1;
                                            break;
                                        case "Multiple":
                                            bitfieldStartBoundary += parser.maxIDs.POS_IDs[0] + 1;
                                            break;
                                        default:
                                            Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                            break;
                                    }// end switch (configManager.targetMode)
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                    break;
                            }// end switch    
                        }// end if(!Suppress("ContextTargets"))
                    }//end for
                    break;
            }// end switch

            offset =(int[]) offsetList.ToArray(offsetList[0].GetType());

            return contextBitfieldLength;

        }// end ComputeBitfieldLengthAndOffset

        // Utility to return empty last character feature
        protected override String EmptyLastCharFeatureString()
        {            
            return "0,";
        }// end EmptyFeatureString()

        // Utility to return empty target string
        protected override String EmptyTargetString()
        {
            return "0,";
        }// end EmptyFeatureString()

        // Method to extract the last character(s) features
        protected override String GetLastCharFeatures(String itemName)
        {
            if (itemName != null)
            {
                // Counter of number of depth characters
                int i = 0;

                // Position of the character
                int j = 2;

                // The returned feature string               
                String lastCharFeature = String.Empty;

                //int tempLastCharFeature;

                while (i < configManager.lastCharFeaturesDepth)
                {
                    try
                    {
                        // Check if the character falls between 0x0620 and 0x064A=> the characters of Arabic
                        if ((int)itemName.ToCharArray()[itemName.Length - j] >= 1568 &&
                            (int)itemName.ToCharArray()[itemName.Length - j] <= 1610)
                        {
                            lastCharFeature = lastCharFeature + ((int)itemName.ToCharArray()[itemName.Length - j] % 1568 + 1).ToString() + ",";

                           /* tempLastCharFeature = ((int)FeaturesFormatter.CHAR_FEATURE_BITFIELD_LEN + 1) - ((int)itemName.ToCharArray()[itemName.Length - j] % 1568);
                            lastCharFeature = lastCharFeature + tempLastCharFeature.ToString() + ",";*/
                            i++;
                        }
                        else
                        {
                            // Move to next character before the current one
                            j++;
                        }
                    }
                    catch
                    {
                        lastCharFeature = lastCharFeature + EmptyLastCharFeatureString();
                        break;
                    }

                }//end while

                return lastCharFeature;

            }
            else
            {
                return "";
            }
        }// GetLastCharFeatures

        // Utility to compute the required string length in case of Raw features
        public static int ComputeStringLengthRaw(ConfigurationManager configManager, Parser parser)
        {
            int stringLength = 0;
            switch (configManager.outputFeatures)
            {
                case "All":
                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }


                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    // For POS
                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += parser.maxIDs.POS_IDs[1];

                    break;
                case "POSAndWord":

                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += parser.maxIDs.POS_IDs[1];

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += 1;
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength;
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch
                    }


                    break;
                case "MrfAndWord":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += 1;
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength;
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch
                    }

 

                    break;
                case "WordOnly":

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += 1;
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength;
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch
                    }
                    break;


                case "MrfAndPOS":
                    // mrfType + p + r + f + s = 5, each 1 number
                    // POS = 1 number
                    //stringLength = 6;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    // For POS
                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += parser.maxIDs.POS_IDs[1];


                    break;
                case "MrfOnly":
                    // mrfType + p + r + f + s = 5, each 1 number
                    //stringLength = 5;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }



                    break;
                case "POSOnly":
                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += parser.maxIDs.POS_IDs[1];
                    
                    break;

            }// end switch
            return stringLength;
        }// end FormatTargetStringFeatures()

        // Method to format the targetString
        // The targetString will be fomratted according to its offset position in the string of features
        // BUT, will not be added to wordFeatures string now. It'll be added when formatting the context, where
        // it should be addded to before and after items but not the central one, which will have 0's
        protected override void FormatTargetStringFeatures(ref Feature wordFeatures)
        {
            String targetString = String.Empty;

            if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
            {
                switch (configManager.targetType)
                {
                    case "SYNT_DIAC":
                    case "FULL_DIAC":
                    case "ClassifySyntDiac":
                        // No need to add 1, target in Enums.cs starts from 1 already not 0
                        targetString = (wordFeatures.target[0]).ToString() + ",";
                        break;
                    case "POS":
                        foreach (int target in wordFeatures.target)
                        {
                            targetString += (target + 1).ToString() + ",";
                        }

                        // Now Pad the rest of POS field with zeros
                        for (int i = wordFeatures.target.Length + 1; i <= parser.maxIDs.POS_IDs[1]; i++)
                        {
                            wordFeatures.features += "0,";
                        }//end for
                        
                        break;
                    default:
                        Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                        break;
                }// end switch    
            }// end if !Suppress("ContextTargets")

            wordFeatures.targetString = targetString;
        }// end class

    }// ComputeStringLengthRaw
    
}
