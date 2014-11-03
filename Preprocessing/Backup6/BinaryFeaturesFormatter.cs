using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class BinaryFeaturesFormatter : FeaturesFormatter
    {
        // Constructor
        public BinaryFeaturesFormatter(ConfigurationManager configManager, Logger logger, Word[] words)
            : base(configManager, logger, words)
        {
            stringLength = ComputeStringLengthBinary(configManager);

            // Form the empty feature string
            emptyFeatureString = EmptyFeatureString();
        }


        // Method to put the features in their format
        protected override void FormatMrfWordFeatures(Word word, ref WordFeatures wordFeatures)
        {

            if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
            {
                wordFeatures.features = GetIntBinaryString(word.mrfType, Parser.GetNumBits(Parser.maxIDs.mrfType));
            }

            if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.p, Parser.GetNumBits(Parser.maxIDs.p));
            }

            if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.r, Parser.GetNumBits(Parser.maxIDs.r));
            }

            if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.f, Parser.GetNumBits(Parser.maxIDs.f));
            }

            if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.s, Parser.GetNumBits(Parser.maxIDs.s));
            }


            /*wordFeatures.features = GetIntBinaryString(word.mrfType, Parser.GetNumBits(Parser.maxIDs.mrfType)) +
                                    GetIntBinaryString(word.p, Parser.GetNumBits(Parser.maxIDs.p)) +
                                    GetIntBinaryString(word.r, Parser.GetNumBits(Parser.maxIDs.r)) +
                                    GetIntBinaryString(word.f, Parser.GetNumBits(Parser.maxIDs.f)) +
                                    GetIntBinaryString(word.s, Parser.GetNumBits(Parser.maxIDs.s));*/

            /*String s = GetIntBinaryString(word.mrfType, Parser.GetNumBits(Parser.maxIDs.mrfType));
            s = GetIntBinaryString(word.p, Parser.GetNumBits(Parser.maxIDs.p)); 
            s = GetIntBinaryString(word.r, Parser.GetNumBits(Parser.maxIDs.r));
            s = GetIntBinaryString(word.f, Parser.GetNumBits(Parser.maxIDs.f));
            s = GetIntBinaryString(word.s, Parser.GetNumBits(Parser.maxIDs.s));*/

        } // end FormatMrfWordFeatures

        // Method to put the features in their format
        protected override void FormatWordIDWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
            {
                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        wordFeatures.features += GetIntBinaryString(word.vocabularyWordID, Parser.GetNumBits(Parser.maxIDs.vocabularyWordID));
                        break;
                    case "CharacterLevel":
                        // Loop on characters of the word
                        foreach (char wordChar in word.wordNameWithProperDiacritics)
                        {
                            wordFeatures.features += GetIntBinaryString(wordChar % 1568 + 1, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                        }// end foreach

                        // Now, pad the rest of the word to the max word length
                        for (int i = word.wordNameWithProperDiacritics.Length + 1; i <= Parser.maxIDs.wordLength; i++)
                        {
                            wordFeatures.features += GetIntBinaryString(0, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                        }//end for

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;

                }//end switch                 
            }
        }

        // Override the POS word features format
        protected override void FormatPOSWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            wordFeatures.features += GetIntBinaryString((int)word.equivalentPOS_ID, Parser.GetNumBits(Parser.maxIDs.vocabularyWordID));
                //(word.equivalentPOS_ID / Parser.maxIDs.equivalentPOS_ID).ToString() + ",";
        }

        // Method to format the central context word feature
        protected override String FormatCentralContextWordFeatures(Word word, String features)
        {
            String centralWordFeatures = features;
            if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.mrfType, Parser.GetNumBits(Parser.maxIDs.mrfType));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.p, Parser.GetNumBits(Parser.maxIDs.p));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.r, Parser.GetNumBits(Parser.maxIDs.r));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.f, Parser.GetNumBits(Parser.maxIDs.f));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                centralWordFeatures += GetIntBinaryString(word.s, Parser.GetNumBits(Parser.maxIDs.s));
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                switch (configManager.wordOnlyEncoding)
                {
                    case "WordLevel":
                        centralWordFeatures += GetIntBinaryString(word.vocabularyWordID, Parser.GetNumBits(Parser.maxIDs.vocabularyWordID));
                        break;
                    case "CharacterLevel":
                        // Loop on characters of the word
                        foreach (char wordChar in word.wordNameWithProperDiacritics)
                        {
                            centralWordFeatures += GetIntBinaryString(wordChar % 1568 + 1, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                        }// end foreach

                        // Now, pad the rest of the word to the max word length
                        for (int i = word.wordNameWithProperDiacritics.Length + 1; i <= Parser.maxIDs.wordLength; i++)
                        {
                            centralWordFeatures += GetIntBinaryString(0, Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN));
                        }//end for

                        break;
                    default:
                        Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                        break;

                }//end switch                
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
        protected override bool IsConformantStringLen(WordFeatures wordFeature)
        {
            // *2 to account for "," after each number
            return (((stringLength * 2) == wordFeature.features.Length) ? true : false);

        }// end IsConformantStringLen

        // Utility to compute the required string length in case of Raw features
        public static int ComputeStringLengthBinary(ConfigurationManager configManager)
        {
            int stringLength = 0;
            switch (configManager.outputFeatures)
            {
                case "All":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = Parser.GetNumBits(Parser.maxIDs.mrfType);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.p);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.r);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.f);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.s);
                    }

                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    // Add the word only ID
                    if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
                    {
                        switch (configManager.wordOnlyEncoding)
                        {
                            case "WordLevel":
                                stringLength += Parser.GetNumBits(Parser.maxIDs.vocabularyWordID);
                                break;
                            case "CharacterLevel":
                                stringLength += Parser.maxIDs.wordLength * Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                                break;
                            default:
                                Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                                break;

                        }//end switch

                    }

                    break;

                case "POSAndWord":

                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            stringLength += Parser.GetNumBits(Parser.maxIDs.vocabularyWordID);
                            break;
                        case "CharacterLevel":
                            stringLength += Parser.maxIDs.wordLength * Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "MrfAndWord":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = Parser.GetNumBits(Parser.maxIDs.mrfType);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.p);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.r);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.f);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.s);
                    }

                    switch (configManager.wordOnlyEncoding)
                    {
                        case "WordLevel":
                            stringLength += Parser.GetNumBits(Parser.maxIDs.vocabularyWordID);
                            break;
                        case "CharacterLevel":
                            stringLength += Parser.maxIDs.wordLength * Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
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
                            stringLength += Parser.GetNumBits(Parser.maxIDs.vocabularyWordID);
                            break;
                        case "CharacterLevel":
                            stringLength += Parser.maxIDs.wordLength * Parser.GetNumBits(FeaturesFormatter.CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN);
                            break;
                        default:
                            Console.WriteLine("Incorrect WordOnlyEncoding configuration. {0} is invalid configuration. Valid configurations are: WordLevel or CharacterLevel.", configManager.wordOnlyEncoding);
                            break;

                    }//end switch

                    break;

                case "MrfAndPOS":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = Parser.GetNumBits(Parser.maxIDs.mrfType);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.p);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.r);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.f);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.s);
                    }

                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    /*stringLength = (Parser.GetNumBits(Parser.maxIDs.mrfType) +
                                    Parser.GetNumBits(Parser.maxIDs.p) +
                                    Parser.GetNumBits(Parser.maxIDs.r) +
                                    Parser.GetNumBits(Parser.maxIDs.f) +
                                    Parser.GetNumBits(Parser.maxIDs.s) +
                                    Parser.maxIDs.POS_IDs[0] + 1);*/
                    break;
                case "MrfOnly":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = Parser.GetNumBits(Parser.maxIDs.mrfType);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.p);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.r);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.f);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.s);
                    }

                    /*stringLength = (Parser.GetNumBits(Parser.maxIDs.mrfType) + 
                                    Parser.GetNumBits(Parser.maxIDs.p) + 
                                    Parser.GetNumBits(Parser.maxIDs.r) + 
                                    Parser.GetNumBits(Parser.maxIDs.f) + 
                                    Parser.GetNumBits(Parser.maxIDs.s));*/
                    break;
                case "POSOnly":
                    stringLength = (Parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch
            return stringLength;
        }// end ComputeStringLength

    }
}
