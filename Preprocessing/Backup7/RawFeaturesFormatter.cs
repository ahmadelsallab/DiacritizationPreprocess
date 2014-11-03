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
        public RawFeaturesFormatter(ConfigurationManager configManager, Logger logger, Word[] words)
            : base(configManager, logger, words)
        {

            stringLength = ComputeStringLengthRaw(configManager);

            // mrfType + p + r + f + s = 5, each 1 number
            //stringLength = (5 + Parser.maxIDs.POS_IDs[0] + 1);

            // Form the empty feature string
            emptyFeatureString = EmptyFeatureString();

            // Form the empty target string
            emptyTargetString = EmptyTargetString();

            bitfieldLength = ComputeBitfieldLength(configManager);
            
        }

        // Method to put the features in their format
        protected override void FormatMrfWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            /*wordFeatures.features = ((double)word.mrfType / (double)Parser.maxIDs.mrfType).ToString() + "," +
                                    ((double)word.p / (double)Parser.maxIDs.p).ToString() + "," +
                                    ((double)word.r / (double)Parser.maxIDs.r).ToString() + "," +
                                    ((double)word.f / (double)Parser.maxIDs.f).ToString() + "," +
                                    ((double)word.s / (double)Parser.maxIDs.s).ToString() + ",";*/

            if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
            {
                wordFeatures.features = (offset + word.mrfType + 1).ToString() + ",";
                if ((offset + word.mrfType + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + word.mrfType + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                offset += Parser.maxIDs.mrfType + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
            {
                wordFeatures.features += (offset + word.p + 1).ToString() + ",";
                if ((offset + word.p + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + word.p + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                offset += Parser.maxIDs.p + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
            {
                wordFeatures.features += (offset + word.r + 1).ToString() + ",";
                if ((offset + word.r + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + word.r + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                offset += Parser.maxIDs.r + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
            {
                wordFeatures.features += (offset + word.f + 1).ToString() + ",";
                if ((offset + word.f + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + word.f + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                offset += Parser.maxIDs.f + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
            {
                wordFeatures.features += (offset + word.s + 1).ToString() + ",";
                if ((offset + word.s + 1) > bitfieldLength)
                {
                    logger.LogError("Raw ID is " + (offset + word.s + 1) + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                }
                offset += Parser.maxIDs.s + 1;
                addedFeatures += 1;
            }
        }

        // Override the POS word features format
        protected override void FormatPOSWordFeatures(Word word, ref WordFeatures wordFeatures)
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
            for (int i = word.POS_IDs.Length + 1; i <= Parser.maxIDs.POS_IDs[1]; i++)
            {
                wordFeatures.features += "0,";
            }//end for
            
            // Remove the last ","--> No need the comparison is with Length - 1 already
            //wordFeatures.features = wordFeatures.features.Remove(wordFeatures.features.Length - 1);

            offset += Parser.maxIDs.POS_IDs[0] + 1;
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
        protected override void FormatWordIDWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            /*if ((offset + word.vocabularyWordID) > 144533)
            {
                //int x = 0;
                //x++;
                logger.LogError("Raw ID is " + word.vocabularyWordID + " while bitfield length is " + Parser.maxIDs.vocabularyWordID, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN); 
            }*/
            int ID;
            if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
            {
                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        ID = offset + word.vocabularyWordID + 1; 
                        wordFeatures.features += (ID).ToString() + ",";
                        if (ID > bitfieldLength)
                        {
                            logger.LogError("Raw ID is " + ID + " while bitfield length is " + bitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
                        }
                        offset += Parser.maxIDs.vocabularyWordID;
                        addedFeatures += 1;
                        break;
                    case "CharacterLevel":
                        // Loop on characters of the word
                        foreach (char wordChar in word.wordNameWithProperDiacritics)
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
                        for (int i = word.wordNameWithProperDiacritics.Length + 1; i <= Parser.maxIDs.wordLength; i++)
                        {
                            wordFeatures.features += "0,";
                            offset += FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                        }//end for

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;
                }// end switch
            }// end if


        }

        // Method to check if the wordFeature string conforms to the expected string length or not
        protected override bool IsConformantStringLen(WordFeatures wordFeature)
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
            // TODO: check if features without the mrf part conforms to Parser.maxIDs.POS_IDs[0] + 1 or not

        }// end IsConformantStringLen

        
        // Method to format the central context word feature
        protected override String FormatCentralContextWordFeatures(Word word, String features)
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
            }


            return centralWordFeatures;
        }
      

        // Utility to compute the required string length in case of Raw features
        public static int [] ComputeChunksLength(ConfigurationManager configManager)
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
                    stringLength += Parser.maxIDs.POS_IDs[1];

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "DIAC":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += Parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
                                stringLength += Parser.maxIDs.wordLength;
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
                            case "DIAC":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += Parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    break;
                case "POSAndWord":

                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += Parser.maxIDs.POS_IDs[1];
                    
                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += 1;
                                break;
                            case "CharacterLevel":
                                stringLength += Parser.maxIDs.wordLength;
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
                            case "DIAC":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += Parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
                                stringLength += Parser.maxIDs.wordLength;
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
                            case "DIAC":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += Parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
                                stringLength += Parser.maxIDs.wordLength;
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
                            case "DIAC":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += Parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
                    stringLength += Parser.maxIDs.POS_IDs[1];

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "DIAC":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += Parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
                            case "DIAC":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += Parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    break;
                case "POSOnly":

                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += Parser.maxIDs.POS_IDs[1];

                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {

                        switch (configManager.targetType)
                        {
                            case "DIAC":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += Parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
                        
                        // Add the CRF before target
                        chunksList.Add(chunkOffset);
                        if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                        {
                            switch (configManager.targetType)
                            {
                                case "DIAC":
                                    chunkOffset += 1;
                                    break;
                                case "POS":
                                    chunkOffset += Parser.maxIDs.POS_IDs[1] + 1;
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
                                case "DIAC":
                                    chunkOffset += 1;
                                    break;
                                case "POS":
                                    chunkOffset += Parser.maxIDs.POS_IDs[1] + 1;
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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

                    // Context words
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
                                case "DIAC":
                                    chunkOffset += 1;
                                    break;
                                case "POS":
                                    chunkOffset += Parser.maxIDs.POS_IDs[1] + 1;
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
                                    break;
                            }// end switch    
                        }// end if(!Suppress("ContextTargets"))
                    }//end for
                    break;
            }// end switch

            return (int[])chunksList.ToArray(chunksList[0].GetType());

        }// end ComputeChunksLength

        // Utility to compute the required bitfield length in case of Raw features
        public static int ComputeBitfieldLength(ConfigurationManager configManager)
        {
            int bitfieldLength = 0;
            switch (configManager.outputFeatures)
            {
                case "All":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.s + 1);
                    }


                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (Parser.maxIDs.POS_IDs[0] + 1);


                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += Parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "POSAndWord":

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += Parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch
                    break;

                case "MrfAndWord":
                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.s + 1);
                    }



                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += Parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
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
                            bitfieldLength += Parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
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
                        bitfieldLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.s + 1);
                    }

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    break;
                case "MrfOnly":
                    // mrfType + p + r + f + s = 5, each 1 number
                    //stringLength = 5;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.s + 1);
                    }

                    break;
                case "POSOnly":

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (Parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch

            // Add central word features
            if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.mrfType + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.p + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.r + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.f + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.s + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.vocabularyWordID + 1);
            }

            return bitfieldLength;

        }// end ComputeBitfieldLength

        // Utility to compute the required bitfield length in case of Raw features
        public static int ComputeBitfieldLengthAndOffset(ConfigurationManager configManager, out int[] offset)
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
                        bitfieldLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.s + 1);
                    }

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (Parser.maxIDs.POS_IDs[0] + 1);


                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += Parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch
                    
                    break;

                case "POSAndWord":

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (Parser.maxIDs.POS_IDs[0] + 1);


                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += Parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch
                    break;
                
                case "MrfAndWord":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.s + 1);
                    }

                    // Add the bitfield length of word ID
                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            bitfieldLength += Parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
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
                            bitfieldLength += Parser.maxIDs.vocabularyWordID + 1;
                            break;
                        case "CharacterLevel":
                            bitfieldLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
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
                        bitfieldLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.s + 1);
                    }


                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    break;
                case "MrfOnly":


                    // mrfType + p + r + f + s = 5, each 1 number
                    //stringLength = 5;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.s + 1);
                    }

                    break;
                case "POSOnly":

                    // The Matlab bitfield is related to the maximum ID bitfieldValue not to the max. number of IDs
                    bitfieldLength += (Parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch

            // Add central context word features--> No need, will be added next in contextBitfieldLength as separate bitfield domain
            /*if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.mrfType + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.p + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.r + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.f + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.s + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                bitfieldLength += (Parser.maxIDs.vocabularyWordID + 1);
            }*/

            // Add context targets to bitfield length--> No need, will be added next in contextBitfieldLength as separate bitfield domain
            /*if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
            {
                switch (configManager.targetType)
                {
                    case "DIAC":
                        bitfieldLength += ((TargetCode[])Enum.GetValues(typeof(TargetCode))).Length - 1;
                        break;
                    case "POS":
                        bitfieldLength += Parser.maxIDs.POS_IDs[0] + 1;
                        break;
                    default:
                        Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
                        
                        // Add the CRF before target
                        offsetList.Add(bitfieldStartBoundary);
                        if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                        {
                            switch (configManager.targetType)
                            {
                                case "DIAC":
                                    bitfieldStartBoundary += ((TargetCode[])Enum.GetValues(typeof(TargetCode))).Length - 1;
                                    break;
                                case "POS":
                                    bitfieldStartBoundary += Parser.maxIDs.POS_IDs[0] + 1;
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
                        bitfieldStartBoundary += (Parser.maxIDs.mrfType + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (Parser.maxIDs.p + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (Parser.maxIDs.r + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (Parser.maxIDs.f + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (Parser.maxIDs.s + 1);                        
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
                                case "DIAC":
                                    bitfieldStartBoundary += ((TargetCode[])Enum.GetValues(typeof(TargetCode))).Length - 1;
                                    break;
                                case "POS":
                                    bitfieldStartBoundary += Parser.maxIDs.POS_IDs[0] + 1;
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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

                    // Context words
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
                                case "DIAC":
                                    bitfieldStartBoundary += ((TargetCode[])Enum.GetValues(typeof(TargetCode))).Length - 1;
                                    break;
                                case "POS":
                                    bitfieldStartBoundary += Parser.maxIDs.POS_IDs[0] + 1;
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
        protected String EmptyLastCharFeatureString()
        {            
            return "0,";
        }// end EmptyFeatureString()

        // Utility to return empty target string
        protected override String EmptyTargetString()
        {
            return "0,";
        }// end EmptyFeatureString()

        // Method to extract the last character(s) features
        protected override String GetLastCharFeatures(String wordName)
        {
            if (wordName != null)
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
                        if ((int)wordName.ToCharArray()[wordName.Length - j] >= 1568 &&
                            (int)wordName.ToCharArray()[wordName.Length - j] <= 1610)
                        {
                            lastCharFeature = lastCharFeature + ((int)wordName.ToCharArray()[wordName.Length - j] % 1568 + 1).ToString() + ",";

                           /* tempLastCharFeature = ((int)FeaturesFormatter.CHAR_FEATURE_BITFIELD_LEN + 1) - ((int)wordName.ToCharArray()[wordName.Length - j] % 1568);
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
        public static int ComputeStringLengthRaw(ConfigurationManager configManager)
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
                            case "DIAC":
                                stringLength += 1;
                                break;
                            case "POS":
                                stringLength += Parser.maxIDs.POS_IDs[1];
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
                                break;
                        }// end switch 
                    }

                    // For POS
                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += Parser.maxIDs.POS_IDs[1];

                    break;
                case "POSAndWord":

                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += Parser.maxIDs.POS_IDs[1];

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += 1;
                                break;
                            case "CharacterLevel":
                                stringLength += Parser.maxIDs.wordLength;
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
                                stringLength += Parser.maxIDs.wordLength;
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
                                stringLength += Parser.maxIDs.wordLength;
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
                    stringLength += Parser.maxIDs.POS_IDs[1];


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
                    stringLength += Parser.maxIDs.POS_IDs[1];
                    
                    break;

            }// end switch
            return stringLength;
        }// end FormatTargetStringFeatures()

        // Method to format the targetString
        // The targetString will be fomratted according to its offset position in the string of features
        // BUT, will not be added to wordFeatures string now. It'll be added when formatting the context, where
        // it should be addded to before and after words but not the central one, which will have 0's
        protected override void FormatTargetStringFeatures(ref WordFeatures wordFeatures)
        {
            String targetString = String.Empty;

            if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
            {
                switch (configManager.targetType)
                {
                    case "DIAC":
                        // No need to add 1, target in Enums.cs starts from 1 already not 0
                        targetString = (wordFeatures.target[0]).ToString() + ",";
                        break;
                    case "POS":
                        foreach (int target in wordFeatures.target)
                        {
                            targetString += (target + 1).ToString() + ",";
                        }

                        // Now Pad the rest of POS field with zeros
                        for (int i = wordFeatures.target.Length + 1; i <= Parser.maxIDs.POS_IDs[1]; i++)
                        {
                            wordFeatures.features += "0,";
                        }//end for
                        
                        break;
                    default:
                        Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
                        break;
                }// end switch    
            }// end if !Suppress("ContextTargets")

            wordFeatures.targetString = targetString;
        }// end class

    }// ComputeStringLengthRaw
    
}
