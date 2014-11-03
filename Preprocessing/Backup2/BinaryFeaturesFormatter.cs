using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class BinaryFeaturesFormatter : FeaturesFormatter
    {
        // Constructor
        public BinaryFeaturesFormatter(ConfigurationManager configManager, Logger logger, Parser parser, Word[] words)
            : base(configManager, logger, parser, words)
        {
            switch (configManager.outputFeatures)
            {
                case "MrfAndPOS":
                    stringLength = (parser.GetNumBits(parser.maxIDs.mrfType) +
                                    parser.GetNumBits(parser.maxIDs.p) +
                                    parser.GetNumBits(parser.maxIDs.r) +
                                    parser.GetNumBits(parser.maxIDs.f) +
                                    parser.GetNumBits(parser.maxIDs.s) +
                                    parser.maxIDs.POS_IDs[0] + 1);
                    break;
                case "MrfOnly":
                    stringLength = (parser.GetNumBits(parser.maxIDs.mrfType) + 
                                    parser.GetNumBits(parser.maxIDs.p) + 
                                    parser.GetNumBits(parser.maxIDs.r) + 
                                    parser.GetNumBits(parser.maxIDs.f) + 
                                    parser.GetNumBits(parser.maxIDs.s));
                    break;
                case "POSOnly":
                    stringLength = (parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch

            // Form the empty feature string
            emptyFeatureString = EmptyFeatureString();
        }
        // Method to put the features in their format
        protected override void FormatMrfWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            wordFeatures.features = GetIntBinaryString(word.mrfType, parser.GetNumBits(parser.maxIDs.mrfType)) +
                                    GetIntBinaryString(word.p, parser.GetNumBits(parser.maxIDs.p)) +
                                    GetIntBinaryString(word.r, parser.GetNumBits(parser.maxIDs.r)) +
                                    GetIntBinaryString(word.f, parser.GetNumBits(parser.maxIDs.f)) +
                                    GetIntBinaryString(word.s, parser.GetNumBits(parser.maxIDs.s));
            /*String s = GetIntBinaryString(word.mrfType, parser.GetNumBits(parser.maxIDs.mrfType));
            s = GetIntBinaryString(word.p, parser.GetNumBits(parser.maxIDs.p)); 
            s = GetIntBinaryString(word.r, parser.GetNumBits(parser.maxIDs.r));
            s = GetIntBinaryString(word.f, parser.GetNumBits(parser.maxIDs.f));
            s = GetIntBinaryString(word.s, parser.GetNumBits(parser.maxIDs.s));*/

        } // end FormatMrfWordFeatures

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

    }
}
