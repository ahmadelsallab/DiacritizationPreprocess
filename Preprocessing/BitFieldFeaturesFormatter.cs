using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class BitFieldFeaturesFormatter : FeaturesFormatter
    {
        // Constructor
        public BitFieldFeaturesFormatter(ConfigurationManager configManager, Parser parser, Logger logger, Item[] items)
            : base(configManager, parser, logger, items)
        {
            stringLength = ComputeStringLengthBitfield(configManager, parser);
            
            // Form the empty feature string
            emptyFeatureString = EmptyFeatureString();

            // Form the empty target string
            emptyTargetString = EmptyTargetString();

        }


        // Method to put the features in their format
        protected override void FormatMrfFeatures(Item word, ref Feature wordFeatures)
        {
            if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
            {
                wordFeatures.features = ConvertToBitfieldString(word.mrfType + 1, (uint)parser.maxIDs.mrfType + 1);
            }

            if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.p + 1, (uint)parser.maxIDs.p + 1);
            }

            if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.r + 1, (uint)parser.maxIDs.r + 1);
            }

            if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.f + 1, (uint)parser.maxIDs.f + 1);
            }

            if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.s + 1, (uint)parser.maxIDs.s + 1);
            }


            /*wordFeatures.features = ConvertToBitfieldString(word.mrfType, (uint)parser.maxIDs.mrfType) +
                                    ConvertToBitfieldString(word.p, (uint)parser.maxIDs.p) +
                                    ConvertToBitfieldString(word.r, (uint)parser.maxIDs.r) +
                                    ConvertToBitfieldString(word.f, (uint)parser.maxIDs.f) +
                                    ConvertToBitfieldString(word.s, (uint)parser.maxIDs.s);*/
        }

        // Method to put the features in their format
        protected override void FormatItemIDFeatures(Item item, ref Feature wordFeatures)
        {
            if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
            {
                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        if (item != null)
                        {
                            wordFeatures.features += ConvertToBitfieldString(item.vocabularyWordID + 1, (uint)parser.maxIDs.vocabularyWordID + 1);
                        }
                        else
                        {
                            wordFeatures.features += ConvertToBitfieldString(0, (uint)parser.maxIDs.vocabularyWordID + 1);
                        }
                        break;
                    case "CharacterLevel":
                        if (item != null)
                        {
                            // Loop on characters of the word
                            foreach (char wordChar in item.itemNameWithProperDiacritics)
                            {
                                wordFeatures.features += ConvertToBitfieldString(wordChar % 1568 + 1, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                            }// end foreach

                            // Now, pad the rest of the word to the max word length
                            for (int i = item.itemNameWithProperDiacritics.Length + 1; i <= parser.maxIDs.wordLength; i++)
                            {
                                wordFeatures.features += ConvertToBitfieldString(0, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                            }//end for
                        }
                        else
                        {
                            // Log empty features
                            for (int i = 0; i <= parser.maxIDs.wordLength; i++)
                            {
                                wordFeatures.features += ConvertToBitfieldString(0, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                            }//end for
                        }

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;

                }//end switch                
            }

        }

        // Method to put the word name feature in its char level encoded format
        protected override String FormatWordFeaturesCharLevel(String wordName)
        {
            String features = String.Empty;
            if (wordName != null)
            {
                // Loop on characters of the word
                foreach (char wordChar in wordName)
                {
                    features += ConvertToBitfieldString(wordChar % START_OF_ARABIC_CHAR_ASCII_CODE + 1, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                }// end foreach

                // Now, pad the rest of the word to the max word length
                for (int i = wordName.Length + 1; i <= MAX_WORD_LEN; i++)
                {
                    features += ConvertToBitfieldString(0, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                }//end for
            }
            else
            {
                // Log empty features
                for (int i = 0; i < MAX_WORD_LEN; i++)
                {
                    features += ConvertToBitfieldString(0, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                }//end for
            }

            return features;
        }

        // Method to format the central context word feature
        protected override String FormatCentralContextFeatures(Item item, String features)
        {
            String centralWordFeatures = features;       
            if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(item.mrfType + 1, (uint)parser.maxIDs.mrfType + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(item.p + 1, (uint)parser.maxIDs.p + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(item.r + 1, (uint)parser.maxIDs.r + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(item.f + 1, (uint)parser.maxIDs.f + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(item.s + 1, (uint)parser.maxIDs.s + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        centralWordFeatures += ConvertToBitfieldString(item.vocabularyWordID + 1, (uint)parser.maxIDs.vocabularyWordID + 1);
                        break;
                    case "CharacterLevel":
                        // Loop on characters of the word
                        foreach (char wordChar in item.itemNameWithProperDiacritics)
                        {
                            centralWordFeatures += ConvertToBitfieldString(wordChar % 1568 + 1, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                        }// end foreach

                        // Now, pad the rest of the word to the max word length
                        for (int i = item.itemNameWithProperDiacritics.Length + 1; i <= parser.maxIDs.wordLength; i++)
                        {
                            centralWordFeatures += ConvertToBitfieldString(0, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                        }//end for

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;

                }//end switch  

                
            }

            if ((String)configManager.addFeaturesToCentralContextWord["CharPosition"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(item.charPosition + 1, (uint)parser.maxIDs.wordLength);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["WordContext"] == "Add")
            {
                centralWordFeatures += ExtractCentralWordContextFeatures(item);
            }

            return centralWordFeatures;
        }

        // Method to check if the wordFeature string conforms to the expected string length or not
        protected override bool IsConformantStringLen(Feature wordFeature)
        {
            // *2 to account for "," after each number
            return ((stringLength * 2) == wordFeature.features.Length ? true : false);

        }// end IsConformantStringLen

        // Utility to compute the required string length in case of Raw features
        public static int ComputeStringLengthBitfield(ConfigurationManager configManager, Parser parser)
        {
            int stringLength = 0;
            switch (configManager.outputFeatures)
            {
                case "All":
                    
                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.s + 1);
                    }

                    stringLength += (parser.maxIDs.POS_IDs[0] + 1);

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += (parser.maxIDs.vocabularyWordID + 1);
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch

                    }

                    break;
                    
                case "POSAndWord":
                    
                    stringLength += (parser.maxIDs.POS_IDs[0] + 1);

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += (parser.maxIDs.vocabularyWordID + 1);
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
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
                        stringLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.s + 1);
                    }

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += (parser.maxIDs.vocabularyWordID + 1);
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
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
                                stringLength += (parser.maxIDs.vocabularyWordID + 1);
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch

                    }

                    break;

                case "MrfAndPOS":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.s + 1);
                    }

                    stringLength += (parser.maxIDs.POS_IDs[0] + 1);

                    //stringLength = (parser.maxIDs.mrfType + 1) + (parser.maxIDs.p + 1) + (parser.maxIDs.r + 1) + (parser.maxIDs.f + 1) + (parser.maxIDs.s + 1) + (parser.maxIDs.POS_IDs[0] + 1);                    
                    break;
                case "MrfOnly":
                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = (parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (parser.maxIDs.s + 1);
                    }

                    stringLength += (parser.maxIDs.POS_IDs[0] + 1);
                    //stringLength = (parser.maxIDs.mrfType + 1) + (parser.maxIDs.p + 1) + (parser.maxIDs.r + 1) + (parser.maxIDs.f + 1) + (parser.maxIDs.s + 1);                    
                    break;
                case "POSOnly":
                    stringLength = (parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch
            return stringLength;
        }// end ComputeStringLength

        // Method to format the targetString
        protected override void FormatTargetStringFeatures(ref Feature wordFeatures)
        {
            String targetString = "";
            if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
            {
                switch (configManager.targetType)
                {
                    case "SYNT_DIAC":
                    case "FULL_DIAC":
                    case "ClassifySyntDiac":
                        uint numDiacTargets = (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length;
                        targetString = FeaturesFormatter.ConvertToBitfieldString(wordFeatures.target[0], numDiacTargets);
                        break;
                    case "POS":
                        uint numPOSTargets;
                        switch (configManager.targetMode)
                        {
                            case "Single":
                                numPOSTargets = (uint)((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length;
                                targetString = FeaturesFormatter.ConvertToBitfieldString(wordFeatures.target[0], numPOSTargets);
                                break;
                            case "Multiple":
                                numPOSTargets = (uint)parser.maxIDs.POS_IDs[0] + 1;
                                targetString = FeaturesFormatter.ConvertToBitfieldString(wordFeatures.target, numPOSTargets);
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
            }// end if !Suppress("ContextTargets")

            wordFeatures.targetString = targetString;
        }// end FormatTargetStringFeatures()

        // Utility to return empty target string
        protected override String EmptyTargetString()
        {
            if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
            {

                switch (configManager.targetType)
                {
                    case "SYNT_DIAC":
                    case "FULL_DIAC":
                    case "ClassifySyntDiac":
                        return FeaturesFormatter.ConvertToBitfieldString(0, (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length);
                    //break;
                    case "POS":
                        switch (configManager.targetMode)
                        {
                            case "Single":
                                return FeaturesFormatter.ConvertToBitfieldString(0, (uint)((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length);
                                //break;
                            case "Multiple":
                                return FeaturesFormatter.ConvertToBitfieldString(0, (uint)parser.maxIDs.POS_IDs[0] + 1);
                                //break;
                            default:
                                Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                return null;
                                //break;
                        }// end switch (configManager.targetMode)  
                        
                    //break;
                    default:
                        Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                        return null;
                    //break;
                }// end switch    
            }// end if !Suppress("ContextTargets")
            else
            {
                return null;
            }


        }// end EmptyFeatureString()
    }
}
