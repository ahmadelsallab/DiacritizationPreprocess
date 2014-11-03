using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class BitFieldFeaturesFormatter : FeaturesFormatter
    {
        // Constructor
        public BitFieldFeaturesFormatter(ConfigurationManager configManager, Logger logger, Word[] words)
            : base(configManager, logger, words)
        {
            stringLength = ComputeStringLengthBitfield(configManager);
            
            // Form the empty feature string
            emptyFeatureString = EmptyFeatureString();

            // Form the empty target string
            emptyTargetString = EmptyTargetString();

        }


        // Method to put the features in their format
        protected override void FormatMrfWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
            {
                wordFeatures.features = ConvertToBitfieldString(word.mrfType + 1, (uint)Parser.maxIDs.mrfType + 1);
            }

            if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.p + 1, (uint)Parser.maxIDs.p + 1);
            }

            if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.r + 1, (uint)Parser.maxIDs.r + 1);
            }

            if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.f + 1, (uint)Parser.maxIDs.f + 1);
            }

            if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.s + 1, (uint)Parser.maxIDs.s + 1);
            }


            /*wordFeatures.features = ConvertToBitfieldString(word.mrfType, (uint)Parser.maxIDs.mrfType) +
                                    ConvertToBitfieldString(word.p, (uint)Parser.maxIDs.p) +
                                    ConvertToBitfieldString(word.r, (uint)Parser.maxIDs.r) +
                                    ConvertToBitfieldString(word.f, (uint)Parser.maxIDs.f) +
                                    ConvertToBitfieldString(word.s, (uint)Parser.maxIDs.s);*/
        }

        // Method to put the features in their format
        protected override void FormatWordIDWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
            {
                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        wordFeatures.features += ConvertToBitfieldString(word.vocabularyWordID + 1, (uint)Parser.maxIDs.vocabularyWordID + 1);
                        break;
                    case "CharacterLevel":
                        // Loop on characters of the word
                        foreach (char wordChar in word.wordNameWithProperDiacritics)
                        {
                            wordFeatures.features += ConvertToBitfieldString(wordChar % 1568 + 1, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                        }// end foreach

                        // Now, pad the rest of the word to the max word length
                        for (int i = word.wordNameWithProperDiacritics.Length + 1; i <= Parser.maxIDs.wordLength; i++)
                        {
                            wordFeatures.features += ConvertToBitfieldString(0, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                        }//end for

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;

                }//end switch                
            }

        }

        // Method to format the central context word feature
        protected override String FormatCentralContextWordFeatures(Word word, String features)
        {
            String centralWordFeatures = features;       
            if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.mrfType + 1, (uint)Parser.maxIDs.mrfType + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.p + 1, (uint)Parser.maxIDs.p + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.r + 1, (uint)Parser.maxIDs.r + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.f + 1, (uint)Parser.maxIDs.f + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.s + 1, (uint)Parser.maxIDs.s + 1);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        centralWordFeatures += ConvertToBitfieldString(word.vocabularyWordID + 1, (uint)Parser.maxIDs.vocabularyWordID + 1);
                        break;
                    case "CharacterLevel":
                        // Loop on characters of the word
                        foreach (char wordChar in word.wordNameWithProperDiacritics)
                        {
                            centralWordFeatures += ConvertToBitfieldString(wordChar % 1568 + 1, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                        }// end foreach

                        // Now, pad the rest of the word to the max word length
                        for (int i = word.wordNameWithProperDiacritics.Length + 1; i <= Parser.maxIDs.wordLength; i++)
                        {
                            centralWordFeatures += ConvertToBitfieldString(0, FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                        }//end for

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;

                }//end switch  

                
            }

            return centralWordFeatures;
        }

        // Method to check if the wordFeature string conforms to the expected string length or not
        protected override bool IsConformantStringLen(WordFeatures wordFeature)
        {
            // *2 to account for "," after each number
            return ((stringLength * 2) == wordFeature.features.Length ? true : false);

        }// end IsConformantStringLen

        // Utility to compute the required string length in case of Raw features
        public static int ComputeStringLengthBitfield(ConfigurationManager configManager)
        {
            int stringLength = 0;
            switch (configManager.outputFeatures)
            {
                case "All":
                    
                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.s + 1);
                    }

                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += (Parser.maxIDs.vocabularyWordID + 1);
                                break;
                            case "CharacterLevel":
                                stringLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch

                    }

                    break;
                    
                case "POSAndWord":
                    
                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += (Parser.maxIDs.vocabularyWordID + 1);
                                break;
                            case "CharacterLevel":
                                stringLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
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
                        stringLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.s + 1);
                    }

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += (Parser.maxIDs.vocabularyWordID + 1);
                                break;
                            case "CharacterLevel":
                                stringLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
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
                                stringLength += (Parser.maxIDs.vocabularyWordID + 1);
                                break;
                            case "CharacterLevel":
                                stringLength += Parser.maxIDs.wordLength * FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN;
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
                        stringLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.s + 1);
                    }

                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    //stringLength = (Parser.maxIDs.mrfType + 1) + (Parser.maxIDs.p + 1) + (Parser.maxIDs.r + 1) + (Parser.maxIDs.f + 1) + (Parser.maxIDs.s + 1) + (Parser.maxIDs.POS_IDs[0] + 1);                    
                    break;
                case "MrfOnly":
                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.s + 1);
                    }

                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);
                    //stringLength = (Parser.maxIDs.mrfType + 1) + (Parser.maxIDs.p + 1) + (Parser.maxIDs.r + 1) + (Parser.maxIDs.f + 1) + (Parser.maxIDs.s + 1);                    
                    break;
                case "POSOnly":
                    stringLength = (Parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch
            return stringLength;
        }// end ComputeStringLength

        // Method to format the targetString
        protected override void FormatTargetStringFeatures(ref WordFeatures wordFeatures)
        {
            String targetString = "";
            if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
            {
                // Get number of targets
                uint numDiacTargets = (uint)((TargetCode[])Enum.GetValues(typeof(TargetCode))).Length;
                uint numPOSTargets = (uint)Parser.maxIDs.POS_IDs[0] + 1;

                switch (configManager.targetType)
                {
                    case "DIAC":
                        targetString = FeaturesFormatter.ConvertToBitfieldString(wordFeatures.target[0], numDiacTargets);
                        break;
                    case "POS":
                        targetString = FeaturesFormatter.ConvertToBitfieldString(wordFeatures.target, numPOSTargets);
                        break;
                    default:
                        Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
                    case "DIAC":
                        return FeaturesFormatter.ConvertToBitfieldString(0, (uint)((TargetCode[])Enum.GetValues(typeof(TargetCode))).Length);
                    //break;
                    case "POS":
                        return FeaturesFormatter.ConvertToBitfieldString(0, (uint)Parser.maxIDs.POS_IDs[0] + 1);
                    //break;
                    default:
                        Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
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
