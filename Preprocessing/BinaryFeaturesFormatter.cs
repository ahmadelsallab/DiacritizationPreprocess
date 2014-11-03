using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class BinaryFeaturesFormatter : FeaturesFormatter
    {
        // Constructor
        public BinaryFeaturesFormatter(ConfigurationManager configManager, Parser parser, Logger logger, Item[] items)
            : base(configManager, parser, logger, items)
        {
            stringLength = ComputeStringLengthBinary(configManager, parser);

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
                wordFeatures.features = GetIntBinaryString(word.mrfType, Parser.GetNumBits(parser.maxIDs.mrfType));
            }

            if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.p, Parser.GetNumBits(parser.maxIDs.p));
            }

            if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.r, Parser.GetNumBits(parser.maxIDs.r));
            }

            if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.f, Parser.GetNumBits(parser.maxIDs.f));
            }

            if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.s, Parser.GetNumBits(parser.maxIDs.s));
            }


            /*wordFeatures.features = GetIntBinaryString(word.mrfType, Parser.GetNumBits(parser.maxIDs.mrfType)) +
                                    GetIntBinaryString(word.p, Parser.GetNumBits(parser.maxIDs.p)) +
                                    GetIntBinaryString(word.r, Parser.GetNumBits(parser.maxIDs.r)) +
                                    GetIntBinaryString(word.f, Parser.GetNumBits(parser.maxIDs.f)) +
                                    GetIntBinaryString(word.s, Parser.GetNumBits(parser.maxIDs.s));*/

            /*String s = GetIntBinaryString(word.mrfType, Parser.GetNumBits(parser.maxIDs.mrfType));
            s = GetIntBinaryString(word.p, Parser.GetNumBits(parser.maxIDs.p)); 
            s = GetIntBinaryString(word.r, Parser.GetNumBits(parser.maxIDs.r));
            s = GetIntBinaryString(word.f, Parser.GetNumBits(parser.maxIDs.f));
            s = GetIntBinaryString(word.s, Parser.GetNumBits(parser.maxIDs.s));*/

        } // end FormatMrfFeatures

        // Method to put the features in their format
        protected override void FormatItemIDFeatures(Item word, ref Feature wordFeatures)
        {
            if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
            {
                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        if (word != null)
                        {
                            wordFeatures.features += GetIntBinaryString(word.vocabularyWordID, Parser.GetNumBits(parser.maxIDs.vocabularyWordID));
                        }
                        else
                        {
                            wordFeatures.features += GetIntBinaryString(0, Parser.GetNumBits(parser.maxIDs.vocabularyWordID));
                        }
                        break;
                    case "CharacterLevel":
                        if (word != null)
                        {
                            // Loop on characters of the word
                            foreach (char wordChar in word.itemNameWithProperDiacritics)
                            {
                                wordFeatures.features += GetIntBinaryString(wordChar % 1568 + 1, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                            }// end foreach

                            // Now, pad the rest of the word to the max word length
                            for (int i = word.itemNameWithProperDiacritics.Length + 1; i <= parser.maxIDs.wordLength; i++)
                            {
                                wordFeatures.features += GetIntBinaryString(0, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                            }//end for
                        }
                        else
                        {
                            // Log empty features
                            for (int i = 0; i <= parser.maxIDs.wordLength; i++)
                            {
                                wordFeatures.features += GetIntBinaryString(0, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                            }//end for
                        }

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;

                }//end switch                 
            }
        } // FormatItemIDFeatures

        // Method to put the word name feature in its char level encoded format
        protected override String FormatWordFeaturesCharLevel(String wordName)
        {
            String features = String.Empty;

            if (wordName != null)
            {
                // Loop on characters of the word
                foreach (char wordChar in wordName)
                {
                    features += GetIntBinaryString(wordChar % START_OF_ARABIC_CHAR_ASCII_CODE + 1, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                }// end foreach

                // Now, pad the rest of the word to the max word length
                for (int i = wordName.Length + 1; i <= MAX_WORD_LEN; i++)
                {
                    features += GetIntBinaryString(0, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                }//end for
            }
            else
            {
                // Log empty features
                for (int i = 0; i < MAX_WORD_LEN; i++)
                {
                    features += GetIntBinaryString(0, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                }//end for
            }
            return features;         
            
        }

        // Override the POS word features format
        protected override void FormatPOSFeatures(Item word, ref Feature wordFeatures)
        {
            wordFeatures.features += GetIntBinaryString((int)word.equivalentPOS_ID, Parser.GetNumBits(parser.maxIDs.vocabularyWordID));
                //(word.equivalentPOS_ID / parser.maxIDs.equivalentPOS_ID).ToString() + ",";
        }

        // Method to format the central context word feature
        protected override String FormatCentralContextFeatures(Item word, String features)
        {
            String centralWordFeatures = features;
            if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.mrfType, Parser.GetNumBits(parser.maxIDs.mrfType));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.p, Parser.GetNumBits(parser.maxIDs.p));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.r, Parser.GetNumBits(parser.maxIDs.r));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.f, Parser.GetNumBits(parser.maxIDs.f));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.s, Parser.GetNumBits(parser.maxIDs.s));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        centralWordFeatures += GetIntBinaryString(word.vocabularyWordID, Parser.GetNumBits(parser.maxIDs.vocabularyWordID));
                        break;
                    case "CharacterLevel":
                        // Loop on characters of the word
                        foreach (char wordChar in word.itemNameWithProperDiacritics)
                        {
                            centralWordFeatures += GetIntBinaryString(wordChar % 1568 + 1, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                        }// end foreach

                        // Now, pad the rest of the word to the max word length
                        for (int i = word.itemNameWithProperDiacritics.Length + 1; i <= parser.maxIDs.wordLength; i++)
                        {
                            centralWordFeatures += GetIntBinaryString(0, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                        }//end for

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;

                }//end switch                
            }

            if ((String)configManager.addFeaturesToCentralContextWord["CharPosition"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.charPosition, Parser.GetNumBits(parser.maxIDs.wordLength));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["WordContext"] == "Add")
            {
                centralWordFeatures += ExtractCentralWordContextFeatures(word);
            }

            return centralWordFeatures;
        }

        // Utility to convert a number to its binary representation as comma separated string
        private string GetIntBinaryString(int n, int length)
        {
            char[] b = new char[2*length];
            int pos = 2*length - 1;
            int i = 0;

            while (i < length)
            {
                if ((n & (1 << i)) != 0)
                {
                    b[pos] = ',';
                    pos--;
                    b[pos] = '1';
                    
                }
                else
                {
                    b[pos] = ',';
                    pos--;
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        } // end GetIntBinaryString

        // Method to check if the wordFeature string conforms to the expected string length or not
        protected override bool IsConformantStringLen(Feature wordFeature)
        {
            // *2 to account for "," after each number
            return (((stringLength * 2) == wordFeature.features.Length) ? true : false);

        }// end IsConformantStringLen

        // Utility to compute the required string length in case of Raw features
        public static int ComputeStringLengthBinary(ConfigurationManager configManager, Parser parser)
        {
            int stringLength = 0;
            switch (configManager.outputFeatures)
            {
                case "All":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = Parser.GetNumBits(parser.maxIDs.mrfType);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.p);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.r);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.f);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.s);
                    }

                    stringLength += (parser.maxIDs.POS_IDs[0] + 1);

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += Parser.GetNumBits(parser.maxIDs.vocabularyWordID);
                                break;
                            case "CharacterLevel":
                                stringLength += parser.maxIDs.wordLength * Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch

                    }

                    break;

                case "POSAndWord":

                    stringLength += (parser.maxIDs.POS_IDs[0] + 1);

                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            stringLength += Parser.GetNumBits(parser.maxIDs.vocabularyWordID);
                            break;
                        case "CharacterLevel":
                            stringLength += parser.maxIDs.wordLength * Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "MrfAndWord":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = Parser.GetNumBits(parser.maxIDs.mrfType);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.p);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.r);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.f);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.s);
                    }

                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            stringLength += Parser.GetNumBits(parser.maxIDs.vocabularyWordID);
                            break;
                        case "CharacterLevel":
                            stringLength += parser.maxIDs.wordLength * Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "WordOnly":

                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            stringLength += Parser.GetNumBits(parser.maxIDs.vocabularyWordID);
                            break;
                        case "CharacterLevel":
                            stringLength += parser.maxIDs.wordLength * Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "MrfAndPOS":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = Parser.GetNumBits(parser.maxIDs.mrfType);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.p);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.r);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.f);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.s);
                    }

                    stringLength += (parser.maxIDs.POS_IDs[0] + 1);

                    /*stringLength = (Parser.GetNumBits(parser.maxIDs.mrfType) +
                                    Parser.GetNumBits(parser.maxIDs.p) +
                                    Parser.GetNumBits(parser.maxIDs.r) +
                                    Parser.GetNumBits(parser.maxIDs.f) +
                                    Parser.GetNumBits(parser.maxIDs.s) +
                                    parser.maxIDs.POS_IDs[0] + 1);*/
                    break;
                case "MrfOnly":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = Parser.GetNumBits(parser.maxIDs.mrfType);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.p);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.r);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.f);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(parser.maxIDs.s);
                    }

                    /*stringLength = (Parser.GetNumBits(parser.maxIDs.mrfType) + 
                                    Parser.GetNumBits(parser.maxIDs.p) + 
                                    Parser.GetNumBits(parser.maxIDs.r) + 
                                    Parser.GetNumBits(parser.maxIDs.f) + 
                                    Parser.GetNumBits(parser.maxIDs.s));*/
                    break;
                case "POSOnly":
                    stringLength = (parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch
            return stringLength;
        }// end ComputeStringLength

        // Method to put the features in their format
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
                        int maxDiacTargetValue = (int)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Max();// -1 remove DEFAULT
                        targetString = GetIntBinaryString(wordFeatures.target[0], Parser.GetNumBits(maxDiacTargetValue));
                        break;
                    case "POS":
                        int maxPOSTargetValue;
                        switch (configManager.targetMode)
                        {
                            case "Single":
                                maxPOSTargetValue = (int)((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Max();// -1 remove DEFAULT
                                targetString = GetIntBinaryString(wordFeatures.target[0], Parser.GetNumBits(maxPOSTargetValue));
 
                                break;
                            case "Multiple":
                                maxPOSTargetValue = parser.maxIDs.POS_IDs[1];
                                foreach (int target in wordFeatures.target)
                                {
                                    targetString += GetIntBinaryString(target, Parser.GetNumBits(maxPOSTargetValue));
                                }// end foreach

                                break;
                            default:
                                Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                break;
                        }// end switch (configManager.targetMode)  


                        //wordFeatures.features += GetIntBinaryString((int)word.equivalentPOS_ID, Parser.GetNumBits(parser.maxIDs.vocabularyWordID));
                        break;
                    default:
                        Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                        break;
                }// end switch               

            }
            wordFeatures.targetString = targetString;
        } // FormatTargetStringFeatures

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
                        return GetIntBinaryString(0, Parser.GetNumBits((int)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Max()));
                    //break;
                    case "POS":
                        switch (configManager.targetMode)
                        {
                            case "Single":
                                return GetIntBinaryString(0, Parser.GetNumBits((int)((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Max()));
                                //break;
                            case "Multiple":
                                return GetIntBinaryString(0, Parser.GetNumBits(parser.maxIDs.POS_IDs[1]));
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
