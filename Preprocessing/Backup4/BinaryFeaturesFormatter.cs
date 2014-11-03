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
            stringLength = ComputeStringLength(configManager);

            // Form the empty feature string
            emptyFeatureString = EmptyFeatureString();
        }


        // Method to put the features in their format
        protected override void FormatMrfWordFeatures(Word word, ref WordFeatures wordFeatures)
        {

            if ((String)configManager.mrfSuppressFeaturesHashTable["mrfType"] != "Suppress")
            {
                wordFeatures.features = GetIntBinaryString(word.mrfType, Parser.GetNumBits(Parser.maxIDs.mrfType));
            }

            if ((String)configManager.mrfSuppressFeaturesHashTable["p"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.p, Parser.GetNumBits(Parser.maxIDs.p));
            }

            if ((String)configManager.mrfSuppressFeaturesHashTable["r"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.r, Parser.GetNumBits(Parser.maxIDs.r));
            }

            if ((String)configManager.mrfSuppressFeaturesHashTable["f"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + GetIntBinaryString(word.f, Parser.GetNumBits(Parser.maxIDs.f));
            }

            if ((String)configManager.mrfSuppressFeaturesHashTable["s"] != "Suppress")
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
                case "MrfAndPOS":
                    if ((String)configManager.mrfSuppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = Parser.GetNumBits(Parser.maxIDs.mrfType);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.p);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.r);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.f);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["s"] != "Suppress")
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

                    if ((String)configManager.mrfSuppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = Parser.GetNumBits(Parser.maxIDs.mrfType);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.p);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.r);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += Parser.GetNumBits(Parser.maxIDs.f);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["s"] != "Suppress")
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
